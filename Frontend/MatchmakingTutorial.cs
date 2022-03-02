using System;
using ClientAPI.Service;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Welcome;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Frontend
{
	public class MatchmakingTutorial : GameState
	{
		protected override void OnStateEnabled()
		{
			this._swordfishLog = GameHubBehaviour.Hub.Swordfish.Log;
			this._swordfishMessage = GameHubBehaviour.Hub.Swordfish.Msg;
			this.Cleanup();
			GameHubBehaviour.Hub.ClientApi.WebServiceRequestTimeout += this.ClientApiOnWebServiceRequestTimeout;
			this.StartLoadingAndGoToTutorialArena();
		}

		protected override void OnStateDisabled()
		{
			GameHubBehaviour.Hub.ClientApi.WebServiceRequestTimeout -= this.ClientApiOnWebServiceRequestTimeout;
		}

		private void ClientApiOnWebServiceRequestTimeout(object sender, WebServiceRequestTimeoutArgs e)
		{
			MatchmakingTutorial.Log.WarnFormat("ClientApiOnWebServiceRequestTimeout. Api={0} Service={1}", new object[]
			{
				e.ApiName,
				e.ServiceName
			});
			this._swordfishMessage.Matchmaking.OnMatchStartedEvent -= this.OnTutorialMatchStarted;
			this.TryToShowTimeoutMessage();
		}

		private void StartLoadingAndGoToTutorialArena()
		{
			this._swordfishLog.BILogClient(48, true);
			this._swordfishMessage.Matchmaking.OnMatchStartedEvent += this.OnTutorialMatchStarted;
			string config = "MatchKind:Tutorial:ArenaIndex:0";
			this._swordfishMessage.Matchmaking.StartMatch(new string[]
			{
				GameHubBehaviour.Hub.User.UserSF.UniversalID
			}, config, new Action(this.OnMatchError));
		}

		private void OnMatchError()
		{
			this._swordfishLog.BILogClient(72, true);
			this._swordfishMessage.Matchmaking.OnMatchStartedEvent -= this.OnTutorialMatchStarted;
			this.TryToShowTimeoutMessage();
		}

		private void TryToShowTimeoutMessage()
		{
			if (this._errorMessageId != Guid.Empty)
			{
				return;
			}
			this._errorMessageId = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				IsStackable = false,
				Guid = this._errorMessageId,
				QuestionText = Language.Get("LostMessageHubConnection", TranslationContext.GUI),
				OkButtonText = Language.Get("Ok", TranslationContext.GUI),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(this._errorMessageId);
					GameHubBehaviour.Hub.EndSession("MatchmakingTutorial: WebServiceRequestTimeout");
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void OnTutorialMatchStarted()
		{
			this._swordfishLog.BILogClient(49, true);
			this._swordfishMessage.ConnectToMatch();
			this._swordfishMessage.Matchmaking.OnMatchStartedEvent -= this.OnTutorialMatchStarted;
		}

		private void Cleanup()
		{
			GameHubBehaviour.Hub.User.Bag.CurrentServerIp = null;
			GameHubBehaviour.Hub.User.Bag.CurrentMatchId = null;
			GameHubBehaviour.Hub.User.Bag.CurrentGroupId = null;
			GameHubBehaviour.Hub.User.Bag.CurrentPort = 0;
			SceneManager.LoadScene("Void");
			Mural.PostAll(default(CleanupMessage), typeof(ICleanupListener));
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MatchmakingTutorial));

		public WelcomeGameStateBehaviour WelcomeGameState;

		private SwordfishLog _swordfishLog;

		private SwordfishMessage _swordfishMessage;

		private Guid _errorMessageId;
	}
}
