using System;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class BombDeliveredTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivered;
		}

		protected override void OnStepCompletedOnServer()
		{
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.OnBombDelivered;
			GameHubBehaviour.Hub.MatchMan.EndMatch(this._playerTeam);
		}

		private void OnBombDelivered(int causerid, TeamKind scoredTeam, Vector3 deliveryPosition)
		{
			this._playerTeam = scoredTeam;
			this.CompleteBehaviourAndSync();
		}

		private TeamKind _playerTeam;
	}
}
