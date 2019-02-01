using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Scene
{
	public class LevelUpPickupDropper : BasePickupDropper
	{
		protected override bool BeforeDrop(SpawnReason spawnReason, UnspawnEvent unspawnEvent)
		{
			return true;
		}

		protected override void ExecuteOnClientDrop(SpawnReason spawnReason, UnspawnEvent unspawnEvent)
		{
		}

		protected override bool ExecuteOnServerDrop(SpawnReason spawnReason, UnspawnEvent unspawnEvent)
		{
			return true;
		}

		protected override PickupDropEvent CreatePickupDropEvent(SpawnReason spawnReason, UnspawnEvent unspawnEvent)
		{
			return new PickupDropEvent
			{
				Position = base.transform.position,
				Causer = base.Id.ObjId,
				PickerTeam = base.CombatObject.Team.GetEnemyTeam(),
				PickupAsset = this.AssetPickup,
				Reason = SpawnReason.Pickup,
				UnspawnOnLifeTimeEnd = this.UnspawnOnLifeTimeEnd
			};
		}

		protected override void AfterDrop(SpawnReason spawnReason, UnspawnEvent unspawnEvent)
		{
		}

		public static readonly BitLogger Log = new BitLogger(typeof(LevelUpPickupDropper));
	}
}
