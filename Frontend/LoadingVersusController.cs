using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using ClientAPI.Objects;
using FMod;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using Standard_Assets.Scripts.HMM.Util;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class LoadingVersusController : GameHubBehaviour
	{
		public IObservable<Unit> OnLoadingStarted
		{
			get
			{
				return this._onLoadingStarted;
			}
		}

		public IObservable<Unit> OnLoadingFinished
		{
			get
			{
				return this._onLoadingFinished;
			}
		}

		public bool IsLoading
		{
			get
			{
				return this._isLoading;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnPreHideLoading;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnPosHideLoading;

		public void Awake()
		{
			this._timedUpdater = new TimedUpdater(300, true, true);
			this._diContainer = this._diContainer.ParentContainers.First<DiContainer>();
			this.WindowGameObject.SetActive(false);
			this.HintGroupGameObject.SetActive(false);
			this._isLoading = false;
			LoadingVersusPlayer component = this.AllyTeamGrid.GetChild(0).GetComponent<LoadingVersusPlayer>();
			component.CarSprite.sprite2D = null;
			LoadingVersusPlayer component2 = this.EnemyTeamGrid.GetChild(0).GetComponent<LoadingVersusPlayer>();
			component2.CarSprite.sprite2D = null;
			ObjectPoolUtils.CreateObjectPool<LoadingVersusPlayer>(component, out this._allyObjects, 4, this._diContainer);
			ObjectPoolUtils.CreateObjectPool<LoadingVersusPlayer>(component2, out this._enemyObjects, 4, this._diContainer);
			component.gameObject.SetActive(false);
			component2.gameObject.SetActive(false);
			this.AllyTeamGrid.Reposition();
			this.EnemyTeamGrid.Reposition();
			this.BackgroundSprite.ClearSprite();
		}

		public void ShowWindow(int arenaIndex)
		{
			this._onLoadingStarted.OnNext(Unit.Default);
			GameArenaConfig arenaConfig = GameHubBehaviour.Hub.ArenaConfig;
			TeamKind currentPlayerTeam = GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			string loadingImageName = arenaConfig.GetLoadingImageName(arenaIndex, (int)currentPlayerTeam);
			string arenaDraftName = arenaConfig.GetArenaDraftName(arenaIndex);
			string gameModeDraft = arenaConfig.GetCurrentArena().GameModeDraft;
			LoadingVersusController.ArenaGui arenaGui = this.FindArenaGui(arenaIndex);
			if (currentPlayerTeam == TeamKind.Blue)
			{
				arenaGui.GroupGameObject.SetActive(true);
			}
			else
			{
				arenaGui.FlippedGroupGameObject.SetActive(true);
			}
			this.InternalShowWindow(loadingImageName, arenaDraftName, gameModeDraft);
		}

		private LoadingVersusController.ArenaGui FindArenaGui(int arenaIndex)
		{
			LoadingVersusController.ArenaGui? arenaGui = null;
			foreach (LoadingVersusController.ArenaGui value in this._arenaGuiList)
			{
				value.GroupGameObject.SetActive(false);
				value.FlippedGroupGameObject.SetActive(false);
				if (value.ArenaIndex == arenaIndex)
				{
					arenaGui = new LoadingVersusController.ArenaGui?(value);
				}
			}
			return arenaGui.Value;
		}

		private void InternalShowWindow(string backgroundSpriteName, string arenaDraftName, string gameModeDraft)
		{
			if (!SpectatorController.IsSpectating)
			{
				GameHubBehaviour.Hub.Characters.Async().ClientSendCounselorActivation(GameHubBehaviour.Hub.Options.Game.CounselorActive);
			}
			this.BackgroundSprite.SpriteName = backgroundSpriteName;
			this.ArenaTitleLabel.text = Language.Get(arenaDraftName, TranslationContext.MainMenuGui);
			this.GameModeLabel.text = Language.Get(gameModeDraft, TranslationContext.Loading);
			this.WindowGameObject.SetActive(true);
			this.WindowPanel.alpha = 1f;
			this._isLoading = true;
			LoadingVersusController.Log.DebugFormat("Show Loading Window players:{0} Current matchData:{1}", new object[]
			{
				GameHubBehaviour.Hub.Players.PlayersAndBots.Count,
				GameHubBehaviour.Hub.Match
			});
			this.AllyTeamGrid.hideInactive = false;
			this.EnemyTeamGrid.hideInactive = false;
			bool flag = this._enableTeamFlip && GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena().TugOfWarFlipTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[i];
				bool flag2 = playerData.Team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam ^ flag;
				LoadingVersusPlayer loadingVersusPlayer = null;
				if (!ObjectPoolUtils.TryToGetFreeObject<LoadingVersusPlayer>((!flag2) ? this._enemyObjects : this._allyObjects, ref loadingVersusPlayer))
				{
					LoadingVersusController.Log.WarnFormat("No card available for player: {0}. Team:{1}", new object[]
					{
						playerData.Name,
						playerData.Team
					});
				}
				else
				{
					loadingVersusPlayer.gameObject.name = ((!playerData.IsCurrentPlayer) ? (playerData.PlayerCarId + 20).ToString("0000") : "0000");
					loadingVersusPlayer.gameObject.SetActive(true);
					loadingVersusPlayer.UpdatePlayerInfo(this, playerData, flag, this._teams);
					loadingVersusPlayer.transform.localScale = Vector3.one;
				}
			}
			this.AllyTeamGrid.hideInactive = true;
			this.EnemyTeamGrid.hideInactive = true;
			this.AllyTeamGrid.Reposition();
			this.EnemyTeamGrid.Reposition();
			int num = Random.Range(0, this._hintDrafts.Length);
			MultiPlatformLocalizationDraft multiPlatformLocalizationDraft = this._hintDrafts[num];
			string titleText = Language.Get("LOADING_HINT_TITLE", TranslationContext.LoadingHint);
			string descriptionText = Language.Get(multiPlatformLocalizationDraft.CurrentPlatformDraft, TranslationContext.LoadingHint);
			this.ShowHint(titleText, descriptionText);
			if (this.audioSnapshotPlayback != null)
			{
				this.audioSnapshotPlayback.Stop();
				this.audioSnapshotPlayback = null;
			}
			if (GameHubBehaviour.Hub.AudioSettings.loadingSnapshot != null)
			{
				this.audioSnapshotPlayback = FMODAudioManager.PlayAt(GameHubBehaviour.Hub.AudioSettings.loadingSnapshot, base.transform);
			}
			this.SetupTeamInfo();
			this._keyBindingsView.UpdateBindings();
		}

		public void HideWindow()
		{
			this._onLoadingFinished.OnNext(Unit.Default);
			base.StartCoroutine(this.HideWindowDelayCoroutine());
		}

		private void InvokeOnPreHideLoading()
		{
			if (this.OnPreHideLoading != null)
			{
				this.OnPreHideLoading();
			}
		}

		private void InvokeOnPosHideLoading()
		{
			if (this.OnPosHideLoading != null)
			{
				this.OnPosHideLoading();
			}
		}

		private IEnumerator HideWindowDelayCoroutine()
		{
			while (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.PreReplay || GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.Replay)
			{
				yield return UnityUtils.WaitForEndOfFrame;
			}
			this.InvokeOnPreHideLoading();
			float time = 0f;
			while (time < 0.5f)
			{
				time += Time.unscaledDeltaTime;
				this.WindowPanel.alpha = 1f - time / 0.5f;
				yield return UnityUtils.WaitForEndOfFrame;
			}
			this.WindowGameObject.SetActive(false);
			this._isLoading = false;
			LoadingVersusController.Log.Debug("Hide Loading Window");
			for (int i = 0; i < this._allyObjects.Length; i++)
			{
				this._allyObjects[i].gameObject.SetActive(false);
			}
			for (int j = 0; j < this._enemyObjects.Length; j++)
			{
				this._enemyObjects[j].gameObject.SetActive(false);
			}
			this.BackgroundSprite.ClearSprite();
			if (this.audioSnapshotPlayback != null)
			{
				this.audioSnapshotPlayback.KeyOff();
				this.audioSnapshotPlayback = null;
			}
			this.InvokeOnPosHideLoading();
			yield break;
		}

		public void ShowHint(string titleText, string descriptionText)
		{
			this.HintTitleLabel.text = titleText;
			this.HintDescriptionLabel.text = descriptionText;
			this.HintGroupGameObject.SetActive(true);
		}

		public void HideHint()
		{
			this.HintGroupGameObject.SetActive(true);
		}

		public void Update()
		{
			if (this._timedUpdater.ShouldHalt())
			{
				return;
			}
			if (GameHubBehaviour.Hub == null || !this.WindowGameObject.activeSelf)
			{
				return;
			}
			if (!this._isLoading)
			{
				LoadingVersusController.Log.ErrorFormat("LoadingWindow was active when it should not be! CurrentState={0}", new object[]
				{
					GameHubBehaviour.Hub.State.Current.GetType().ToString()
				});
				this.WindowGameObject.SetActive(false);
				return;
			}
		}

		public void OnDisable()
		{
			if (this.audioSnapshotPlayback != null)
			{
				this.audioSnapshotPlayback.Stop();
				this.audioSnapshotPlayback = null;
			}
		}

		private void SetupTeamInfo()
		{
			this.AllyTeamGameObject.SetActive(false);
			this.EnemyTeamGameObject.SetActive(false);
			this.SetGroupTeamInfo(TeamKind.Blue);
			this.SetGroupTeamInfo(TeamKind.Red);
			bool flag = this._enableTeamFlip && GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena().TugOfWarFlipTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			this.AllyTeamGameObject.transform.parent = ((!flag) ? this.LeftFlagTeamPivot.transform : this.RightFlagTeamPivot.transform);
			this.AllyTeamGameObject.transform.localPosition = Vector3.zero;
			this.EnemyTeamGameObject.transform.parent = ((!flag) ? this.RightFlagTeamPivot.transform : this.LeftFlagTeamPivot.transform);
			this.EnemyTeamGameObject.transform.localPosition = Vector3.zero;
		}

		private void SetGroupTeamInfo(TeamKind teamKind)
		{
			Team groupTeam = this._teams.GetGroupTeam(teamKind);
			if (groupTeam == null)
			{
				return;
			}
			ITeamNameRestriction teamNameRestriction = this._diContainer.Resolve<ITeamNameRestriction>();
			string anyTeamTagRestriction = teamNameRestriction.GetAnyTeamTagRestriction(groupTeam.CurrentUgmUserPlayerId, groupTeam.Tag);
			bool flag = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == teamKind;
			if (flag)
			{
				this.AllyTeamIconSprite.SpriteName = groupTeam.ImageUrl;
				this.AllyTeamNameLabel.text = NGUIText.EscapeSymbols(string.Format("[{0}]", anyTeamTagRestriction));
				this.AllyTeamGameObject.SetActive(true);
				this.FetchAndFillTeamUserGeneratedContentPublisherUserName(groupTeam, this.AllyTeamUserGeneratedContentCurrentOwnerPublisherUserNameLabel);
			}
			else
			{
				this.EnemyTeamIconSprite.SpriteName = groupTeam.ImageUrl;
				this.EnemyTeamNameLabel.text = NGUIText.EscapeSymbols(string.Format("[{0}]", anyTeamTagRestriction));
				this.EnemyTeamGameObject.SetActive(true);
				this.FetchAndFillTeamUserGeneratedContentPublisherUserName(groupTeam, this.EnemyTeamUserGeneratedContentCurrentOwnerPublisherUserNameLabel);
			}
		}

		private void FetchAndFillTeamUserGeneratedContentPublisherUserName(Team team, UILabel label)
		{
			label.text = string.Empty;
			IGetDisplayablePublisherUserName getDisplayablePublisherUserName = this._diContainer.Resolve<IGetDisplayablePublisherUserName>();
			ObservableExtensions.Subscribe<string>(Observable.Do<string>(getDisplayablePublisherUserName.GetAsTeamUgcOwner(team.CurrentUgmUserUniversalId), delegate(string displayablePublisherUserName)
			{
				label.text = displayablePublisherUserName;
			}));
		}

		private static readonly BitLogger Log = new BitLogger(typeof(LoadingVersusController));

		[Inject]
		private IMatchTeams _teams;

		[Inject]
		private DiContainer _diContainer;

		private const float HideWindowDelayInSec = 0.5f;

		private readonly Subject<Unit> _onLoadingStarted = new Subject<Unit>();

		private readonly Subject<Unit> _onLoadingFinished = new Subject<Unit>();

		public Color AllyTeamColor;

		public Color EnemyTeamColor;

		public Color AllyPlayerNameColor;

		public Color EnemyPlayerNameColor;

		public Color CurrentPlayerColor;

		public int PingGoodEndValue;

		public int PingBadStartValue;

		public Color PingGoodColor;

		public Color PingMediumColor;

		public Color PingBadColor;

		public Color ProgressBarEmptyColor;

		public Color ProgressBarFullColor;

		public GameObject WindowGameObject;

		public UIPanel WindowPanel;

		public GameObject HintGroupGameObject;

		public UILabel HintTitleLabel;

		public UILabel HintDescriptionLabel;

		public UIGrid AllyTeamGrid;

		public UIGrid EnemyTeamGrid;

		[SerializeField]
		private LoadingVersusKeyBindingsView _keyBindingsView;

		[Header("[Border sprites]")]
		public Sprite AllyBorderSprite;

		public Sprite EnemyBorderSprite;

		public Sprite CurrentBorderSprite;

		[Header("[ProgressBar sprites]")]
		public Sprite AllyProgressBarSprite;

		public Sprite EnemyProgressBarSprite;

		public Sprite CurrentProgressBarSprite;

		[Header("[Arena]")]
		public HMMUI2DDynamicSprite BackgroundSprite;

		public UILabel ArenaTitleLabel;

		public UILabel GameModeLabel;

		[SerializeField]
		private LoadingVersusController.ArenaGui[] _arenaGuiList;

		[Header("[Team]")]
		public GameObject LeftFlagTeamPivot;

		public GameObject RightFlagTeamPivot;

		public GameObject AllyTeamGameObject;

		public HMMUI2DDynamicSprite AllyTeamIconSprite;

		public UILabel AllyTeamNameLabel;

		public UILabel AllyTeamUserGeneratedContentCurrentOwnerPublisherUserNameLabel;

		public GameObject EnemyTeamGameObject;

		public HMMUI2DDynamicSprite EnemyTeamIconSprite;

		public UILabel EnemyTeamNameLabel;

		public UILabel EnemyTeamUserGeneratedContentCurrentOwnerPublisherUserNameLabel;

		[SerializeField]
		private bool _enableTeamFlip;

		private TimedUpdater _timedUpdater;

		private FMODAudioManager.FMODAudio audioSnapshotPlayback;

		private bool _isLoading;

		private LoadingVersusPlayer[] _allyObjects;

		private LoadingVersusPlayer[] _enemyObjects;

		[SerializeField]
		private MultiPlatformLocalizationDraft[] _hintDrafts;

		[Serializable]
		private struct ArenaGui
		{
			public int ArenaIndex;

			public GameObject GroupGameObject;

			public GameObject FlippedGroupGameObject;
		}
	}
}
