using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using Hoplon.GadgetScript;
using Pocketverse;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public class GadgetEvent : IHMMEventContext, IBitStreamSerializable, IEventContext
	{
		private GadgetEvent()
		{
		}

		public static GadgetEvent GetInstance(int blockIndex, IHMMGadgetContext context)
		{
			GadgetEvent gadgetEvent;
			if (GadgetEvent.Pool.Count > 0)
			{
				gadgetEvent = GadgetEvent.Pool.Pop();
				GadgetEvent._freedEvents.Remove(gadgetEvent);
			}
			else
			{
				gadgetEvent = new GadgetEvent();
			}
			gadgetEvent._gadgetContext = context;
			gadgetEvent.Id = gadgetEvent._gadgetContext.GetNewEventId();
			gadgetEvent.BlockIndex = blockIndex;
			gadgetEvent.CreationTime = gadgetEvent._gadgetContext.CurrentTime;
			gadgetEvent.ShouldBeSent = false;
			gadgetEvent.PreviousEventId = -1;
			return gadgetEvent;
		}

		public static GadgetEvent GetInstance(int blockIndex, IHMMGadgetContext context, List<BaseParameter> parameters)
		{
			GadgetEvent instance = GadgetEvent.GetInstance(blockIndex, context);
			instance._numInitialParameters = parameters.Count;
			for (int i = 0; i < parameters.Count; i++)
			{
				parameters[i].WriteToBitStreamWithContentId(context, instance._parametersStream);
			}
			return instance;
		}

		public static void Free(GadgetEvent gadgetEvent)
		{
			if (GadgetEvent._freedEvents.Contains(gadgetEvent))
			{
				GadgetEvent.Log.ErrorFormat("Trying to remove GadgetEvent twice! GadgetContext: {0} Block: {1}", new object[]
				{
					gadgetEvent._gadgetContext,
					gadgetEvent.RootBlock
				});
				return;
			}
			GadgetEvent._freedEvents.Add(gadgetEvent);
			for (int i = 0; i < gadgetEvent._executionPointsWithInnerEvent.Count; i++)
			{
				int key = gadgetEvent._executionPointsWithInnerEvent[i];
				for (int j = 0; j < gadgetEvent._innerEvents[key].Count; j++)
				{
					IEventContext eventContext = gadgetEvent._innerEvents[key][j];
					GadgetEvent.Free((GadgetEvent)eventContext);
				}
			}
			GadgetEvent.Clear(gadgetEvent);
			GadgetEvent.Pool.Push(gadgetEvent);
		}

		private static void Clear(GadgetEvent gadgetEvent)
		{
			gadgetEvent._parentEvent = null;
			gadgetEvent._executedBlock = 0;
			gadgetEvent._innerEventsExecutedInPoint = 0;
			gadgetEvent._bodies.Clear();
			gadgetEvent._removedBodies.Clear();
			gadgetEvent._parametersStream.ResetBitsRead();
			gadgetEvent._parametersStream.ResetBitsWritten();
			gadgetEvent._executionPointsWithInnerEvent.Clear();
			gadgetEvent._innerEvents.Clear();
			gadgetEvent._numInitialParameters = 0;
			if (gadgetEvent._byteArrayData != null)
			{
				gadgetEvent._parametersStream.FreeCachedByteArray(gadgetEvent._byteArrayData);
			}
		}

		public int Id { get; private set; }

		public int BlockIndex { get; private set; }

		public IBlock RootBlock
		{
			get
			{
				return BaseBlock.GetBlock(this.BlockIndex);
			}
		}

		public int CreationTime { get; set; }

		public bool ShouldBeSent { get; private set; }

		public int PreviousEventId { get; private set; }

		public IEventContext ParentEvent
		{
			get
			{
				return this._parentEvent;
			}
		}

		public bool ConsumeBody()
		{
			if (this._bodies.Count == 0)
			{
				return false;
			}
			int num = this._bodies[0];
			this._bodies.RemoveAt(0);
			this._gadgetContext.SetLastBodyId(num);
			int num2;
			bool flag = this._gadgetContext.TryGetBodyDestructionTime(num, out num2);
			bool flag2 = flag && num2 < this._gadgetContext.CurrentTime;
			return !this._gadgetContext.Bodies.ContainsKey(num) && !flag2;
		}

		public void LoadInitialParameters()
		{
			for (int i = 0; i < this._numInitialParameters; i++)
			{
				BaseParameter.ReadParameterFromBitStreamWithContentId(this._gadgetContext, this._parametersStream);
			}
		}

		public void SaveParameter(BaseParameter parameter)
		{
			parameter.WriteToBitStreamWithContentId(this._gadgetContext, this._parametersStream);
		}

		public void LoadParameter(BaseParameter outputParameter)
		{
			outputParameter.ReadFromBitStreamWithContentId(this._gadgetContext, this._parametersStream);
		}

		public void SetPreviousEventId(int eventId)
		{
			if (this._parentEvent != null)
			{
				((IHMMEventContext)this._parentEvent).SetPreviousEventId(eventId);
			}
			else if (this.PreviousEventId != -1 && this.PreviousEventId != eventId && eventId != this.Id)
			{
				GadgetEvent.Log.WarnFormat("Trying to set LastFrameId when it was already set. (Is event trying to destroy multiple Bodies from different events?) Gadget={0} - Root Block={1} LastFrameId={2} | Current Event Id: {3} | Trying to set to:{4}", new object[]
				{
					((BaseGadget)this._gadgetContext).name,
					((BaseBlock)BaseBlock.GetBlock(this.BlockIndex)).name,
					this.PreviousEventId,
					this.Id,
					eventId
				});
			}
			else if (eventId != this.Id)
			{
				this.PreviousEventId = eventId;
			}
		}

		public void SendToClient()
		{
			this.ShouldBeSent = true;
			foreach (int key in this._bodies)
			{
				IGadgetBody gadgetBody;
				if (this._gadgetContext.Bodies.TryGetValue(key, out gadgetBody))
				{
					gadgetBody.WasSentToClient = true;
				}
			}
		}

		public void SetParentEvent(IEventContext eventContext)
		{
			this._parentEvent = eventContext;
			this.Id = this._parentEvent.Id;
		}

		public void AddInnerEvent(IEventContext eventContext)
		{
			if (!this._innerEvents.ContainsKey(this._executedBlock))
			{
				this._innerEvents.Add(this._executedBlock, new List<IEventContext>());
				this._executionPointsWithInnerEvent.Add(this._executedBlock);
			}
			this._innerEvents[this._executedBlock].Add(eventContext);
		}

		public IList<IEventContext> GetInnerEvents()
		{
			List<IEventContext> result;
			this._innerEvents.TryGetValue(this._executedBlock, out result);
			return result;
		}

		public void BlockExecuted()
		{
			this._executedBlock++;
		}

		public void AddBody(int bodyId)
		{
			this._bodies.Add(bodyId);
		}

		public void RemoveBody(int bodyId)
		{
			this._removedBodies.Add(bodyId);
		}

		public void Undo()
		{
			for (int i = 0; i < this._bodies.Count; i++)
			{
				if (this._gadgetContext.Bodies.ContainsKey(this._bodies[i]))
				{
					this._gadgetContext.Bodies[this._bodies[i]].Destroy();
					this._gadgetContext.Bodies.Remove(this._bodies[i]);
				}
			}
			for (int j = 0; j < this._executionPointsWithInnerEvent.Count; j++)
			{
				List<IEventContext> list = this._innerEvents[this._executionPointsWithInnerEvent[j]];
				for (int k = 0; k < list.Count; k++)
				{
					((IHMMEventContext)list[k]).Undo();
				}
			}
			this._parametersStream.ResetBitsRead();
			this._executedBlock = 0;
			this._innerEventsExecutedInPoint = 0;
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteInt(this.Id);
			bs.WriteInt(this.BlockIndex);
			bs.WriteInt(this.CreationTime);
			bs.WriteIntArray(this._bodies.ToArray());
			bs.WriteIntArray(this._removedBodies.ToArray());
			bs.WriteInt(this._numInitialParameters);
			bs.WriteByteArray(this._parametersStream.ByteArray);
			bs.WriteIntArray(this._executionPointsWithInnerEvent.ToArray());
			for (int i = 0; i < this._executionPointsWithInnerEvent.Count; i++)
			{
				List<IEventContext> list = this._innerEvents[this._executionPointsWithInnerEvent[i]];
				bs.WriteInt(list.Count);
				for (int j = 0; j < list.Count; j++)
				{
					((IBitStreamSerializable)list[j]).WriteToBitStream(bs);
				}
			}
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Id = bs.ReadInt();
			this.BlockIndex = bs.ReadInt();
			this.CreationTime = bs.ReadInt();
			int[] array = bs.CachedReadIntArray();
			int[] array2 = bs.CachedReadIntArray();
			this._bodies.Clear();
			this._bodies.AddRange(array);
			this._removedBodies.Clear();
			this._removedBodies.AddRange(array2);
			this._numInitialParameters = bs.ReadInt();
			this._byteArrayData = bs.CachedReadByteArray();
			this._parametersStream.ByteArray = this._byteArrayData;
			int[] array3 = bs.CachedReadIntArray();
			this._executionPointsWithInnerEvent.AddRange(array3);
			for (int i = 0; i < this._executionPointsWithInnerEvent.Count; i++)
			{
				int num = bs.ReadInt();
				List<IEventContext> list = new List<IEventContext>(8);
				this._innerEvents.Add(this._executionPointsWithInnerEvent[i], list);
				for (int j = 0; j < num; j++)
				{
					GadgetEvent instance = GadgetEvent.GetInstance(-1, this._gadgetContext);
					list.Add(instance);
					instance.ReadFromBitStream(bs);
				}
			}
			bs.FreeCachedIntArray(array);
			bs.FreeCachedIntArray(array2);
			bs.FreeCachedIntArray(array3);
		}

		public void GetBodies(out List<int> created, out List<int> removed)
		{
			created = new List<int>(this._bodies.ToArray());
			removed = new List<int>(this._removedBodies.ToArray());
			this.RecursiveFillBodyArrays(ref created, ref removed);
		}

		private void RecursiveFillBodyArrays(ref List<int> bodies, ref List<int> removes)
		{
			for (int i = 0; i < this._executionPointsWithInnerEvent.Count; i++)
			{
				int key = this._executionPointsWithInnerEvent[i];
				List<IEventContext> list;
				this._innerEvents.TryGetValue(key, out list);
				if (list != null)
				{
					for (int j = 0; j < list.Count; j++)
					{
						IHMMEventContext ihmmeventContext = list[j] as IHMMEventContext;
						if (ihmmeventContext != null)
						{
							List<int> collection;
							List<int> collection2;
							ihmmeventContext.GetBodies(out collection, out collection2);
							bodies.AddRange(collection);
							removes.AddRange(collection2);
						}
					}
				}
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GadgetEvent));

		private static readonly Stack<GadgetEvent> Pool = new Stack<GadgetEvent>(1024);

		private static readonly HashSet<GadgetEvent> _freedEvents = new HashSet<GadgetEvent>();

		private IEventContext _parentEvent;

		private int _executedBlock;

		private int _innerEventsExecutedInPoint;

		private int _numInitialParameters;

		private readonly List<int> _bodies = new List<int>(32);

		private readonly List<int> _removedBodies = new List<int>(32);

		private readonly BitStream _parametersStream = new BitStream();

		private IHMMGadgetContext _gadgetContext;

		private readonly List<int> _executionPointsWithInnerEvent = new List<int>(32);

		private readonly Dictionary<int, List<IEventContext>> _innerEvents = new Dictionary<int, List<IEventContext>>(32);

		private byte[] _byteArrayData;
	}
}
