using System;
using System.Collections;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Automated
{
	public class AutoTest : GameHubBehaviour
	{
		private void OnEnable()
		{
			AutoTest.Log.Debug("OnEnable");
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.ListenToStateChanged;
			MainMenu.PlayerReloadedEvent += this.OnMenuLoaded;
		}

		private void OnDisable()
		{
			AutoTest.Log.Debug("OnDisable");
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.ListenToStateChanged;
			MainMenu.PlayerReloadedEvent -= this.OnMenuLoaded;
		}

		private void Start()
		{
			base.enabled = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.AutoTest);
		}

		private void Update()
		{
			if (this._isOnQueue && this._mainMenuGui.MatchStats.MMInterfaceState == MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk)
			{
				AutoTest.Log.Debug("AcceptMatch");
				this._mainMenuGui.MatchAccept.OnClickMatchAccept();
				this._isOnQueue = false;
				this._mainMenuGui = null;
			}
		}

		private void ListenToStateChanged(GameState pChangedstate)
		{
			if (pChangedstate is Game)
			{
				AutoTest.Log.Debug("Game");
				this._game = (Game)pChangedstate;
				this._game.FinishedLoading += this.OnFinishedLoadingGameLevelAndEvents;
				this._game.OnGameOver += this.OnGameOver;
			}
			else if (pChangedstate is MainMenu)
			{
				AutoTest.Log.Debug("MainMenu");
				this._mainMenuGui = ((MainMenu)pChangedstate).GetStateGuiController<MainMenuGui>();
			}
		}

		private void OnMenuLoaded()
		{
			AutoTest.Log.Debug("Menu loaded");
			base.StartCoroutine(this.TryToJoinQueue());
		}

		private IEnumerator TryToJoinQueue()
		{
			while (null == this._mainMenuGui)
			{
				AutoTest.Log.Debug("TryToJoinQueue");
				yield return UnityUtils.WaitForOneSecond;
			}
			this._mainMenuGui.JoinQueue(GameModeTabs.CoopVsBots);
			this._isOnQueue = true;
			yield break;
		}

		private IEnumerator Pick()
		{
			AutoTest.Log.Debug("Pick");
			yield return UnityUtils.WaitForTwoSeconds;
			PickModeSetup pick = (PickModeSetup)GameHubBehaviour.Hub.State.Current;
			IItemType[] availableCharInfos = GameHubBehaviour.Hub.InventoryColletion.GetAllAvailableCharactersItemTypes();
			IItemType charItemType = availableCharInfos[SysRandom.Int(0, availableCharInfos.Length)];
			AutoTest.Log.Debug("TryingToPick");
			CharacterItemTypeComponent charComponent = charItemType.GetComponent<CharacterItemTypeComponent>();
			pick.SelectCharacter(charComponent.CharacterId);
			yield return UnityUtils.WaitForOneSecond;
			pick.ConfirmPick(charComponent.CharacterId);
			yield return UnityUtils.WaitForOneSecond;
			pick.ConfirmSkin(charItemType.Id, Guid.Empty);
			yield return UnityUtils.WaitForOneSecond;
			yield break;
		}

		private void OnFinishedLoadingGameLevelAndEvents()
		{
			this._game.FinishedLoading -= this.OnFinishedLoadingGameLevelAndEvents;
			base.StartCoroutine(this.WaitAndWin(GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.AutoTestTimeInSeconds)));
		}

		private IEnumerator WaitAndWin(int secondsToWin)
		{
			yield return new WaitForSeconds((float)secondsToWin);
			yield break;
		}

		private void OnGameOver(MatchData.MatchState matchWinner)
		{
			AutoTest.Log.Debug("GameOver");
			this._game.OnGameOver -= this.OnGameOver;
			base.StartCoroutine(this.WaitAndReturnToMenu(40f));
		}

		private IEnumerator WaitAndReturnToMenu(float secondsToWait)
		{
			yield return new WaitForSeconds(secondsToWait);
			this._game.ClearBackToMain();
			yield break;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(AutoTest));

		private Game _game;

		private MainMenuGui _mainMenuGui;

		private bool _isOnQueue;
	}
}
