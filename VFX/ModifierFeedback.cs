using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ModifierFeedback : AbstractFX, ICachedObject
	{
		public GadgetBehaviour Gadget
		{
			get
			{
				if (this._gadget == null)
				{
					this._gadget = this.GetGadgetBehaviour();
				}
				return this._gadget;
			}
		}

		private GadgetBehaviour GetGadgetBehaviour()
		{
			if (this.Instance != null && this.Instance.Causer > 0 && this.Owner)
			{
				CombatObject combat = CombatRef.GetCombat(this.Owner);
				return combat.GetGadget(this.Instance.GadgetSlot);
			}
			return null;
		}

		public override int EventId
		{
			get
			{
				return this.Instance.InstanceId;
			}
			set
			{
				this.Instance.InstanceId = value;
			}
		}

		public override Identifiable Target
		{
			get
			{
				Identifiable result;
				if ((result = this._target) == null)
				{
					result = (this._target = GameHubBehaviour.Hub.ObjectCollection.GetObject(this.Instance.Target));
				}
				return result;
			}
		}

		public override Identifiable Owner
		{
			get
			{
				Identifiable result;
				if ((result = this._owner) == null)
				{
					result = (this._owner = GameHubBehaviour.Hub.ObjectCollection.GetObject(this.Instance.Causer));
				}
				return result;
			}
		}

		public override Vector3 TargetPosition
		{
			get
			{
				return (!(this._targetPosition == Vector3.zero) || !(this.Target != null)) ? this._targetPosition : (this._targetPosition = this.Target.transform.position);
			}
		}

		public override byte CustomVar
		{
			get
			{
				return 0;
			}
		}

		public override CDummy.DummyKind GetDummyKind()
		{
			return this.Instance.FeedbackInfo.Dummy;
		}

		public override Transform GetDummy(CDummy.DummyKind kind)
		{
			return this.Target.GetComponent<CombatObject>().Dummy.GetDummy(kind, null, null);
		}

		public override bool WasCreatedInFog()
		{
			return false;
		}

		public override GadgetBehaviour GetGadget()
		{
			return this.Gadget;
		}

		private void Awake()
		{
			if (this.VFX)
			{
				GameHubBehaviour.Hub.Resources.PrefabPreCache(this.VFX, 1);
			}
			if (this.SFX)
			{
				GameHubBehaviour.Hub.Resources.PrefabPreCache(this.SFX, 1);
			}
		}

		public void InitializeFeedback()
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.IsTest())
			{
				if (this.VFX)
				{
					this.vfxInstance = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.VFX, base.transform.position, base.transform.rotation);
					GameHubBehaviour.Hub.Drawer.AddEffect(this.vfxInstance.transform);
					this.vfxInstance.baseMasterVFX = this.VFX;
					this.vfxInstance = this.vfxInstance.Activate(this);
				}
				if (this.SFX)
				{
					this.sfxInstance = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.SFX, base.transform.position, base.transform.rotation);
					GameHubBehaviour.Hub.Drawer.AddEffect(this.sfxInstance.transform);
					this.sfxInstance.baseMasterVFX = this.SFX;
					this.sfxInstance = this.sfxInstance.Activate(this);
				}
			}
		}

		private void InstantiateMaster(MasterVFX master)
		{
		}

		public void DestroyFeedbackEffect(DestroyFeedbackEffectMessage evt)
		{
			if (this.vfxInstance)
			{
				this.vfxInstance.Destroy(BaseFX.EDestroyReason.Default);
			}
			if (this.sfxInstance)
			{
				this.sfxInstance.Destroy(BaseFX.EDestroyReason.Default);
			}
			for (int i = 0; i < base.DestroyEffectListenerScripts.Length; i++)
			{
				if (base.DestroyEffectListenerScripts[i] is DestroyFeedbackEffectMessage.IDestroyFeedbackEffectListener)
				{
					((DestroyFeedbackEffectMessage.IDestroyFeedbackEffectListener)base.DestroyEffectListenerScripts[i]).OnDestroyFeedbackEffect(evt);
				}
			}
			this._gadget = null;
			this._target = null;
			GameHubBehaviour.Hub.Resources.ReturnToPrefabCache(this.prefabRef, base.transform);
		}

		public void OnSendToCache()
		{
			this._owner = null;
			this._target = null;
		}

		public void OnGetFromCache()
		{
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ModifierFeedback));

		public ModifierFeedbackInstance Instance;

		public Transform prefabRef;

		public MasterVFX VFX;

		public MasterVFX SFX;

		public MasterVFX vfxInstance;

		public MasterVFX sfxInstance;

		private GadgetBehaviour _gadget;

		private Identifiable _target;

		private Identifiable _owner;

		private Vector3 _targetPosition = Vector3.zero;
	}
}
