using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using HeavyMetalMachines.Utils;
using Hoplon.GadgetScript;
using Pocketverse;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public class GadgetEvent : IHMMEventContext, IBitStreamSerializable, IEventContext, INodeHolder, IParameterContext
	{
		public GadgetEvent(int blockIndex, IGadgetContext context)
		{
			this._gadgetContext = (IHMMGadgetContext)context;
			this.Id = this._gadgetContext.GetNewEventId();
			this.BlockIndex = blockIndex;
			this.CreationTime = this._gadgetContext.CurrentTime;
			this.ShouldBeSent = false;
			this.PreviousEventId = -1;
		}

		public GadgetEvent(int blockIndex, IGadgetContext context, List<BaseParameter> parameters) : this(blockIndex, context)
		{
			this._numInitialParameters = parameters.Count;
			for (int i = 0; i < parameters.Count; i++)
			{
				parameters[i].WriteToBitStreamWithContentId(context, this._parametersStream);
			}
		}

		public int Id { get; private set; }

		public int BlockIndex { get; private set; }

		public int CreationTime { get; private set; }

		public bool ShouldBeSent { get; private set; }

		public int PreviousEventId { get; private set; }

		public IEventContext ParentEvent
		{
			get
			{
				return this._parentEvent;
			}
		}

		public int FirstEventBodyId
		{
			get
			{
				return (this._bodies.Count <= 0) ? 0 : this._bodies[0];
			}
		}

		public int[] NodeIds { get; private set; }

		public int[] NodeRemovals { get; private set; }

		public bool ShouldCreateBody
		{
			get
			{
				if (this._bodies.Count == 0)
				{
					return false;
				}
				int key = this._bodies[0];
				bool flag = this._bodyDestructionEvent.ContainsKey(key) && this._bodyDestructionEvent[key].CreationTime < this._gadgetContext.CurrentTime;
				return !this._gadgetContext.Bodies.ContainsKey(key) && !flag;
			}
		}

		public void TryConsumeFirstBody()
		{
			if (this._bodies.Count == 0)
			{
				return;
			}
			this._bodies.RemoveAt(0);
		}

		public void LoadInitialParameters()
		{
			for (int i = 0; i < this._numInitialParameters; i++)
			{
				BaseParameter.ReadParameterFromBitStreamWithContentId(this._gadgetContext, this._parametersStream);
			}
		}

		public void SetBodyDestructionEvent(int bodyId, IEventContext ev)
		{
			this._bodyDestructionEvent[bodyId] = ev;
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
				GadgetEvent.Log.FatalFormat("Trying to set LastFrameId when it was already set. (Is event trying to destroy multiple Bodies from different events?) Gadget={0} - Root Block={1} Prev={2} Id={3} Eve={4}", new object[]
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

		public IEventContext GetInnerEvent()
		{
			if (this._executionPointsWithInnerEvent.Contains(this._executedBlock) && this._innerEvents[this._executedBlock].Count > this._innerEventsExecutedInPoint)
			{
				IEventContext result = this._innerEvents[this._executedBlock][this._innerEventsExecutedInPoint];
				this._innerEventsExecutedInPoint++;
				return result;
			}
			this._innerEventsExecutedInPoint = 0;
			return null;
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
			this.ConvertBodiesToNodes();
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
			this._bodies = new List<int>(bs.ReadIntArray());
			this._removedBodies = new List<int>(bs.ReadIntArray());
			this._numInitialParameters = bs.ReadInt();
			this._parametersStream = new BitStream(bs.ReadByteArray());
			this._executionPointsWithInnerEvent.AddRange(bs.ReadIntArray());
			for (int i = 0; i < this._executionPointsWithInnerEvent.Count; i++)
			{
				int num = bs.ReadInt();
				List<IEventContext> list = new List<IEventContext>();
				this._innerEvents.Add(this._executionPointsWithInnerEvent[i], list);
				for (int j = 0; j < num; j++)
				{
					GadgetEvent gadgetEvent = new GadgetEvent(-1, this._gadgetContext);
					list.Add(gadgetEvent);
					gadgetEvent.ReadFromBitStream(bs);
				}
			}
		}

		private void ConvertBodiesToNodes()
		{
			this.NodeIds = this._bodies.ToArray();
			this.NodeRemovals = this._removedBodies.ToArray();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GadgetEvent));

		private IEventContext _parentEvent;

		private int _executedBlock;

		private int _innerEventsExecutedInPoint;

		private int _numInitialParameters;

		private List<int> _bodies = new List<int>();

		private List<int> _removedBodies = new List<int>();

		private Dictionary<int, IEventContext> _bodyDestructionEvent = new Dictionary<int, IEventContext>();

		private BitStream _parametersStream = new BitStream();

		private readonly IHMMGadgetContext _gadgetContext;

		private readonly List<int> _executionPointsWithInnerEvent = new List<int>();

		private readonly Dictionary<int, List<IEventContext>> _innerEvents = new Dictionary<int, List<IEventContext>>();
	}
}
