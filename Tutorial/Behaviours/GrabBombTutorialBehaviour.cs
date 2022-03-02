using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class GrabBombTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged += this.OnBombDelivered;
		}

		protected override void OnStepCompletedOnServer()
		{
			GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged -= this.OnBombDelivered;
		}

		private void OnBombDelivered(CombatObject carrier)
		{
			GrabBombTutorialBehaviour.Log.DebugFormat("OnbombCarrierChange Carrier {0}", new object[]
			{
				carrier
			});
			if (!carrier || carrier.Id.ObjId != GameHubBehaviour.Hub.Players.Players[0].CharacterInstance.ObjId)
			{
				return;
			}
			this.CompleteBehaviourAndSync();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GrabBombTutorialBehaviour));
	}
}
