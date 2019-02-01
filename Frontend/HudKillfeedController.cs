using System;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudKillfeedController : HudFeedController<HudKillfeedObject.HudKillfeedGuiData>
	{
		public override void Awake()
		{
			if (this.FeedMaxSize <= 0)
			{
				this.FeedMaxSize = 8;
			}
			base.Awake();
		}

		public void Start()
		{
			GameHubBehaviour.Hub.Events.Annoucer.ListenToEvent += this.OnAnnouncerEvent;
		}

		public void OnDestroy()
		{
			GameHubBehaviour.Hub.Events.Annoucer.ListenToEvent -= this.OnAnnouncerEvent;
		}

		private void OnAnnouncerEvent(AnnouncerManager.QueuedAnnouncerLog announcerLog)
		{
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay)
			{
				return;
			}
			AnnouncerEvent announcerEvent = announcerLog.AnnouncerEvent;
			if (announcerEvent.AnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.PlayerKilledByEnvironment)
			{
				this.ShowSuicideFeedback(announcerEvent.Victim);
				return;
			}
			if (announcerEvent.AnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.PlayerKilledByEnvironmentWithAssists && announcerEvent.AnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayer && announcerEvent.AnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayerWithAssists)
			{
				return;
			}
			if (announcerEvent.AnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.PlayerKilledByEnvironmentWithAssists)
			{
				announcerEvent.Killer = announcerEvent.AssistPlayers[0];
			}
			this.ShowKillFeedback(announcerEvent.Killer, announcerEvent.Victim);
		}

		private void ShowSuicideFeedback(int victim)
		{
			Color color;
			Sprite bgSprite;
			this.GetPlayerData(victim, out color, out bgSprite);
			HudKillfeedObject.HudKillfeedPlayerData victimPlayerData = new HudKillfeedObject.HudKillfeedPlayerData
			{
				Id = victim,
				Color = color,
				BgSprite = bgSprite
			};
			HudKillfeedObject.HudKillfeedGuiData data = new HudKillfeedObject.HudKillfeedGuiData(this.SuicideSprite, victimPlayerData, this.MaxPlayerNameChar);
			base.PushToFeed(data);
		}

		private void ShowKillFeedback(int killer, int victim)
		{
			Color color;
			Sprite bgSprite;
			this.GetPlayerData(killer, out color, out bgSprite);
			Color color2;
			Sprite bgSprite2;
			this.GetPlayerData(victim, out color2, out bgSprite2);
			HudKillfeedObject.HudKillfeedPlayerData killerPlayerData = new HudKillfeedObject.HudKillfeedPlayerData
			{
				Id = killer,
				Color = color,
				BgSprite = bgSprite
			};
			HudKillfeedObject.HudKillfeedPlayerData victimPlayerData = new HudKillfeedObject.HudKillfeedPlayerData
			{
				Id = victim,
				Color = color2,
				BgSprite = bgSprite2
			};
			HudKillfeedObject.HudKillfeedGuiData data = new HudKillfeedObject.HudKillfeedGuiData(killerPlayerData, this.GetKillerCenterSprite(killer), victimPlayerData, this.MaxPlayerNameChar);
			base.PushToFeed(data);
		}

		private Sprite GetKillerCenterSprite(int killerId)
		{
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(killerId);
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId == killerId)
			{
				return this.CurrentPlayerKillSprite;
			}
			return (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team != playerOrBotsByObjectId.Team) ? this.EnemyKillSprite : this.AllyKillSprite;
		}

		private void GetPlayerData(int playerId, out Color playerColor, out Sprite bgSprite)
		{
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(playerId);
			bool flag = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == playerOrBotsByObjectId.Team;
			playerColor = GUIColorsInfo.Instance.RedTeamColor;
			bgSprite = this.EnemyBgSprite;
			if (flag)
			{
				playerColor = ((GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId != playerId) ? GUIColorsInfo.Instance.BlueTeamColor : GUIColorsInfo.Instance.MyColor);
				bgSprite = ((GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId != playerId) ? this.AllyBgSprite : this.CurrentPlayerBgSprite);
			}
		}

		public int MaxPlayerNameChar = 15;

		public Sprite CurrentPlayerBgSprite;

		public Sprite AllyBgSprite;

		public Sprite EnemyBgSprite;

		public Sprite CurrentPlayerKillSprite;

		public Sprite AllyKillSprite;

		public Sprite EnemyKillSprite;

		public Sprite SuicideSprite;
	}
}
