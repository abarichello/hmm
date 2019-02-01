using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.VFX
{
	internal class ClientVFXTrigger : GameHubBehaviour
	{
		private void Start()
		{
			this.targetObj = base.GetComponentInParent<CombatObject>();
			if (this.targetObj == null)
			{
				return;
			}
			this.Identifiable = base.GetComponentInParent<Identifiable>();
			if (this.vfx && GameHubBehaviour.Hub.Net.IsClient())
			{
				MasterVFX obj = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.vfx, base.transform.position, base.transform.rotation);
				GameHubBehaviour.Hub.Resources.ReturnToPrefabCache(this.vfx, obj);
			}
			ClientVFXTrigger.TriggerType triggerType = this.targetType;
			if (triggerType == ClientVFXTrigger.TriggerType.OnImpulseReceived)
			{
				this.targetObj.OnImpulseReceived += this.TriggerFX;
			}
		}

		private void OnDestroy()
		{
			ClientVFXTrigger.TriggerType triggerType = this.targetType;
			if (triggerType == ClientVFXTrigger.TriggerType.OnImpulseReceived)
			{
				if (this.targetObj != null)
				{
					this.targetObj.OnImpulseReceived -= this.TriggerFX;
				}
			}
		}

		private void TriggerFX(float obj, int otherId)
		{
			if (this.vfx == null || !GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			MasterVFX masterVFX = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.vfx, base.transform.position, base.transform.rotation);
			masterVFX.transform.parent = GameHubBehaviour.Hub.Drawer.Effects;
			masterVFX.baseMasterVFX = this.vfx;
			masterVFX.Activate(this.Identifiable, this.Identifiable, base.transform);
		}

		public ClientVFXTrigger.TriggerType targetType;

		public MasterVFX vfx;

		private Identifiable Identifiable;

		private CombatObject targetObj;

		internal enum TriggerType
		{
			OnImpulseReceived
		}
	}
}
