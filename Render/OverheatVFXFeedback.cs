using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.VFX;
using Pocketverse;

namespace HeavyMetalMachines.Render
{
	public class OverheatVFXFeedback : BaseVFXFeedback
	{
		protected override void OnStart()
		{
			this._gadgetState = this.combatObject.Combat.GadgetStates.GetGadgetState(this.Slot);
			if (this.CooldownPrefab != null)
			{
				GameHubBehaviour.Hub.Resources.PrefabPreCache(this.CooldownPrefab, 1);
			}
		}

		protected override bool CompareValues(float target, float current)
		{
			return this._gadgetState.GadgetState != GadgetState.CoolingAfterOverheat && current > target;
		}

		protected override void OnUpdate()
		{
			this.percent = this._gadgetState.Heat;
			if (this.CooldownPrefab == null)
			{
				return;
			}
			if (this._prefabInstance != null)
			{
				if (this._gadgetState.GadgetState != GadgetState.CoolingAfterOverheat)
				{
					this._prefabInstance.Destroy(BaseFX.EDestroyReason.Default);
					this._prefabInstance = null;
				}
			}
			else if (this._gadgetState.GadgetState == GadgetState.CoolingAfterOverheat)
			{
				this._prefabInstance = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.CooldownPrefab, base.transform.position, base.transform.rotation);
				this._prefabInstance.transform.parent = GameHubBehaviour.Hub.Drawer.Effects;
				this._prefabInstance.baseMasterVFX = this.CooldownPrefab;
				this._prefabInstance.Activate(this.combatObject.Id, this.combatObject.Id, this.combatObject.transform);
			}
		}

		protected override void OnFinished()
		{
			if (this._prefabInstance)
			{
				this._prefabInstance.Destroy(BaseFX.EDestroyReason.Default);
				this._prefabInstance = null;
			}
		}

		public GadgetSlot Slot;

		public MasterVFX CooldownPrefab;

		private GadgetData.GadgetStateObject _gadgetState;

		private MasterVFX _prefabInstance;
	}
}
