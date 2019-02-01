using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Scene;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Turret
{
	public class ScrapPickup : BasePickup
	{
		private void Awake()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
			}
		}

		protected override void ExecuteOnTriggerServer(CombatObject combatObject, Collider other)
		{
			GameHubBehaviour.Hub.ScrapBank.AddPickupReward(combatObject.Id.ObjId, this.ScrapRewardIndex, base.transform.position);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ScrapPickup));

		public int ScrapRewardIndex;
	}
}
