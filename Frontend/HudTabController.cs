using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using ClientAPI.Objects;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudTabController : HudWindow, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		private GameGui GameGui
		{
			get
			{
				GameGui result;
				if ((result = this._gameGui) == null)
				{
					result = (this._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>());
				}
				return result;
			}
		}

		public void OnPlayerBuildComplete(PlayerBuildComplete evt)
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				return;
			}
			this._playerBuildCount++;
			if (this._playerBuildCount < GameHubBehaviour.Hub.Players.PlayersAndBots.Count)
			{
				return;
			}
			this.BuildCompleteAllPlayers();
			this._playerBuildCount = 0;
			this.FixGridPosition();
		}

		private void BuildCompleteAllPlayers()
		{
			List<PlayerData> list = new List<PlayerData>(GameHubBehaviour.Hub.Players.PlayersAndBots);
			HudUtils.PlayerDataComparer comparer = new HudUtils.PlayerDataComparer(GameHubBehaviour.Hub, HudUtils.PlayerDataComparer.PlayerDataComparerType.InstanceId);
			list.Sort(comparer);
			for (int i = 0; i < list.Count; i++)
			{
				PlayerData playerData = list[i];
				CombatObject bitComponent = playerData.CharacterInstance.GetBitComponent<CombatObject>();
				bool flag = !GameHubBehaviour.Hub.User.IsNarrator && playerData.CharacterInstance.ObjId == GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId;
				bool isAlly = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == playerData.Team;
				if (flag)
				{
					this.AllyPlayers[0].gameObject.SetActive(true);
					this.AllyPlayers[0].Setup(bitComponent);
				}
				else
				{
					this.SetupCombatObject(bitComponent, isAlly);
				}
			}
			this.SetupTeamInfo();
		}

		private void SetupCombatObject(CombatObject combatObject, bool isAlly)
		{
			HudTabPlayer[] array = (!isAlly) ? this.EnemyPlayers : this.AllyPlayers;
			int num = (!isAlly) ? 0 : 1;
			for (int i = num; i < array.Length; i++)
			{
				HudTabPlayer hudTabPlayer = array[i];
				if (hudTabPlayer.IsEmptySlot)
				{
					hudTabPlayer.gameObject.SetActive(true);
					hudTabPlayer.Setup(combatObject);
					break;
				}
			}
		}

		private void FixGridPosition()
		{
			if (this.EnemyPlayers[0].IsEmptySlot)
			{
				return;
			}
			for (int i = 1; i < this.AllyPlayers.Length; i++)
			{
				if (this.AllyPlayers[i].IsEmptySlot)
				{
					Vector3 localPosition = this.EnemyPlayers[0].transform.parent.localPosition;
					localPosition.y = this.AllyPlayers[i].transform.parent.localPosition.y - (float)(this.PlayerBaseSlotSprite.height * i + this.BasePlayerSlotsOffset);
					this.EnemyPlayers[0].transform.parent.localPosition = localPosition;
					return;
				}
			}
		}

		private void Awake()
		{
			HudMegafeedbacksController.EvtAnimationStart += this.OnMegaAnimationPlaying;
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.OnVisibilityChange += this.OtherCompetingWindowOnVisibilityChange;
			this.WindowGameObject.SetActive(false);
			this.ExitGroupGameObject.SetActive(false);
			this._toggleModeButtonClickEventDelegate = new EventDelegate(new EventDelegate.Callback(this.ToggleModeButtonClick));
			this.ToggleModeButton.onClick.Add(this._toggleModeButtonClickEventDelegate);
			this.SetVisibilityMode(HudTabController.VisibilityMode.Statistics);
			for (int i = 0; i < this.AllyPlayers.Length; i++)
			{
				this.AllyPlayers[i].gameObject.SetActive(false);
			}
			for (int j = 0; j < this.EnemyPlayers.Length; j++)
			{
				this.EnemyPlayers[j].gameObject.SetActive(false);
			}
			this._isGameOver = false;
		}

		private void OtherCompetingWindowOnVisibilityChange(bool visible)
		{
			if (visible)
			{
				base.SetWindowVisibility(false);
			}
		}

		private void OnMegaAnimationPlaying(bool animationIsPlaying)
		{
			this._megaFeedbackIsPlaying = animationIsPlaying;
			if (this._megaFeedbackIsPlaying)
			{
				GameHubBehaviour.Hub.GuiScripts.DriverHelper.SetWindowVisibility(false);
				if (this.IsVisible)
				{
					base.SetWindowVisibility(false);
				}
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			this.ToggleModeButton.onClick.Remove(this._toggleModeButtonClickEventDelegate);
			this._toggleModeButtonClickEventDelegate = null;
			this._playerBuildCount = 0;
			this._isGameOver = false;
			HudMegafeedbacksController.EvtAnimationStart -= this.OnMegaAnimationPlaying;
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.OnVisibilityChange -= this.OtherCompetingWindowOnVisibilityChange;
			this._gameGui = null;
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.Announcer.ListenToEvent += this.ListenToAnnouncer;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.Announcer.ListenToEvent -= this.ListenToAnnouncer;
		}

		private void ListenToAnnouncer(AnnouncerManager.QueuedAnnouncerLog announcerLog)
		{
			if (announcerLog.AnnouncerEvent.AnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.BotControllerActivated && announcerLog.AnnouncerEvent.AnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.BotControllerDeactivated)
			{
				return;
			}
			for (int i = 0; i < this.AllyPlayers.Length; i++)
			{
				if (this.AllyPlayers[i].CombatObject != null && this.AllyPlayers[i].CombatObject.Id.ObjId == announcerLog.AnnouncerEvent.Killer)
				{
					this.AllyPlayers[i].UpdatePlayerName(announcerLog.AnnouncerEvent.AnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.BotControllerActivated);
					return;
				}
			}
			for (int j = 0; j < this.EnemyPlayers.Length; j++)
			{
				if (this.EnemyPlayers[j].CombatObject != null && this.EnemyPlayers[j].CombatObject.Id.ObjId == announcerLog.AnnouncerEvent.Killer)
				{
					this.EnemyPlayers[j].UpdatePlayerName(announcerLog.AnnouncerEvent.AnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.BotControllerActivated);
				}
			}
		}

		private void HudGarageShopOnVisibilityChange(bool visible)
		{
			if (visible && this.IsVisible && !this._isGameOver)
			{
				base.SetWindowVisibility(false);
			}
		}

		private void ToggleModeButtonClick()
		{
			this.SetVisibilityMode((this._visibilityMode != HudTabController.VisibilityMode.Statistics) ? HudTabController.VisibilityMode.Statistics : HudTabController.VisibilityMode.Upgrade);
		}

		private void SetVisibilityMode(HudTabController.VisibilityMode visibilityMode)
		{
			this._visibilityMode = visibilityMode;
			this.TitleGroupStatisticsGameObject.SetActive(this._visibilityMode == HudTabController.VisibilityMode.Statistics);
			this.TitleGroupUpgradeGameObject.SetActive(this._visibilityMode == HudTabController.VisibilityMode.Upgrade);
			for (int i = 0; i < this.AllyPlayers.Length; i++)
			{
				this.AllyPlayers[i].SetVisibilityMode(this._visibilityMode);
			}
			for (int j = 0; j < this.EnemyPlayers.Length; j++)
			{
				this.EnemyPlayers[j].SetVisibilityMode(this._visibilityMode);
			}
			string key = (this._visibilityMode != HudTabController.VisibilityMode.Statistics) ? "TAB_BUTTON_STATISTICS" : "TAB_BUTTON_UPGRADE";
			this.ToggleModeButtonLabel.text = Language.Get(key, TranslationSheets.GUI);
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.MatchMan == null)
			{
				return;
			}
			if (!this._isGameOver && !this._megaFeedbackIsPlaying && !SpectatorController.IsSpectating)
			{
				bool button = ControlOptions.GetButton(ControlAction.GUIOpenScore);
				if (button && !this.IsVisible)
				{
					base.SetWindowVisibility(true);
				}
				else if (!button && this.IsVisible)
				{
					base.SetWindowVisibility(false);
				}
			}
			if (!this.IsVisible)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == TeamKind.Blue)
			{
				this.AllyKillsLabel.text = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue.ToString();
				this.EnemyKillsLabel.text = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed.ToString();
			}
			else
			{
				this.AllyKillsLabel.text = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed.ToString();
				this.EnemyKillsLabel.text = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue.ToString();
			}
			if (!this._isGameOver)
			{
				this.TimerUpdate();
			}
		}

		private void TimerUpdate()
		{
			this._time = GameHubBehaviour.Hub.GameTime.MatchTimer.GetTimeSpan();
			if (this._seconds != this._time.Seconds)
			{
				this._seconds = this._time.Seconds;
				this.TimerLabel.text = TimeUtils.FormatTime(this._time);
			}
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			PlayerController bitComponent = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<PlayerController>();
			bitComponent.HudTabInterfaceOpen = visible;
			base.ChangeWindowVisibility(visible);
			if (!visible)
			{
				GameHubBehaviour.Hub.CursorManager.Pop();
				return;
			}
			GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.MatchstatsCursor);
			this.UpdateTopStatsInfo(true, this.AllyPlayers);
			this.UpdateTopStatsInfo(false, this.EnemyPlayers);
			this.SetVisibilityMode(HudTabController.VisibilityMode.Statistics);
			for (int i = 0; i < this.AllyPlayers.Length; i++)
			{
				HudTabPlayer hudTabPlayer = this.AllyPlayers[i];
				if (hudTabPlayer.CombatObject != null)
				{
					hudTabPlayer.UpdatePlayerName(!hudTabPlayer.CombatObject.Player.IsBot && hudTabPlayer.CombatObject.Player.IsBotControlled);
				}
			}
			for (int j = 0; j < this.EnemyPlayers.Length; j++)
			{
				HudTabPlayer hudTabPlayer2 = this.EnemyPlayers[j];
				if (hudTabPlayer2.CombatObject != null)
				{
					hudTabPlayer2.UpdatePlayerName(!hudTabPlayer2.CombatObject.Player.IsBot && hudTabPlayer2.CombatObject.Player.IsBotControlled);
				}
			}
		}

		private int UpdateTopStatsInfo(bool isAlly, HudTabPlayer[] hudTabPlayers)
		{
			int num = 0;
			List<UILabel> list = new List<UILabel>();
			List<UILabel> list2 = new List<UILabel>();
			List<UILabel> list3 = new List<UILabel>();
			List<UILabel> list4 = new List<UILabel>();
			List<UILabel> list5 = new List<UILabel>();
			List<UILabel> list6 = new List<UILabel>();
			List<UILabel> list7 = new List<UILabel>();
			int num2 = -1;
			int num3 = 999999999;
			int num4 = -1;
			float num5 = -1f;
			float num6 = -1f;
			float num7 = -1f;
			float num8 = -1f;
			Color color = (!isAlly) ? this.StatsTopEnemyColor : this.StatsTopAllyColor;
			Color color2 = (!isAlly) ? this.StatsNormalEnemyColor : this.StatsNormalAllyColor;
			foreach (HudTabPlayer hudTabPlayer in hudTabPlayers)
			{
				if (!hudTabPlayer.IsEmptySlot)
				{
					PlayerStats playerStats = hudTabPlayer.RefreshInfo();
					num += playerStats.ScrapCollected;
					int num9 = int.Parse(hudTabPlayer.PlayerKillsLabel.text);
					if (num9 >= num2)
					{
						num2 = num9;
					}
					int num10 = int.Parse(hudTabPlayer.DeathsLabel.text);
					if (num10 <= num3)
					{
						num3 = num10;
					}
					int num11 = int.Parse(hudTabPlayer.LastHitsLabel.text);
					if (num11 >= num4)
					{
						num4 = num11;
					}
					int num12 = int.Parse(hudTabPlayer.DamageDoneLabel.text);
					if ((float)num12 >= num5)
					{
						num5 = (float)num12;
					}
					int num13 = int.Parse(hudTabPlayer.RepairDealtLabel.text);
					if ((float)num13 >= num6)
					{
						num6 = (float)num13;
					}
					int num14 = int.Parse(hudTabPlayer.BombTimeLabel.text);
					if ((float)num14 >= num7)
					{
						num7 = (float)num14;
					}
					int num15 = int.Parse(hudTabPlayer.DebuffTimeLabel.text);
					if ((float)num15 >= num8)
					{
						num8 = (float)num15;
					}
				}
			}
			foreach (HudTabPlayer hudTabPlayer2 in hudTabPlayers)
			{
				if (!hudTabPlayer2.IsEmptySlot)
				{
					int num16 = int.Parse(hudTabPlayer2.PlayerKillsLabel.text);
					if (num16 == num2)
					{
						list.Add(hudTabPlayer2.PlayerKillsLabel);
					}
					int num17 = int.Parse(hudTabPlayer2.DeathsLabel.text);
					if (num17 == num3)
					{
						list2.Add(hudTabPlayer2.DeathsLabel);
					}
					int num18 = int.Parse(hudTabPlayer2.LastHitsLabel.text);
					if (num18 == num4)
					{
						list3.Add(hudTabPlayer2.LastHitsLabel);
					}
					int num19 = int.Parse(hudTabPlayer2.DamageDoneLabel.text);
					if ((float)num19 == num5)
					{
						list4.Add(hudTabPlayer2.DamageDoneLabel);
					}
					int num20 = int.Parse(hudTabPlayer2.RepairDealtLabel.text);
					if ((float)num20 == num6)
					{
						list5.Add(hudTabPlayer2.RepairDealtLabel);
					}
					int num21 = int.Parse(hudTabPlayer2.BombTimeLabel.text);
					if ((float)num21 == num7)
					{
						list6.Add(hudTabPlayer2.BombTimeLabel);
					}
					int num22 = int.Parse(hudTabPlayer2.DebuffTimeLabel.text);
					if ((float)num22 == num8)
					{
						list7.Add(hudTabPlayer2.DebuffTimeLabel);
					}
					hudTabPlayer2.PlayerKillsLabel.color = color2;
					hudTabPlayer2.DeathsLabel.color = color2;
					hudTabPlayer2.LastHitsLabel.color = color2;
					hudTabPlayer2.DamageDoneLabel.color = color2;
					hudTabPlayer2.RepairDealtLabel.color = color2;
					hudTabPlayer2.BombTimeLabel.color = color2;
					hudTabPlayer2.DebuffTimeLabel.color = color2;
				}
			}
			if (list != null && list.Count > 0)
			{
				for (int k = 0; k < list.Count; k++)
				{
					UILabel uilabel = list[k];
					uilabel.color = color;
				}
			}
			if (list2 != null && list2.Count > 0)
			{
				for (int l = 0; l < list2.Count; l++)
				{
					UILabel uilabel2 = list2[l];
					uilabel2.color = color;
				}
			}
			if (list3 != null && list3.Count > 0)
			{
				for (int m = 0; m < list3.Count; m++)
				{
					UILabel uilabel3 = list3[m];
					uilabel3.color = color;
				}
			}
			if (list4 != null && list4.Count > 0)
			{
				for (int n = 0; n < list4.Count; n++)
				{
					UILabel uilabel4 = list4[n];
					uilabel4.color = color;
				}
			}
			if (list5 != null && list5.Count > 0)
			{
				for (int num23 = 0; num23 < list5.Count; num23++)
				{
					UILabel uilabel5 = list5[num23];
					uilabel5.color = color;
				}
			}
			if (list6 != null && list6.Count > 0)
			{
				for (int num24 = 0; num24 < list6.Count; num24++)
				{
					UILabel uilabel6 = list6[num24];
					uilabel6.color = color;
				}
			}
			if (list7 != null && list7.Count > 0)
			{
				for (int num25 = 0; num25 < list7.Count; num25++)
				{
					UILabel uilabel7 = list7[num25];
					uilabel7.color = color;
				}
			}
			return num;
		}

		public void DontShowWindowOnGameOver()
		{
			this._isGameOver = true;
			if (this.IsVisible)
			{
				base.SetWindowVisibility(false);
			}
		}

		public void OnClickExitMainMenu()
		{
			this.GameGui.ClearBackToMain();
		}

		public override bool CanOpen()
		{
			return !GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.IsWindowVisible() && (HudWindowManager.Instance.State == HudWindowManager.GuiGameState.Game && !GameHubBehaviour.Hub.GuiScripts.Esc.IsWindowVisible()) && !GameHubBehaviour.Hub.GuiScripts.DriverHelper.IsWindowVisible();
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
				HudTabController.Log.Error(string.Format("Error on GetGroupTeamAsync [{0}]. Exception:{1}", TeamKind.Blue, exception));
			});
			TeamUtils.GetGroupTeamAsync(GameHubBehaviour.Hub, TeamKind.Red, delegate(Team team)
			{
				this.SetGroupTeamInfo(TeamKind.Red, team);
			}, delegate(Exception exception)
			{
				HudTabController.Log.Error(string.Format("Error on GetGroupTeamAsync [{0}]. Exception:{1}", TeamKind.Red, exception));
			});
		}

		private void SetGroupTeamInfo(TeamKind teamKind, Team team)
		{
			if (team == null)
			{
				return;
			}
			string text = string.Format("[{0}]", team.Tag);
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == teamKind)
			{
				this.AllyTeamIconSprite.SpriteName = team.ImageUrl;
				this.AllyTeamNameLabel.text = text;
				this.AllyTeamGameObject.SetActive(true);
			}
			else
			{
				this.EnemyTeamIconSprite.SpriteName = team.ImageUrl;
				this.EnemyTeamNameLabel.text = text;
				this.EnemyTeamGameObject.SetActive(true);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudTabController));

		public int BasePlayerSlotsOffset;

		private GameGui _gameGui;

		public Color StatsNormalAllyColor;

		public Color StatsTopAllyColor;

		public Color StatsNormalEnemyColor;

		public Color StatsTopEnemyColor;

		public Color UpgradeBorderDisabledColor;

		public Color UpgradeBorderEnabledColor;

		public Color UpgradeBorderFullColor;

		public UILabel TimerLabel;

		public UILabel AllyKillsLabel;

		public UILabel EnemyKillsLabel;

		public UIButton ToggleModeButton;

		public UILabel ToggleModeButtonLabel;

		public GameObject TitleGroupStatisticsGameObject;

		public GameObject TitleGroupUpgradeGameObject;

		public UI2DSprite PlayerBaseSlotSprite;

		public GameObject ExitGroupGameObject;

		public int PlayerNameMaxChars = 15;

		public HudTabPlayer[] AllyPlayers;

		public HudTabPlayer[] EnemyPlayers;

		[Header("[Team]")]
		public GameObject AllyTeamGameObject;

		public HMMUI2DDynamicSprite AllyTeamIconSprite;

		public UILabel AllyTeamNameLabel;

		public GameObject EnemyTeamGameObject;

		public HMMUI2DDynamicSprite EnemyTeamIconSprite;

		public UILabel EnemyTeamNameLabel;

		private TimeSpan _time;

		private int _seconds;

		private HudTabController.VisibilityMode _visibilityMode;

		private EventDelegate _toggleModeButtonClickEventDelegate;

		private int _playerBuildCount;

		private bool _isGameOver;

		private bool _megaFeedbackIsPlaying;

		public enum VisibilityMode : byte
		{
			Statistics,
			Upgrade
		}
	}
}
