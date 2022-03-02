using System;
using System.Collections.Generic;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Tutorial
{
	public class TutorialController : GameHubBehaviour
	{
		private MainMenuGui MainMenuGui
		{
			get
			{
				return (!(this._mainMenuGui == null)) ? this._mainMenuGui : (this._mainMenuGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>());
			}
		}

		private static TutorialController Instance
		{
			get
			{
				return (!(TutorialController._instance == null)) ? TutorialController._instance : (TutorialController._instance = GameHubBehaviour.Hub.TutorialHub.TutorialControllerInstance);
			}
		}

		public void Start()
		{
			this.tutorialDatasDictionary.Clear();
			TutorialData[] tutorialDatas = this.tutorialContentData.tutorialDatas;
			int i = 0;
			int num = tutorialDatas.Length;
			while (i < num)
			{
				TutorialData tutorialData = tutorialDatas[i];
				this.tutorialDatasDictionary.Add(tutorialData.Name, tutorialData);
				i++;
			}
			this._listeningToLobbyLoadedEvent = false;
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.OnStateChange;
			GameHubBehaviour.Hub.PlayerPrefs.ExecOnceOnPrefsLoaded(new Action(this.OnPlayerPrefsLoaded));
		}

		private void OnPlayerPrefsLoaded()
		{
			this._tutorialCompletionCount = GameHubBehaviour.Hub.PlayerPrefs.GetInt("TUTORIAL_INGAME_FINISHED", 0);
		}

		public void OnDestroy()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.OnStateChange;
			TutorialController._instance = null;
		}

		private void MainMenuGuiOnLobbyUpdate(bool isInLobby)
		{
			if (!isInLobby)
			{
				return;
			}
			MainMenuGui.OnLobbyUpdate -= this.MainMenuGuiOnLobbyUpdate;
			this._listeningToLobbyLoadedEvent = false;
		}

		private void IncreaseTutorialCompletionCount()
		{
			this._tutorialCompletionCount++;
			TutorialController.Log.DebugFormat("[IncreaseTutorialCompletionCount] New Value: {0}", new object[]
			{
				this._tutorialCompletionCount
			});
			GameHubBehaviour.Hub.PlayerPrefs.SetInt("TUTORIAL_INGAME_FINISHED", this._tutorialCompletionCount);
			GameHubBehaviour.Hub.PlayerPrefs.SaveNow();
		}

		private void OnStateChange(GameState changedstate)
		{
			if (!(changedstate is MainMenu))
			{
				this._mainMenuGui = null;
				return;
			}
			if (!this._listeningToLobbyLoadedEvent)
			{
				this._listeningToLobbyLoadedEvent = true;
				MainMenuGui.OnLobbyUpdate += this.MainMenuGuiOnLobbyUpdate;
			}
		}

		public void StartLoadingAndGoToTutorial()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				MainMenu mainMenu = (MainMenu)GameHubBehaviour.Hub.State.Current;
				mainMenu.SearchForAMatch(string.Empty);
				return;
			}
			GameHubBehaviour.Hub.GuiScripts.Loading.ShowTutorialLoading(false);
			GameHubBehaviour.Hub.State.GotoState(GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.MatchMaking), false);
		}

		public bool PlayerMustGoToTutorial()
		{
			return this._isFeatureToggled.Check(Features.ForcedTutorial) && this.IsPlayerEligibleForTutorial();
		}

		private bool IsPlayerEligibleForTutorial()
		{
			return !this.HasPlayerDoneTutorial();
		}

		public bool HasPlayerDoneTutorial()
		{
			PlayerBag bag = GameHubBehaviour.Hub.User.Bag;
			return bag.HasDoneTutorial;
		}

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		public static readonly BitLogger Log = new BitLogger(typeof(TutorialController));

		public TutorialUIContentData tutorialContentData;

		public bool EnableTutorialWelcome;

		public Dictionary<string, TutorialData> tutorialDatasDictionary = new Dictionary<string, TutorialData>();

		private MainMenuGui _mainMenuGui;

		private static TutorialController _instance;

		private int _tutorialCompletionCount;

		public const string TUTORIAL_INGAME_FINISHED_PREFSKEY = "TUTORIAL_INGAME_FINISHED";

		private bool _hasStepsToSave;

		private const float TutorialStepsSaveDelay = 10f;

		private bool _listeningToLobbyLoadedEvent;
	}
}
