using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombTeamTrigger : GameHubBehaviour
	{
		private void Start()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				base.gameObject.SetActive(false);
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.ServerTrigger(other, true);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.ServerTrigger(other, false);
			}
		}

		private void ServerTrigger(Collider other, bool enter)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			if (!combat || !combat.IsPlayer)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned && GameHubBehaviour.Hub.BombManager.IsCarryingBomb(combat.Id.ObjId))
			{
				GameHubBehaviour.Hub.BombManager.ChangeTeamTrigger((!enter) ? TeamKind.Neutral : this.TeamOwner);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BombTeamTrigger));

		public TeamKind TeamOwner;
	}
}
