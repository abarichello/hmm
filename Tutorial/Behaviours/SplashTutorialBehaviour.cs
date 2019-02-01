using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	[RequireComponent(typeof(SplashPlayer))]
	public class SplashTutorialBehaviour : InGameTutorialBehaviourBase
	{
		public override void Setup(int tIndex)
		{
			base.Setup(tIndex);
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipTutorialSplashes))
			{
				return;
			}
			LanguageCode languageCode = LocalizationSettings.GetLanguageEnum(Language.settings.defaultLangCode);
			LanguageCode languageCode2 = Language.CurrentLanguage();
			for (int i = 0; i < this.AvailableVideoLanguageCodes.Length; i++)
			{
				LanguageCode languageCode3 = this.AvailableVideoLanguageCodes[i];
				if (languageCode2 == languageCode3)
				{
					languageCode = languageCode2;
					break;
				}
			}
			this.VideoFileName = string.Format(this.VideoFileName, languageCode);
			this._splashPlayer = base.GetComponent<SplashPlayer>();
			this._splashPlayer.SetVideoFileName(this.VideoFileName, 0);
			this._splashPlayer.SetVideoTexture(TutorialUIController.Instance.MovieTexture);
		}

		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipTutorialSplashes))
			{
				this.FinishedVideos();
				return;
			}
			this._splashPlayer.PlaySplashes(new Action(this.FinishedVideos));
			this.SetupSplashInterface(true);
			TutorialUIController.Instance.MovieRewindButton.onClick.Add(new EventDelegate(delegate()
			{
				this._splashPlayer.Rewind();
				TutorialUIController.Instance.MovieButtonsGroup.SetBool("active", false);
			}));
			TutorialUIController.Instance.MovieNextButton.onClick.Add(new EventDelegate(new EventDelegate.Callback(this.CompleteBehaviourAndSync)));
			TutorialUIController.Instance.ShowPanel();
			TutorialUIController.Instance.ShowOvelay(0.5f);
		}

		public void FinishedVideos()
		{
			this.CompleteBehaviourAndSync();
		}

		protected override void OnStepCompletedOnClient()
		{
			base.OnStepCompletedOnClient();
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipTutorialSplashes))
			{
				return;
			}
			this.SetupSplashInterface(false);
			TutorialUIController.Instance.MovieButtonsGroup.SetBool("active", false);
			TutorialUIController.Instance.MovieRewindButton.onClick.Clear();
			TutorialUIController.Instance.MovieNextButton.onClick.Clear();
			if (!this.HasNextWindow)
			{
				TutorialUIController.Instance.HideOvelay(0.5f);
			}
		}

		private void SetupSplashInterface(bool on)
		{
			TutorialUIController.Instance.MovieTextureGroup.SetBool("active", on);
		}

		public bool HasNextWindow = true;

		private const string WindowAnimatorActiveFieldName = "active";

		private SplashPlayer _splashPlayer;

		public string VideoFileName = string.Empty;

		public LanguageCode[] AvailableVideoLanguageCodes;
	}
}
