using System;
using HeavyMetalMachines.Tutorial;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Frontend
{
	public class MatchmakingTutorial : GameState
	{
		protected override void OnStateEnabled()
		{
			this.Cleanup();
			if (!string.IsNullOrEmpty(this.RequiredTutorialForMatchWithBots) && GameHubBehaviour.Hub.TutorialHub.TutorialControllerInstance.HasDoneStep(this.RequiredTutorialForMatchWithBots))
			{
				this.StartCustomWithBotsMatch();
				return;
			}
			this.StartLoadingAndGoToTutorialArena();
		}

		public void StartLoadingAndGoToTutorialArena()
		{
			GameHubBehaviour.Hub.Swordfish.Log.BILogClient(ClientBITags.TutorialWaitingMatchmaking, true);
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnMatchStartedEvent += this.OnTutorialMatchStarted;
			string config = string.Format("Level={0}:TutorialPlayer={1}", GameHubBehaviour.Hub.SharedConfigs.TutorialConfig.TutorialSceneName, GameHubBehaviour.Hub.User.UserSF.UniversalID);
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.StartMatch(new string[]
			{
				GameHubBehaviour.Hub.User.UserSF.UniversalID
			}, config, new Action(this.OnMatchError));
		}

		public void OnMatchError()
		{
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnMatchStartedEvent -= this.OnTutorialMatchStarted;
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = string.Format(Language.Get(this.MatchMakingTutorialErrorDraftFeedback, TranslationSheets.Tutorial), new object[0]),
				OkButtonText = Language.Get("Ok", "GUI"),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					GameHubBehaviour.Hub.Quit();
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void StartCustomWithBotsMatch()
		{
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnMatchStartedEvent += this.OnTutorialMatchStarted;
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.StartMatch(new string[]
			{
				GameHubBehaviour.Hub.User.UserSF.UniversalID
			}, "CustomWithBotsPlayerID=" + GameHubBehaviour.Hub.User.UserSF.Id, new Action(this.OnMatchError));
		}

		private void OnTutorialMatchStarted()
		{
			GameHubBehaviour.Hub.Swordfish.Log.BILogClient(ClientBITags.TutorialConnectingToMatch, true);
			GameHubBehaviour.Hub.Swordfish.Msg.ConnectToMatch(this.MainState, null);
			TutorialController.SavePlayerDoneFirstTutorial();
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnMatchStartedEvent -= this.OnTutorialMatchStarted;
		}

		public void Cleanup()
		{
			GameHubBehaviour.Hub.User.Bag.CurrentServerIp = null;
			GameHubBehaviour.Hub.User.Bag.CurrentMatchId = null;
			GameHubBehaviour.Hub.User.Bag.CurrentGroupId = null;
			GameHubBehaviour.Hub.User.Bag.CurrentPort = 0;
			SceneManager.LoadScene("Void");
			Mural.PostAll(default(CleanupMessage), typeof(ICleanupListener));
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MatchmakingTutorial));

		public MainMenu MainState;

		public GameState GameState;

		public string MatchMakingTutorialErrorDraftFeedback = "MatchMakingTutorialErrorDraftFeedback";

		[TutorialDataReference(true)]
		[Tooltip("If selected none, won't try to start custom with bots")]
		public string RequiredTutorialForMatchWithBots;
	}
}
