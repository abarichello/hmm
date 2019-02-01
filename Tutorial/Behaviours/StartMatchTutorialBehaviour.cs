using System;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class StartMatchTutorialBehaviour : InGameTutorialBehaviourBase
	{
		public override void Setup(int tIndex)
		{
			base.Setup(tIndex);
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				base.tutorialStep.DelayBeforeComplete = (float)this.DelaySeconds;
			}
		}

		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			GameHubBehaviour.Hub.BombManager.Rules.BombScoreTarget = this.BombScoreTarget;
			GameHubBehaviour.Hub.MatchMan.SetWarmup(this.DelaySeconds);
			GameHubBehaviour.Hub.MatchMan.CanStartMatch = true;
			this.CompleteBehaviourAndSync();
		}

		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			GameHubBehaviour.Hub.Characters.Async().ClientSendCounselorActivation(GameHubBehaviour.Hub.Options.Game.CounselorActive);
		}

		public int BombScoreTarget = 1;

		[Header("This behaviour will set DelayBeforeComplete with DelaySeconds")]
		[Header("Don't use less than 11 please! :)")]
		public int DelaySeconds = 11;
	}
}
