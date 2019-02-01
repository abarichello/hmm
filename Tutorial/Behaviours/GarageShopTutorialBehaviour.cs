using System;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class GarageShopTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			if (!GameHubBehaviour.Hub.BombManager.Rules.InstancesEnabled)
			{
				this.CompleteBehaviourAndSync();
				return;
			}
			InGameTutorialMaskController.Instance.gameObject.SetActive(false);
		}

		private void OnInstanceSelected(string id)
		{
			InGameTutorialMaskController.Instance.gameObject.SetActive(true);
			InGameTutorialMaskController.Instance.HideOverlay(0.5f);
			this.CompleteBehaviourAndSync();
		}
	}
}
