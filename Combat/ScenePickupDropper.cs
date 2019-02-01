using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Scene;

namespace HeavyMetalMachines.Combat
{
	public class ScenePickupDropper : BasePickupDropper
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
				PickupAsset = this.AssetPickup,
				UnspawnOnLifeTimeEnd = true
			};
		}

		protected override void AfterDrop(SpawnReason spawnReason, UnspawnEvent unspawnEvent)
		{
		}
	}
}
