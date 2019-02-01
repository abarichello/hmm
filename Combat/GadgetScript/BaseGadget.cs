using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public abstract class BaseGadget : GameHubScriptableObject, IHMMGadgetContext, IGadgetContext, IParameterContext
	{
		public int OwnerId { get; private set; }

		public IGadgetOwner Owner { get; private set; }

		public int Id { get; private set; }

		public int GetNewEventId()
		{
			return this._eventParser.NextId();
		}

		public int GetNewBodyId()
		{
			return ObjectId.New(6, BaseGadget._lastBodyId++);
		}

		public void SetLastBodyId(IEventContext ev)
		{
			BaseGadget._lastBodyId = ev.FirstEventBodyId.GetInstanceId();
		}

		public ICombatObject Bomb
		{
			get
			{
				return this._hmmContext.Bomb;
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

		public bool IsLocalPlayer
		{
			get
			{
				return this._hmmContext.IsClient && GameHubScriptableObject.Hub.Players.CurrentPlayerData.PlayerCarId == this.OwnerId;
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
			this.TriggerEvent(new GadgetEvent(blockIndex, this));
		}

		public void PrecacheAssets()
		{
			this.PrecacheBlocksRecursive(this);
			this._readNodes.Clear();
		}

		private void PrecacheBlocksRecursive(object node)
		{
			if (node == null || this._readNodes.Contains(node))
			{
				return;
			}
			this._readNodes.Add(node);
			IGadgetBlockWithAsset gadgetBlockWithAsset = node as IGadgetBlockWithAsset;
			if (gadgetBlockWithAsset != null)
			{
				gadgetBlockWithAsset.PrecacheAssets();
			}
			FieldInfo[] fields = node.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (!fields[i].FieldType.IsValueType)
				{
					this.PrecacheBlocksRecursive(fields[i].GetValue(node));
				}
			}
		}

		public void TriggerEvent(IEventContext eventContext)
		{
			if (this._runningEvent != null)
			{
				eventContext.SetParentEvent(this._runningEvent);
			}
			this._runningEvent = eventContext;
			((IHMMEventContext)eventContext).LoadInitialParameters();
			this.ExecuteBlocks(BaseBlock.GetBlock(eventContext.BlockIndex), eventContext);
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
			if (this._runningEvent == null)
			{
			}
			if (this._hmmContext.IsServer && this._runningEvent == null)
			{
				IHMMEventContext ihmmeventContext2 = (IHMMEventContext)eventContext;
				if (ihmmeventContext2.ShouldBeSent)
				{
					this._eventParser.SendEvent(this, ihmmeventContext2);
				}
			}
		}

		public virtual IHMMGadgetContext CreateGadgetContext(int id, Identifiable owner, GadgetEventParser eventParser, IHMMContext context)
		{
			BaseGadget baseGadget = UnityEngine.Object.Instantiate<BaseGadget>(this);
			baseGadget._hmmContext = context;
			baseGadget._eventParser = eventParser;
			baseGadget.Id = id;
			baseGadget.OwnerId = owner.ObjId;
			baseGadget.Owner = owner.GetComponent<IGadgetOwner>();
			baseGadget.Bodies = new Dictionary<int, IGadgetBody>();
			baseGadget._originalScriptable = this;
			return baseGadget;
		}

		private void ExecuteBlocks(IBlock block, IEventContext eventContext)
		{
			while (block != null)
			{
				this._blocksExecuted.Add(block);
				block = block.Execute(this, eventContext);
				if (this.IsServer)
				{
					eventContext.BlockExecuted();
				}
				if (this.IsClient)
				{
					IEventContext innerEvent;
					while ((innerEvent = eventContext.GetInnerEvent()) != null)
					{
						this.TriggerEvent(innerEvent);
					}
					eventContext.BlockExecuted();
				}
			}
		}

		[Conditional("AllowHacks")]
		[Conditional("UNITY_EDITOR")]
		private void LogBlocks(int lastId)
		{
			if (this._blocksExecuted.Exists((IBlock x) => ((BaseBlock)x).LogThisBlock))
			{
				List<string> list = this._blocksExecuted.ConvertAll<string>((IBlock x) => ((BaseBlock)x).name);
				string text = string.Format("[{0}] {1}", lastId.ToString(), string.Join(" -> ", list.ToArray()));
			}
			this._blocksExecuted.Clear();
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

		private HashSet<object> _readNodes = new HashSet<object>();

		[SerializeField]
		private bool _debug;

		protected IHMMContext _hmmContext;

		protected static int _lastBodyId;

		protected IEventContext _runningEvent;

		protected GadgetEventParser _eventParser;

		protected BaseGadget _originalScriptable;

		protected IGadgetOwner _owner;

		private static readonly BitLogger Log = new BitLogger(typeof(BaseGadget));

		private readonly List<IBlock> _blocksExecuted = new List<IBlock>();
	}
}
