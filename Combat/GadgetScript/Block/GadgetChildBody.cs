using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	public class GadgetChildBody : BaseGadgetBody
	{
		public override IGadgetContext Context
		{
			get
			{
				return this._gadgetContext;
			}
		}

		public override Identifiable Identifiable
		{
			get
			{
				return this._identifiable;
			}
		}

		public override Transform Transform
		{
			get
			{
				return base.transform;
			}
		}

		private void Awake()
		{
			this._identifiable = base.GetComponent<Identifiable>();
			this._colliders = base.gameObject.GetComponentsInChildren<Collider>(true);
			this._destroyReason = BaseFX.EDestroyReason.Default;
		}

		public void Initialize(IHMMGadgetContext gadgetContext, IHMMEventContext eventContext, Component prefab, GadgetChildBody.ChildBodyEventsBlocks eventBlocks)
		{
			base.Initialize();
			this._eventBlocks = eventBlocks;
			this._gadgetContext = gadgetContext;
			base.Id = this._gadgetContext.GetNewBodyId();
			base.CreationEventId = gadgetContext.Id;
			base.IsAlive = true;
			this._prefab = prefab;
			base.WasSentToClient = eventContext.ShouldBeSent;
			if (this._gadgetContext.IsClient)
			{
				foreach (Collider collider in this._colliders)
				{
					if (collider)
					{
						Object.Destroy(collider);
					}
				}
				this._colliders = new Collider[0];
			}
			else
			{
				foreach (Collider collider2 in this._colliders)
				{
					gadgetContext.Owner.GadgetCombatObject.AddCollider(collider2);
				}
			}
			base.RaiseBodyInitialized();
		}

		public override void Destroy()
		{
			base.Destroy();
			base.IsAlive = false;
			foreach (Collider collider in this._colliders)
			{
				this._gadgetContext.Owner.GadgetCombatObject.RemoveCollider(collider);
			}
			base.RaiseBodyDestroyed();
			ResourceLoader.Instance.ReturnToPrefabCache(this._prefab, this);
			if (this._eventBlocks.OnDestroyed != null)
			{
				this._gadgetContext.TriggerEvent(GadgetEvent.GetInstance(this._eventBlocks.OnDestroyed.Id, this._gadgetContext, GadgetChildBody.EmptyParameterList));
			}
		}

		private static readonly List<BaseParameter> EmptyParameterList = new List<BaseParameter>();

		private Identifiable _identifiable;

		private Collider[] _colliders;

		private IHMMGadgetContext _gadgetContext;

		private Component _prefab;

		private GadgetChildBody.ChildBodyEventsBlocks _eventBlocks;

		[Serializable]
		public class ChildBodyEventsBlocks
		{
			[Tooltip("Called when the gadget body is detroyed.")]
			public BaseBlock OnDestroyed;
		}
	}
}
