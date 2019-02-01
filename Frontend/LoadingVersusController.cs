using System;
using System.Collections;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using ClientAPI.Objects;
using FMod;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class LoadingVersusController : GameHubBehaviour
	{
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
			this.WindowGameObject.SetActive(false);
			this.HintGroupGameObject.SetActive(false);
			this._isLoading = false;
			LoadingVersusPlayer component = this.AllyTeamGrid.GetChild(0).GetComponent<LoadingVersusPlayer>();
			component.CarSprite.sprite2D = null;
			LoadingVersusPlayer component2 = this.EnemyTeamGrid.GetChild(0).GetComponent<LoadingVersusPlayer>();
			component2.CarSprite.sprite2D = null;
			ObjectPoolUtils.CreateObjectPool<LoadingVersusPlayer>(component, out this._allyObjects, 4);
			ObjectPoolUtils.CreateObjectPool<LoadingVersusPlayer>(component2, out this._enemyObjects, 4);
			component.gameObject.SetActive(false);
			component2.gameObject.SetActive(false);
			this.AllyTeamGrid.Reposition();
			this.EnemyTeamGrid.Reposition();
			this.BackgroundSprite.ClearSprite();
		}

		public void ShowWindow(int arenaIndex)
		{
			GameArenaConfig arenaConfig = GameHubBehaviour.Hub.ArenaConfig;
			TeamKind currentPlayerTeam = GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			string loadingImageName = arenaConfig.GetLoadingImageName(arenaIndex, (int)currentPlayerTeam);
			string arenaDraftName = arenaConfig.GetArenaDraftName(arenaIndex);
			string gameModeDraft = arenaConfig.Arenas[arenaIndex].GameModeDraft;
			for (int i = 0; i < this._arenaGuiList.Length; i++)
			{
				LoadingVersusController.ArenaGui arenaGui = this._arenaGuiList[i];
				arenaGui.GroupGameObject.SetActive(false);
				arenaGui.FlippedGroupGameObject.SetActive(false);
				if (arenaGui.ArenaIndex == arenaIndex)
				{
					if (currentPlayerTeam == TeamKind.Blue)
					{
						arenaGui.GroupGameObject.SetActive(true);
					}
					else
					{
						arenaGui.FlippedGroupGameObject.SetActive(true);
					}
				}
			}
			this.InternalShowWindow(loadingImageName, arenaDraftName, gameModeDraft);
		}

		private void InternalShowWindow(string backgroundSpriteName, string arenaDraftName, string gameModeDraft)
		{
			if (!SpectatorController.IsSpectating)
			{
				GameHubBehaviour.Hub.Characters.Async().ClientSendCounselorActivation(GameHubBehaviour.Hub.Options.Game.CounselorActive);
			}
			this.BackgroundSprite.SpriteName = backgroundSpriteName;
			this.ArenaTitleLabel.text = Language.Get(arenaDraftName, TranslationSheets.MainMenuGui);
			this.GameModeLabel.text = Language.Get(gameModeDraft, TranslationSheets.Loading);
			this.WindowGameObject.SetActive(true);
			this.WindowPanel.alpha = 1f;
			this._isLoading = true;
			this.AllyTeamGrid.hideInactive = false;
			this.EnemyTeamGrid.hideInactive = false;
			bool flag = this._enableTeamFlip && GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex].TugOfWarFlipTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
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
					loadingVersusPlayer.UpdatePlayerInfo(this, playerData, flag);
					loadingVersusPlayer.transform.localScale = Vector3.one;
				}
			}
			this.AllyTeamGrid.hideInactive = true;
			this.EnemyTeamGrid.hideInactive = true;
			this.AllyTeamGrid.Reposition();
			this.EnemyTeamGrid.Reposition();
			int num = Language.GetSheetCount(TranslationSheets.LoadingHint) - 1;
			int num2 = UnityEngine.Random.Range(1, num + 1);
			string titleText = Language.Get("LOADING_HINT_TITLE", TranslationSheets.LoadingHint);
			string descriptionText = Language.Get("LOADING_HINT_" + num2, TranslationSheets.LoadingHint);
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
		}

		public void HideWindow()
		{
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
			while (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.PreReplay || GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay)
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
			GameHubBehaviour.Hub.Server.ClientSendPlayerLoadingInfo(SingletonMonoBehaviour<LoadingManager>.Instance.LoadingProgress);
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
			TeamUtils.GetGroupTeamAsync(GameHubBehaviour.Hub, TeamKind.Blue, delegate(Team team)
			{
				this.SetGroupTeamInfo(TeamKind.Blue, team);
			}, delegate(Exception exception)
			{
				LoadingVersusController.Log.Error(string.Format("Error on GetGroupTeamAsync [{0}]. Exception:{1}", TeamKind.Blue, exception));
			});
			TeamUtils.GetGroupTeamAsync(GameHubBehaviour.Hub, TeamKind.Red, delegate(Team team)
			{
				this.SetGroupTeamInfo(TeamKind.Red, team);
			}, delegate(Exception exception)
			{
				LoadingVersusController.Log.Error(string.Format("Error on GetGroupTeamAsync [{0}]. Exception:{1}", TeamKind.Red, exception));
			});
			bool flag = this._enableTeamFlip && GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex].TugOfWarFlipTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			this.AllyTeamGameObject.transform.parent = ((!flag) ? this.LeftFlagTeamPivot.transform : this.RightFlagTeamPivot.transform);
			this.AllyTeamGameObject.transform.localPosition = Vector3.zero;
			this.EnemyTeamGameObject.transform.parent = ((!flag) ? this.RightFlagTeamPivot.transform : this.LeftFlagTeamPivot.transform);
			this.EnemyTeamGameObject.transform.localPosition = Vector3.zero;
		}

		private void SetGroupTeamInfo(TeamKind teamKind, Team team)
		{
			if (team == null)
			{
				return;
			}
			bool flag = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == teamKind;
			if (flag)
			{
				this.AllyTeamIconSprite.SpriteName = team.ImageUrl;
				this.AllyTeamNameLabel.text = string.Format("[{0}]", team.Tag);
				this.AllyTeamGameObject.SetActive(true);
			}
			else
			{
				this.EnemyTeamIconSprite.SpriteName = team.ImageUrl;
				this.EnemyTeamNameLabel.text = string.Format("[{0}]", team.Tag);
				this.EnemyTeamGameObject.SetActive(true);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(LoadingVersusController));

		private const float HideWindowDelayInSec = 0.5f;

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

		public GameObject EnemyTeamGameObject;

		public HMMUI2DDynamicSprite EnemyTeamIconSprite;

		public UILabel EnemyTeamNameLabel;

		[SerializeField]
		private bool _enableTeamFlip;

		private TimedUpdater _timedUpdater;

		private FMODAudioManager.FMODAudio audioSnapshotPlayback;

		private bool _isLoading;

		private LoadingVersusPlayer[] _allyObjects;

		private LoadingVersusPlayer[] _enemyObjects;

		[Serializable]
		private struct ArenaGui
		{
			public int ArenaIndex;

			public GameObject GroupGameObject;

			public GameObject FlippedGroupGameObject;
		}
	}
}
