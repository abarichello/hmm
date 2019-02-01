using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ClientAPI;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using UnityEngine;

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
			GameHubBehaviour.Hub.PlayerPrefs.ExecOnPrefsLoaded(new System.Action(this.OnPlayerPrefsLoaded));
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
				mainMenu.SearchForAMatch(string.Empty, null);
				return;
			}
			GameHubBehaviour.Hub.GuiScripts.Loading.ShowTutorialLoading(false);
			GameHubBehaviour.Hub.State.GotoState(GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.MatchMaking), false);
		}

		public bool HasFinishedTutorial()
		{
			return this.HasDoneStep(GameHubBehaviour.Hub.SharedConfigs.TutorialConfig.InGameTutorialStepFinished);
		}

		public bool HasPassedTutorial(string tutorialName)
		{
			this._tutorialStepsToSend = (this._tutorialStepsToSend ?? GameHubBehaviour.Hub.User.Bag.TutorialSteps);
			return !string.IsNullOrEmpty(this._tutorialStepsToSend) && !this.tutorialDatasDictionary[tutorialName].Redo && this.HasDoneStep(tutorialName);
		}

		public bool HasDoneStep(string tutorialName)
		{
			if (string.IsNullOrEmpty(tutorialName))
			{
				TutorialController.Log.WarnFormat("Empty tutorialName on HasDoneStep. Stack: {0}", new object[]
				{
					new StackTrace()
				});
				return true;
			}
			TutorialData tutorialData;
			if (this.tutorialDatasDictionary.TryGetValue(tutorialName, out tutorialData))
			{
				this._tutorialStepsToSend = (this._tutorialStepsToSend ?? GameHubBehaviour.Hub.User.Bag.TutorialSteps);
				return TutorialUtils.HasDoneStep(this._tutorialStepsToSend, tutorialData.id);
			}
			TutorialController.Log.WarnFormat("Invalid tutorialName on HasDoneStep: {0}", new object[]
			{
				tutorialName
			});
			return true;
		}

		public void PassTutorial(string tutorialStepName)
		{
			if (!this.tutorialDatasDictionary.ContainsKey(tutorialStepName))
			{
				return;
			}
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(ClientBITags.TutorialEnd, string.Format("Tutorial={0}", tutorialStepName), true);
			if (!this.HasPassedTutorial(tutorialStepName))
			{
				this._tutorialStepsToSend = (this._tutorialStepsToSend ?? GameHubBehaviour.Hub.User.Bag.TutorialSteps);
				int id = this.tutorialDatasDictionary[tutorialStepName].id;
				this._tutorialStepsToSend = TutorialUtils.UpdateTutorialBag(this._tutorialStepsToSend, id);
				if (this.tutorialDatasDictionary[tutorialStepName].SaveRightNow)
				{
					this._hasStepsToSave = true;
					base.StartCoroutine(this.WaitAndSaveTutorialSteps(0f));
				}
				else if (!this._hasStepsToSave)
				{
					this._hasStepsToSave = true;
					base.StartCoroutine(this.WaitAndSaveTutorialSteps(10f));
				}
			}
		}

		private IEnumerator WaitAndSaveTutorialSteps(float waitTime)
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				yield break;
			}
			if (waitTime > 0f)
			{
				yield return new WaitForSeconds(waitTime);
			}
			this._hasStepsToSave = false;
			PlayerCustomWS.SaveTutorialSteps(this._tutorialStepsToSend, new SwordfishClientApi.ParameterizedCallback<string>(this.OnSaveTutorialStepsOk), delegate(object x, Exception e)
			{
				TutorialController.Log.ErrorFormat("Error trying to save tutorialSteps.", new object[]
				{
					e
				});
			});
			yield break;
		}

		private void OnSaveTutorialStepsOk(object state, string obj)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<T>)obj);
			if (!netResult.Success)
			{
				TutorialController.Log.ErrorFormat("Save tutorialSteps error={0}", new object[]
				{
					netResult.Msg
				});
			}
		}

		public bool PlayerMustGoToTutorial()
		{
			return this.IsForcedTutorialEnabled() && this.IsPlayerEligibleForTutorial();
		}

		private bool IsForcedTutorialEnabled()
		{
			return GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.ForceTutorial);
		}

		private bool IsPlayerEligibleForTutorial()
		{
			PlayerBag bag = GameHubBehaviour.Hub.User.Bag;
			return !bag.HasDoneTutorial;
		}

		public static bool HasPlayerDoneFirstTutorial()
		{
			return TutorialController.Instance.HasDoneStep(GameHubBehaviour.Hub.SharedConfigs.TutorialConfig.FirstTutorialStep);
		}

		public static void SavePlayerDoneFirstTutorial()
		{
			TutorialController.Instance.PassTutorial(GameHubBehaviour.Hub.SharedConfigs.TutorialConfig.FirstTutorialStep);
		}

		public void ShowTutorialModalWindow(TutorialModalWindow.WindowMode windowMode)
		{
			MainMenuGui mainMenuGui = this.MainMenuGui;
			if (mainMenuGui == null)
			{
				TutorialController.Log.Warn("MainMenuGUI not found!");
				return;
			}
			TutorialModalWindowController tutorialModalWindowController = mainMenuGui.TutorialModalWindowController;
			if (tutorialModalWindowController == null)
			{
				TutorialController.Log.Warn("TutorialModalWindowController not found!");
				return;
			}
			tutorialModalWindowController.Show(windowMode);
		}

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

		private string _tutorialStepsToSend;

		private bool _listeningToLobbyLoadedEvent;
	}
}
