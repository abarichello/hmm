using System;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	public class LevelUpPickup : BasePickup
	{
		protected override void ExecuteOnTriggerServer(CombatObject combatObject, Collider other)
		{
			PlayerStats component = combatObject.GetComponent<PlayerStats>();
			if (component)
			{
				component.LevelCounter++;
			}
			else
			{
				LevelUpPickup.Log.ErrorFormat("LevelUp pickup cannot find playerStats for obj:{0}", new object[]
				{
					combatObject.Id.ObjId
				});
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(LevelUpPickup));
	}
}
