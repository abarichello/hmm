using System;
using HeavyMetalMachines.Audio.Music;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class LoadingTutorialBehaviour : ActionTutorialBehaviourBase
	{
		protected override void ExecuteAction()
		{
			LoadingTutorialBehaviour.EAction action = this.Action;
			if (action != LoadingTutorialBehaviour.EAction.Hide)
			{
				if (action == LoadingTutorialBehaviour.EAction.Show)
				{
					MusicManager.StopMusic();
					GameHubBehaviour.Hub.GuiScripts.Loading.ShowDefaultLoading(false);
				}
			}
			else
			{
				MusicManager.PlayMusic(MusicManager.State.InGame);
				GameHubBehaviour.Hub.GuiScripts.Loading.HideLoading();
			}
		}

		[SerializeField]
		private LoadingTutorialBehaviour.EAction Action;

		private enum EAction
		{
			Hide,
			Show
		}
	}
}
