using System;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HudKillfeedControllerUnityUI : HudFeedController<HudKillfeedObjectUnityUI.HudKillfeedGuiData>
	{
		private void InitializeCaches()
		{
			this._playerBotNameCache = new StringCache<int>(new Func<int, string>(this.BotNameFormatFunction), 8);
			this._playerNameCache = new StringCache<int>(new Func<int, string>(this.PlayerNameFormatFunction), 8);
			this._characterSpriteCache = new SpriteCache(8);
			for (int i = 0; i < this._matchPlayers.PlayersAndBots.Count; i++)
			{
				this._playerNameCache.GenerateValue(this._matchPlayers.PlayersAndBots[i].PlayerCarId);
				this._playerBotNameCache.GenerateValue(this._matchPlayers.PlayersAndBots[i].PlayerCarId);
				Guid id = this._matchPlayers.PlayersAndBots[i].CharacterItemType.Id;
				string roundLookRightIconName = this._getCharacterData.Get(id).RoundLookRightIconName;
				this._characterSpriteCache.PreCacheAsync(roundLookRightIconName);
			}
		}

		private string PlayerNameFormatFunction(int i)
		{
			PlayerData playerOrBotsByObjectId = this._matchPlayers.GetPlayerOrBotsByObjectId(i);
			if (playerOrBotsByObjectId == null)
			{
				return string.Empty;
			}
			string shortName = GUIUtils.GetShortName(playerOrBotsByObjectId.Name, this.MaxPlayerNameChar);
			if (playerOrBotsByObjectId.IsBot)
			{
				return shortName;
			}
			return this._getDisplayableNickName.GetFormattedNickNameWithPlayerTag(playerOrBotsByObjectId.PlayerId, shortName, new long?(playerOrBotsByObjectId.PlayerTag));
		}

		private string BotNameFormatFunction(int i)
		{
			PlayerData playerOrBotsByObjectId = this._matchPlayers.GetPlayerOrBotsByObjectId(i);
			if (playerOrBotsByObjectId == null)
			{
				return string.Empty;
			}
			string characterBotLocalizedName = playerOrBotsByObjectId.GetCharacterBotLocalizedName();
			return GUIUtils.GetShortName(characterBotLocalizedName, this.MaxPlayerNameChar);
		}

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
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				return;
			}
			this.InitializeCaches();
			this._announcerService.ListenToEvent += this.OnAnnouncerEvent;
		}

		public void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				return;
			}
			this._announcerService.ListenToEvent -= this.OnAnnouncerEvent;
		}

		private void OnAnnouncerEvent(QueuedAnnouncerLog announcerLog)
		{
			if (this._scoreBoard.CurrentState == BombScoreboardState.Replay)
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

		private string GetPlayerName(int id)
		{
			PlayerData playerOrBotsByObjectId = this._matchPlayers.GetPlayerOrBotsByObjectId(id);
			return (playerOrBotsByObjectId.IsBot || !playerOrBotsByObjectId.IsBotControlled) ? this._playerNameCache.Get(id) : this._playerBotNameCache.Get(id);
		}

		private Sprite GetCharacterSprite(int id)
		{
			PlayerData playerOrBotsByObjectId = this._matchPlayers.GetPlayerOrBotsByObjectId(id);
			Guid id2 = playerOrBotsByObjectId.CharacterItemType.Id;
			string roundLookRightIconName = this._getCharacterData.Get(id2).RoundLookRightIconName;
			return this._characterSpriteCache.Get(roundLookRightIconName);
		}

		private void ShowSuicideFeedback(int victim)
		{
			Color color;
			Sprite bgSprite;
			this.GetPlayerData(victim, out color, out bgSprite);
			HudKillfeedObjectUnityUI.HudKillfeedPlayerData victimPlayerData = new HudKillfeedObjectUnityUI.HudKillfeedPlayerData
			{
				Id = victim,
				Color = color,
				BgSprite = bgSprite,
				Name = this.GetPlayerName(victim),
				CharacterSprite = this.GetCharacterSprite(victim)
			};
			HudKillfeedObjectUnityUI.HudKillfeedGuiData data = new HudKillfeedObjectUnityUI.HudKillfeedGuiData(this.SuicideSprite, victimPlayerData, this.MaxPlayerNameChar);
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
			HudKillfeedObjectUnityUI.HudKillfeedPlayerData killerPlayerData = new HudKillfeedObjectUnityUI.HudKillfeedPlayerData
			{
				Id = killer,
				Color = color,
				BgSprite = bgSprite,
				Name = this.GetPlayerName(killer),
				CharacterSprite = this.GetCharacterSprite(killer)
			};
			HudKillfeedObjectUnityUI.HudKillfeedPlayerData victimPlayerData = new HudKillfeedObjectUnityUI.HudKillfeedPlayerData
			{
				Id = victim,
				Color = color2,
				BgSprite = bgSprite2,
				Name = this.GetPlayerName(victim),
				CharacterSprite = this.GetCharacterSprite(victim)
			};
			HudKillfeedObjectUnityUI.HudKillfeedGuiData data = new HudKillfeedObjectUnityUI.HudKillfeedGuiData(killerPlayerData, this.GetKillerCenterSprite(killer), victimPlayerData, this.MaxPlayerNameChar);
			base.PushToFeed(data);
		}

		private Sprite GetKillerCenterSprite(int killerId)
		{
			TeamKind teamKindById = this._matchPlayers.GetTeamKindById(killerId);
			if (this._matchPlayers.CurrentPlayerData.PlayerCarId == killerId)
			{
				return this.CurrentPlayerKillSprite;
			}
			return (this._matchPlayers.CurrentPlayerData.Team != teamKindById) ? this.EnemyKillSprite : this.AllyKillSprite;
		}

		private void GetPlayerData(int playerId, out Color playerColor, out Sprite bgSprite)
		{
			TeamKind teamKindById = this._matchPlayers.GetTeamKindById(playerId);
			bool flag = this._matchPlayers.CurrentPlayerData.Team == teamKindById;
			playerColor = this._guiColorsInfo.RedTeamColor;
			bgSprite = this.EnemyBgSprite;
			if (flag)
			{
				playerColor = ((this._matchPlayers.CurrentPlayerData.PlayerCarId != playerId) ? this._guiColorsInfo.BlueTeamColor : this._guiColorsInfo.MyColor);
				bgSprite = ((this._matchPlayers.CurrentPlayerData.PlayerCarId != playerId) ? this.AllyBgSprite : this.CurrentPlayerBgSprite);
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

		[Inject]
		private IAnnouncerService _announcerService;

		[Inject]
		private IScoreBoard _scoreBoard;

		[Inject]
		private IMatchPlayers _matchPlayers;

		[Inject]
		private IMatchTeams _teams;

		[Inject]
		private IGUIColorsInfo _guiColorsInfo;

		[Inject]
		private IGetCharacterData _getCharacterData;

		[Inject]
		private IGetDisplayableNickName _getDisplayableNickName;

		[Inject]
		private ITeamNameRestriction _teamNameRestriction;

		private StringCache<int> _playerNameCache;

		private StringCache<int> _playerBotNameCache;

		private SpriteCache _characterSpriteCache;
	}
}
