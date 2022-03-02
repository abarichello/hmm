using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Playback;
using Hoplon.DependencyInjection;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public abstract class BaseGadget : GameHubScriptableObject, IHMMGadgetContext, IGadgetContext, IGadgetInput
	{
		public IGadgetOwner Owner { get; private set; }

		public int Id { get; private set; }

		public IInjectionResolver InjectionResolver { get; set; }

		public int GetNewEventId()
		{
			return this._serverDispatcher.GetNextFrameId();
		}

		public int GetNewBodyId()
		{
			return ObjectId.New(6, BaseGadget._lastBodyId++);
		}

		public void SetLastBodyId(int objectId)
		{
			BaseGadget._lastBodyId = objectId.GetInstanceId();
		}

		public ICombatObject Bomb
		{
			get
			{
				return this._hmmContext.Bomb;
			}
		}

		public IGameCamera GameCamera
		{
			get
			{
				return this._hmmContext.GameCamera;
			}
		}

		public Dictionary<int, IGadgetBody> Bodies { get; private set; }

		public bool IsClient
		{
			get
			{
				return this._hmmContext.IsClient;
			}
		}

		public bool IsServer
		{
			get
			{
				return this._hmmContext.IsServer;
			}
		}

		public bool IsTest
		{
			get
			{
				return this._hmmContext.IsTest;
			}
		}

		public int CurrentTime
		{
			get
			{
				return this._hmmContext.Clock.GetPlaybackTime();
			}
		}

		public Drawers HierarchyDrawers
		{
			get
			{
				return GameHubScriptableObject.Hub.Drawer;
			}
		}

		public IScoreBoard ScoreBoard
		{
			get
			{
				return this._hmmContext.ScoreBoard;
			}
		}

		public IStateMachine StateMachine
		{
			get
			{
				return this._hmmContext.StateMachine;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BlockExecutionDelegate OnBlockExecutionEnter;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BlockExecutionDelegate OnBlockExecutionExit;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventTriggerDelegate OnEventTriggered;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventTriggerDelegate OnEventCompleted;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event GadgetContextCreated OnGadgetContextCreated;

		public void SetBodyDestructionTime(int bodyId, int time)
		{
			this._bodyDestructionTime[bodyId] = time;
		}

		public bool TryGetBodyDestructionTime(int bodyId, out int time)
		{
			return this._bodyDestructionTime.TryGetValue(bodyId, out time);
		}

		public ICombatObject GetCombatObject(int id)
		{
			return this._hmmContext.GetCombatObject(id);
		}

		public ICombatObject GetCombatObject(Component component)
		{
			return this._hmmContext.GetCombatObject(component);
		}

		public IIdentifiable GetIdentifiable(int id)
		{
			return this._hmmContext.GetIdentifiable(id);
		}

		public bool IsCarryingBomb(ICombatObject combatObject)
		{
			return this._hmmContext.IsCarryingBomb(combatObject);
		}

		public void TriggerEvent(int blockIndex)
		{
			this.TriggerEvent(GadgetEvent.GetInstance(blockIndex, this));
		}

		public void ScheduleEvent(IEventContext eventContext)
		{
			this._timer.ScheduleEvent(eventContext);
		}

		public void CancelScheduledEvent(int scheduledEventId)
		{
			this._timer.CancelScheduledEvent(scheduledEventId);
		}

		public void CancelAllScheduledEvents()
		{
			this._timer.CancelAllEvents();
		}

		public virtual void PrecacheAssets(IHMMContext context)
		{
			this.InitializeBlocks(this._blocksToInitialize, context);
			foreach (IList<IBlock> list in this._subGadgetEvents.Values)
			{
				Queue<BaseBlock> queue = new Queue<BaseBlock>();
				for (int i = 0; i < list.Count; i++)
				{
					queue.Enqueue((BaseBlock)list[i]);
				}
				this.InitializeBlocks(queue, context);
			}
		}

		private void InitializeBlocks(Queue<BaseBlock> blocksToVisit, IHMMContext context)
		{
			IList<BaseBlock> list = new List<BaseBlock>();
			HashSet<BaseBlock> hashSet = new HashSet<BaseBlock>();
			while (blocksToVisit.Count > 0)
			{
				BaseBlock baseBlock = blocksToVisit.Dequeue();
				if (!(null == baseBlock) && !hashSet.Contains(baseBlock))
				{
					hashSet.Add(baseBlock);
					list.Clear();
					baseBlock.Initialize(ref list, context);
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i] != null)
						{
							blocksToVisit.Enqueue(list[i]);
						}
					}
				}
			}
		}

		public void TriggerEvent(IEventContext eventContext)
		{
			if (this._runningEvent != null)
			{
				eventContext.SetParentEvent(this._runningEvent);
			}
			if (this.OnEventTriggered != null)
			{
				this.OnEventTriggered(eventContext);
			}
			this._runningEvent = eventContext;
			((IHMMEventContext)eventContext).LoadInitialParameters();
			this.ExecuteBlocks(eventContext.RootBlock, eventContext);
			if (this.IsServer && this._runningEvent.ParentEvent != null)
			{
				IHMMEventContext ihmmeventContext = (IHMMEventContext)this._runningEvent;
				if (ihmmeventContext.ShouldBeSent)
				{
					ihmmeventContext.ParentEvent.AddInnerEvent(ihmmeventContext);
					((IHMMEventContext)ihmmeventContext.ParentEvent).SendToClient();
				}
			}
			int id = this._runningEvent.Id;
			this._runningEvent = this._runningEvent.ParentEvent;
			if (this.OnEventCompleted != null)
			{
				this.OnEventCompleted(eventContext);
			}
			if (this._runningEvent == null)
			{
				if (this._hmmContext.IsServer && ((IHMMEventContext)eventContext).ShouldBeSent)
				{
					this._eventParser.SendEvent(this, (IHMMEventContext)eventContext);
				}
				else
				{
					GadgetEvent.Free((GadgetEvent)eventContext);
				}
			}
		}

		public virtual IHMMGadgetContext CreateGadgetContext(int id, IGadgetOwner owner, IGadgetEventDispatcher eventParser, IHMMContext context, IServerPlaybackDispatcher dispatcher, IInjectionResolver injectionResolver)
		{
			BaseGadget baseGadget = Object.Instantiate<BaseGadget>(this);
			baseGadget.InjectionResolver = injectionResolver;
			baseGadget._hmmContext = context;
			baseGadget._eventParser = eventParser;
			baseGadget._serverDispatcher = dispatcher;
			baseGadget.Id = id;
			baseGadget.Owner = owner;
			baseGadget.Bodies = new Dictionary<int, IGadgetBody>();
			baseGadget._subGadgetEvents = new Dictionary<int, IList<IBlock>>();
			baseGadget._bodyDestructionTime = new Dictionary<int, int>();
			baseGadget._listenerEvents = new Dictionary<int, IList<BaseGadget.EventHolder>>();
			baseGadget._eventDirectory = new Dictionary<int, IBlock>();
			GameObject gameObject = new GameObject(owner.Identifiable.ObjId.ToString() + "-" + base.name + " - timer");
			GameHubScriptableObject.Hub.Drawer.AddEffect(gameObject.transform);
			baseGadget._timer = gameObject.AddComponent<GadgetTimer>();
			baseGadget._timer.SetGadgetContext(baseGadget);
			baseGadget._originalScriptable = this;
			if (this.OnGadgetContextCreated != null)
			{
				this.OnGadgetContextCreated(baseGadget);
			}
			return baseGadget;
		}

		protected abstract Queue<BaseBlock> _blocksToInitialize { get; }

		private void ExecuteBlocks(IBlock block, IEventContext eventContext)
		{
			while (block != null)
			{
				if (this.OnBlockExecutionEnter != null)
				{
					this.OnBlockExecutionEnter(block);
				}
				IBlock block2;
				try
				{
					block2 = block.Execute(this, eventContext);
				}
				catch (Exception ex)
				{
					BaseGadget.Log.ErrorFormat("Failed to execute block [{0} (ID={1})] of gadget '{2}'. Error={3}.", new object[]
					{
						block.Name,
						block.Id,
						base.name,
						ex
					});
					break;
				}
				if (this.IsServer)
				{
					this.TriggerSubGadgetsEvents(block.Id);
					this.TriggerListenerEvents(block.Id);
				}
				if (this.IsClient)
				{
					this.TriggerInnerEvents(eventContext);
				}
				if (this.OnBlockExecutionExit != null)
				{
					this.OnBlockExecutionExit(block);
				}
				block = block2;
				eventContext.BlockExecuted();
			}
		}

		private void TriggerSubGadgetsEvents(int blockId)
		{
			IList<IBlock> list;
			if (this._subGadgetEvents.TryGetValue(blockId, out list))
			{
				for (int i = 0; i < list.Count; i++)
				{
					this.TriggerEvent(list[i].Id);
				}
			}
		}

		private void TriggerListenerEvents(int blockId)
		{
			IList<BaseGadget.EventHolder> list;
			if (this._listenerEvents.TryGetValue(blockId, out list))
			{
				this.eventsCopy.AddRange(list);
				list.Clear();
				for (int i = 0; i < this.eventsCopy.Count; i++)
				{
					BaseGadget.EventHolder eventHolder = this.eventsCopy[i];
					eventHolder.Event.CreationTime = this.CurrentTime;
					this._eventDirectory.Remove(eventHolder.Event.Id);
					eventHolder.Gadget.TriggerEvent(eventHolder.Event);
				}
				this.eventsCopy.Clear();
			}
		}

		private void TriggerInnerEvents(IEventContext eventContext)
		{
			IList<IEventContext> innerEvents = eventContext.GetInnerEvents();
			if (innerEvents != null)
			{
				for (int i = 0; i < innerEvents.Count; i++)
				{
					this.TriggerEvent(innerEvents[i]);
				}
			}
		}

		public void ListenToBlock(IBlock triggerBlock, IEventContext outcome, IGadgetContext gadget)
		{
			IList<BaseGadget.EventHolder> list;
			if (!this._listenerEvents.TryGetValue(triggerBlock.Id, out list))
			{
				list = new List<BaseGadget.EventHolder>();
				this._listenerEvents.Add(triggerBlock.Id, list);
			}
			list.Add(new BaseGadget.EventHolder
			{
				Event = outcome,
				Gadget = gadget
			});
			this._eventDirectory.Add(outcome.Id, triggerBlock);
		}

		public void CancelListenToBlock(int listenEventId)
		{
			IBlock block;
			if (this._eventDirectory.TryGetValue(listenEventId, out block))
			{
				IList<BaseGadget.EventHolder> list = this._listenerEvents[block.Id];
				for (int i = 0; i < list.Count; i++)
				{
					IEventContext @event = list[i].Event;
					if (@event.Id == listenEventId)
					{
						GadgetEvent.Free((GadgetEvent)@event);
						list.RemoveAt(i);
						this._eventDirectory.Remove(listenEventId);
						return;
					}
				}
			}
		}

		public virtual void SetLifebarVisibility(int combatObjectId, bool visible)
		{
		}

		public virtual void SetAttachedLifebarGroupVisibility(int lifebarOwnerId, int attachedId, bool visible)
		{
		}

		public virtual IParameter<T> GetUIParameter<T>(string param)
		{
			return null;
		}

		public virtual List<BaseParameter> GetAllUIParameters()
		{
			return null;
		}

		public void CleanUp()
		{
			foreach (IGadgetBody gadgetBody in this.Bodies.Values)
			{
				gadgetBody.Destroy();
			}
			this.Bodies.Clear();
		}

		public abstract void ForcePressed();

		public abstract void ForceReleased();

		private Dictionary<int, int> _bodyDestructionTime;

		protected IHMMContext _hmmContext;

		protected static int _lastBodyId;

		protected IEventContext _runningEvent;

		protected IGadgetEventDispatcher _eventParser;

		protected IServerPlaybackDispatcher _serverDispatcher;

		protected BaseGadget _originalScriptable;

		protected IGadgetOwner _owner;

		protected GadgetTimer _timer;

		protected IDictionary<int, IList<IBlock>> _subGadgetEvents = new Dictionary<int, IList<IBlock>>();

		private IDictionary<int, IList<BaseGadget.EventHolder>> _listenerEvents;

		private IDictionary<int, IBlock> _eventDirectory;

		private static readonly BitLogger Log = new BitLogger(typeof(BaseGadget));

		private readonly List<BaseGadget.EventHolder> eventsCopy = new List<BaseGadget.EventHolder>();

		private struct EventHolder
		{
			public IEventContext Event;

			public IGadgetContext Gadget;
		}
	}
}
