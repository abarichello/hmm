using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class CreepBeacon : BasicCannon
	{
		public CreepBeaconInfo MyInfo
		{
			get
			{
				return base.Info as CreepBeaconInfo;
			}
		}

		private PassiveCreepSpawn CreepSpawn
		{
			get
			{
				PassiveCreepSpawn result;
				if ((result = this._creepSpawn) == null)
				{
					result = (this._creepSpawn = base.GetComponent<PassiveCreepSpawn>());
				}
				return result;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			CreepBeaconInfo myInfo = this.MyInfo;
			this._creepBuffs = ModifierData.CreateData(myInfo.CreepBuffs, myInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._creepBuffs.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			this._currentFire = base.FireGadget();
			return this._currentFire;
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (evt.RemoveData.TargetEventId != this._currentFire)
			{
				return;
			}
			this._currentFire = -1;
			if (evt.RemoveData.TargetId == -1)
			{
				return;
			}
			PassiveCreepSpawn creepSpawn = this.CreepSpawn;
			if (!creepSpawn)
			{
				return;
			}
			creepSpawn.BuffedAggroCreeps(evt.RemoveData.TargetId, base.LifeTime, this._creepBuffs, this.Combat, evt.RemoveData.TargetEventId);
		}

		protected ModifierData[] _creepBuffs;

		private PassiveCreepSpawn _creepSpawn;

		private int _currentFire;
	}
}
