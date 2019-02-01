using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Bomb
{
	public class EventPing : BasePing
	{
		protected override bool OnPlayerCreated(PlayerEvent data)
		{
			if (!base.OnPlayerCreated(data))
			{
				return false;
			}
			GameHubBehaviour.Hub.BombManager.ListenToBombDrop += this.ListenToBombDrop;
			GameHubBehaviour.Hub.BombManager.ListenToBombUnspawn += this.ListenToBombUnspawn;
			GameHubBehaviour.Hub.BombManager.ListenToClientBombCreation += this.ListenToClientBombCreation;
			GameHubBehaviour.Hub.BombManager.ListenToBombAlmostDeliveredTriggerExit += this.ListenToBombAlmostDeliveredTriggerExit;
			GameHubBehaviour.Hub.BombManager.ListenToBombAlmostDeliveredTriggerEnter += this.ListenToBombAlmostDeliveredTriggerEnter;
			return true;
		}

		protected override void GameOver(MatchData.MatchState matchWinner)
		{
			base.GameOver(matchWinner);
			if (!GameHubBehaviour.Hub)
			{
				return;
			}
			GameHubBehaviour.Hub.BombManager.ListenToBombDrop -= this.ListenToBombDrop;
			GameHubBehaviour.Hub.BombManager.ListenToBombUnspawn -= this.ListenToBombUnspawn;
			GameHubBehaviour.Hub.BombManager.ListenToClientBombCreation -= this.ListenToClientBombCreation;
			GameHubBehaviour.Hub.BombManager.ListenToBombAlmostDeliveredTriggerExit -= this.ListenToBombAlmostDeliveredTriggerExit;
			GameHubBehaviour.Hub.BombManager.ListenToBombAlmostDeliveredTriggerEnter -= this.ListenToBombAlmostDeliveredTriggerEnter;
		}

		private void ListenToBombDrop(BombInstance bombinstance, SpawnReason reason, int causer)
		{
			Transform bombTransform = GameHubBehaviour.Hub.BombManager.GetBombTransform();
			if (bombTransform)
			{
				this.Ping(bombTransform, 2, false, 0);
			}
		}

		private void ListenToBombUnspawn(PickupRemoveEvent evt)
		{
			BasePing.EventPing3D pingInstance = this.GetPingInstance(3);
			if (pingInstance != null && pingInstance.Instance3D.IsPlaying)
			{
				return;
			}
			SpawnReason reason = evt.Reason;
			if (reason == SpawnReason.Pickup)
			{
				if (evt.Causer == GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
				{
					this.Ping(GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.transform, 4, true, 0);
				}
				else
				{
					CombatObject combat = CombatRef.GetCombat(evt.Causer);
					if (combat.Team == GameHubBehaviour.Hub.Players.CurrentPlayerData.Team)
					{
						this.Ping(combat.transform, 5, true, 0);
					}
					else
					{
						this.Ping(combat.transform, 6, true, 0);
					}
				}
			}
		}

		private void ListenToClientBombCreation(int bombeventid)
		{
			this.StopPingEvent(3);
			Transform bombTransform = GameHubBehaviour.Hub.BombManager.GetBombTransform();
			if (bombTransform)
			{
				this.Ping(bombTransform, 1, false, 0);
			}
		}

		private void ListenToBombAlmostDeliveredTriggerEnter(TeamKind defensorTeamKind)
		{
			if (!GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb())
			{
				return;
			}
			this.Ping(GameHubBehaviour.Hub.BombManager.BombMovement.transform, 3, true, 0);
		}

		private void ListenToBombAlmostDeliveredTriggerExit()
		{
			this.StopPingEvent(3);
		}

		public enum EventPingKind
		{
			None,
			BombSpawn,
			BombDrop,
			BombAlmostDelivered,
			BombPickClientOwner,
			BombPickAllied,
			BombPickEnemy
		}
	}
}
