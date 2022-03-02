using System;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HudTabPlayer : GameHubBehaviour, ICleanupListener
	{
		public CombatObject CombatObject
		{
			get
			{
				return this._combatObject;
			}
		}

		private void Initialize()
		{
			this.PlayerKillsLabel.color = ((!this.isAlly) ? this.HudTabController.StatsNormalEnemyColor : this.HudTabController.StatsNormalAllyColor);
			this.DeathsLabel.color = ((!this.isAlly) ? this.HudTabController.StatsNormalEnemyColor : this.HudTabController.StatsNormalAllyColor);
			this.LastHitsLabel.color = ((!this.isAlly) ? this.HudTabController.StatsNormalEnemyColor : this.HudTabController.StatsNormalAllyColor);
			this.DamageDoneLabel.color = ((!this.isAlly) ? this.HudTabController.StatsNormalEnemyColor : this.HudTabController.StatsNormalAllyColor);
			this.RepairDealtLabel.color = ((!this.isAlly) ? this.HudTabController.StatsNormalEnemyColor : this.HudTabController.StatsNormalAllyColor);
			this.BombTimeLabel.color = ((!this.isAlly) ? this.HudTabController.StatsNormalEnemyColor : this.HudTabController.StatsNormalAllyColor);
			this.DebuffTimeLabel.color = ((!this.isAlly) ? this.HudTabController.StatsNormalEnemyColor : this.HudTabController.StatsNormalAllyColor);
		}

		private void OnEnable()
		{
			this.ActivateListeners();
		}

		private void OnDisable()
		{
			this.DeactivateListeners();
		}

		private void ActivateListeners()
		{
			if (this._combatObject != null)
			{
				this._combatObject.ListenToObjectSpawn += this.OnCombatObjecSpawn;
				this._combatObject.ListenToObjectUnspawn += this.OnCombatObjecUnspawn;
				this._combatObject.Player.ListenToBotControlChanged += this.ListenToBotControlChanged;
			}
		}

		private void ListenToBotControlChanged(PlayerData obj)
		{
			this.UpdatePlayerName(!obj.IsBot && obj.IsBotControlled);
		}

		private void DeactivateListeners()
		{
			if (this._combatObject != null)
			{
				this._combatObject.ListenToObjectSpawn -= this.OnCombatObjecSpawn;
				this._combatObject.ListenToObjectUnspawn -= this.OnCombatObjecUnspawn;
				this._combatObject.Player.ListenToBotControlChanged -= this.ListenToBotControlChanged;
			}
		}

		private void OnDestroy()
		{
			this._combatObject = null;
			this._spawnController = null;
			this._playerStats = null;
		}

		public void SetVisibilityMode()
		{
			this.TitleGroupStatisticsGameObject.SetActive(true);
		}

		public void Setup(CombatObject combatObject)
		{
			this.Initialize();
			this.IsEmptySlot = false;
			this.DeactivateListeners();
			this._combatObject = combatObject;
			this._spawnController = this._combatObject.Player.CharacterInstance.GetComponent<SpawnController>();
			this._playerStats = this._combatObject.Player.CharacterInstance.GetComponent<PlayerStats>();
			PlayerData player = combatObject.Player;
			this.CharacterTexture.SpriteName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, player.Character.CharacterItemTypeGuid, HudUtils.PlayerIconSize.Size64);
			PortraitDecoratorGui.UpdatePortraitSprite(player.Customizations, this.FounderBorderSprite, PortraitDecoratorGui.PortraitSpriteType.Circle);
			this.CharacterLabel.text = player.GetCharacterLocalizedName();
			this.UpdatePlayerName(player.IsBotControlled);
			this.ActivateListeners();
			this._isAlive = this._combatObject.IsAlive();
			this._voiceChatStatusChangerGuiButton.Setup(player.ConvertToPlayer(), player.IsBot, player.Team != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team);
			this.UpdatePublisherUserName(player);
			this._playerNameInfoGrid.Reposition();
		}

		public void SetupSpectator(PlayerData playerData)
		{
			this.IsEmptySlot = false;
			this._isSpectatorSlot = true;
			this._isAlive = true;
			this.UpdateSpectatorName(playerData);
			this.UpdatePublisherUserName(playerData);
		}

		public void UpdatePlayerName(bool useLocalizedBotName)
		{
			this.PlayerLabel.color = Color.white;
			PlayerData player = this._combatObject.Player;
			string text = (!player.IsBot) ? this._getDisplayableNickName.GetFormattedNickNameWithPlayerTag(player.PlayerId, player.Name, new long?(player.PlayerTag)) : player.Name;
			if (useLocalizedBotName)
			{
				text = player.GetCharacterBotLocalizedName();
			}
			string arg = HudUtils.RGBToHex(GUIColorsInfo.GetPlayerColor(player.PlayerId, player.Team));
			string text2 = string.Format("[{0}]{1}[-]", arg, NGUIText.EscapeSymbols(text));
			this.PlayerLabel.text = text2;
		}

		private void UpdateSpectatorName(PlayerData playerData)
		{
			this.PlayerLabel.color = Color.white;
			string formattedNickNameWithPlayerTag = this._getDisplayableNickName.GetFormattedNickNameWithPlayerTag(playerData.PlayerId, playerData.Name, new long?(playerData.PlayerTag));
			string text = NGUIText.EscapeSymbols(formattedNickNameWithPlayerTag);
			this.PlayerLabel.text = text;
		}

		private void UpdatePublisherUserName(PlayerData playerData)
		{
			if (playerData.IsBot)
			{
				this.PsnIdGroupGameObject.SetActive(false);
				return;
			}
			Publisher publisherById = Publishers.GetPublisherById(playerData.PublisherId);
			PublisherPresentingData publisherPresentingData = this._getPublisherPresentingData.Get(publisherById);
			if (publisherPresentingData.ShouldShowPublisherUserName)
			{
				this.PsnIdGroupGameObject.SetActive(true);
				this.PsnIdLabel.text = playerData.PublisherUserName;
				return;
			}
			this.PsnIdGroupGameObject.SetActive(false);
		}

		private void OnCombatObjecSpawn(CombatObject obj, SpawnEvent msg)
		{
			this._isAlive = true;
		}

		private void OnCombatObjecUnspawn(CombatObject obj, UnspawnEvent msg)
		{
			this._isAlive = false;
		}

		public PlayerStats RefreshInfo()
		{
			if (this.IsEmptySlot || this._isSpectatorSlot)
			{
				return null;
			}
			PlayerStats component = this._combatObject.GetComponent<PlayerStats>();
			this.PlayerKillsLabel.text = component.KillsAndAssists.ToString();
			this.DeathsLabel.text = component.Deaths.ToString();
			this.LastHitsLabel.text = component.Kills.ToString();
			this.DamageDoneLabel.text = Language.GetFormatted("INFO_PLAYER_STATS_GENERAL_DAMAGE_VALUE", TranslationContext.MatchEndScreen, new object[]
			{
				(int)component.DamageDealtToPlayers
			});
			this.RepairDealtLabel.text = Language.GetFormatted("INFO_PLAYER_STATS_GENERAL_REPAIR_VALUE", TranslationContext.MatchEndScreen, new object[]
			{
				(int)component.HealingProvided
			});
			this.BombTimeLabel.text = ((int)component.BombPossessionTime).ToString();
			this.DebuffTimeLabel.text = ((int)component.DebuffTime).ToString();
			return component;
		}

		private void Update()
		{
			if (this._combatObject == null)
			{
				return;
			}
			this.PlayerDisconnectedGameObject.SetActive(this._playerStats.Disconnected);
			if (!this._isAlive)
			{
				this.CharacterTexture.alpha = 0.5f;
				if (!this.KillGroupGameObject.activeSelf)
				{
					this.KillGroupGameObject.SetActive(true);
				}
				int deathTimeRemainingMillis = this._spawnController.GetDeathTimeRemainingMillis();
				this.KillCooldownLabel.text = ((int)((float)deathTimeRemainingMillis / 1000f + 1f)).ToString();
				return;
			}
			this.CharacterTexture.alpha = 1f;
			if (this.KillGroupGameObject.activeSelf)
			{
				this.KillGroupGameObject.SetActive(false);
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this.DeactivateListeners();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudTabPlayer));

		[Inject]
		private IMatchTeams _teams;

		[Inject]
		private IGetDisplayableNickName _getDisplayableNickName;

		[Inject]
		private ITeamNameRestriction _teamNameRestriction;

		[Inject]
		private IGetPublisherPresentingData _getPublisherPresentingData;

		public HudTabController HudTabController;

		public HMMUI2DDynamicSprite CharacterTexture;

		public HMMUI2DDynamicSprite FounderBorderSprite;

		public UILabel CharacterLabel;

		public UILabel PlayerLabel;

		public GameObject TitleGroupStatisticsGameObject;

		public UILabel PlayerKillsLabel;

		public UILabel DeathsLabel;

		public UILabel LastHitsLabel;

		public UILabel DamageDoneLabel;

		public UILabel RepairDealtLabel;

		public UILabel BombTimeLabel;

		public UILabel DebuffTimeLabel;

		public GameObject KillGroupGameObject;

		public UILabel KillCooldownLabel;

		public GameObject PlayerDisconnectedGameObject;

		[SerializeField]
		private VoiceChatStatusChangerGUIButton _voiceChatStatusChangerGuiButton;

		[HideInInspector]
		[NonSerialized]
		public bool IsEmptySlot = true;

		private bool _isSpectatorSlot;

		private CombatObject _combatObject;

		private SpawnController _spawnController;

		private PlayerStats _playerStats;

		private bool _isAlive;

		public bool isAlly;

		[SerializeField]
		private UIGrid _playerNameInfoGrid;

		[SerializeField]
		private GameObject PsnIdGroupGameObject;

		[SerializeField]
		private UILabel PsnIdLabel;
	}
}
