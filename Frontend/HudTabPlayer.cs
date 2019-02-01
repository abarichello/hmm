using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

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
			for (int i = 0; i < this.GadgetInfos.Count; i++)
			{
				HudTabPlayer.HudTabGadgetInfo hudTabGadgetInfo = this.GadgetInfos[i];
				if (hudTabGadgetInfo.SkillFullSprite != null)
				{
					hudTabGadgetInfo.SkillFullSprite.gameObject.SetActive(false);
				}
				hudTabGadgetInfo.Upgrade0Sprite.gameObject.SetActive(false);
				hudTabGadgetInfo.Upgrade0BorderSprite.gameObject.SetActive(true);
				hudTabGadgetInfo.Upgrade0BorderSprite.color = this.HudTabController.UpgradeBorderDisabledColor;
				hudTabGadgetInfo.Upgrade1Sprite.gameObject.SetActive(false);
				hudTabGadgetInfo.Upgrade1BorderSprite.gameObject.SetActive(true);
				hudTabGadgetInfo.Upgrade1BorderSprite.color = this.HudTabController.UpgradeBorderDisabledColor;
				if (hudTabGadgetInfo.Line0Sprite != null)
				{
					hudTabGadgetInfo.Line0Sprite.gameObject.SetActive(true);
					hudTabGadgetInfo.Line0Sprite.color = this.HudTabController.UpgradeBorderDisabledColor;
				}
				if (hudTabGadgetInfo.Line1Sprite != null)
				{
					hudTabGadgetInfo.Line1Sprite.gameObject.SetActive(true);
					hudTabGadgetInfo.Line1Sprite.color = this.HudTabController.UpgradeBorderDisabledColor;
				}
				if (hudTabGadgetInfo.NumMaxUpgrades < 2)
				{
					if (hudTabGadgetInfo.Line1Sprite != null)
					{
						hudTabGadgetInfo.Line1Sprite.gameObject.SetActive(false);
					}
					hudTabGadgetInfo.Upgrade1BorderSprite.transform.parent.gameObject.SetActive(false);
				}
			}
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
				this._combatObject.CustomGadget0.ListenToGadgetSetLevel += this.OnGadgetSetLevel;
				this._combatObject.CustomGadget1.ListenToGadgetSetLevel += this.OnGadgetSetLevel;
				this._combatObject.CustomGadget2.ListenToGadgetSetLevel += this.OnGadgetSetLevel;
				this._combatObject.GenericGadget.ListenToGadgetSetLevel += this.OnGadgetSetLevel;
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
				this._combatObject.CustomGadget0.ListenToGadgetSetLevel -= this.OnGadgetSetLevel;
				this._combatObject.CustomGadget1.ListenToGadgetSetLevel -= this.OnGadgetSetLevel;
				this._combatObject.CustomGadget2.ListenToGadgetSetLevel -= this.OnGadgetSetLevel;
				this._combatObject.GenericGadget.ListenToGadgetSetLevel -= this.OnGadgetSetLevel;
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
			if (this._gadgetUpgrades != null)
			{
				this._gadgetUpgrades.Clear();
				this._gadgetUpgrades = null;
			}
		}

		public void SetVisibilityMode(HudTabController.VisibilityMode visibilityMode)
		{
			this.TitleGroupStatisticsGameObject.SetActive(visibilityMode == HudTabController.VisibilityMode.Statistics);
			this.TitleGroupUpgradeGameObject.SetActive(visibilityMode == HudTabController.VisibilityMode.Upgrade);
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
			GUIUtils.ClampLabel(this.CharacterLabel, player.Character.LocalizedName);
			this.UpdatePlayerName(player.IsBotControlled);
			GadgetBehaviour customGadget = combatObject.CustomGadget0;
			GadgetBehaviour customGadget2 = combatObject.CustomGadget1;
			GadgetBehaviour customGadget3 = combatObject.CustomGadget2;
			GadgetBehaviour genericGadget = combatObject.GenericGadget;
			this.InitGadgetUpgradeLocalCache(customGadget);
			this.InitGadgetUpgradeLocalCache(customGadget2);
			this.InitGadgetUpgradeLocalCache(customGadget3);
			this.InitGadgetUpgradeLocalCache(genericGadget);
			this.Gadget0Sprite.SpriteName = HudUtils.GetGadgetIconName(player.Character, customGadget.Slot);
			this.Gadget1Sprite.SpriteName = HudUtils.GetGadgetIconName(player.Character, customGadget2.Slot);
			this.Gadget2Sprite.SpriteName = HudUtils.GetGadgetIconName(player.Character, customGadget3.Slot);
			this.ActivateListeners();
			this._isAlive = this._combatObject.IsAlive();
			this._voiceChatStatusChangerGuiButton.Setup(player.UserId, player.IsBot, player.Team != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team);
			if (GameHubBehaviour.Hub.BombManager.Rules.InstancesEnabled)
			{
				this.InstanceSprite.gameObject.SetActive(true);
				this.InstanceSprite.SpriteName = HudUtils.GetCurrentInstanceIconName(player, true);
				this.InstanceTooltipTrigger.TooltipText = HudUtils.GetCurrentInstanceDescription(player);
			}
		}

		public void UpdatePlayerName(bool useLocalizedBotName)
		{
			this.PlayerLabel.color = Color.white;
			string arg = HudUtils.RGBToHex(GUIColorsInfo.GetPlayerColor(this._combatObject.Player.PlayerId, this._combatObject.Player.Team));
			string text = (!useLocalizedBotName) ? this._combatObject.Player.Name : this._combatObject.Player.Character.LocalizedBotName;
			string formattedPlayerName = string.Format("[{0}]{1}[-]", arg, NGUIText.EscapeSymbols(text));
			GUIUtils.ClampLabel(this.PlayerLabel, formattedPlayerName);
			if (!useLocalizedBotName && !this._combatObject.Player.IsBot)
			{
				TeamUtils.GetUserTagAsync(GameHubBehaviour.Hub, this._combatObject.Player.UserId, delegate(string teamTag)
				{
					if (!string.IsNullOrEmpty(teamTag))
					{
						GUIUtils.ClampLabel(this.PlayerLabel, string.Format("{0} {1}", teamTag, formattedPlayerName));
					}
				}, delegate(Exception exception)
				{
					HudTabPlayer.Log.WarnFormat("Error on GetUserTagAsync. Exception:{0}", new object[]
					{
						exception
					});
				});
			}
		}

		private void OnCombatObjecSpawn(CombatObject obj, SpawnEvent msg)
		{
			this._isAlive = true;
		}

		private void OnCombatObjecUnspawn(CombatObject obj, UnspawnEvent msg)
		{
			this._isAlive = false;
		}

		private void InitGadgetUpgradeLocalCache(GadgetBehaviour gadgetBehaviour)
		{
			if (this._gadgetUpgrades == null)
			{
				this._gadgetUpgrades = new Dictionary<string, int>(10);
			}
			for (int i = 0; i < gadgetBehaviour.Upgrades.Length; i++)
			{
				this._gadgetUpgrades[gadgetBehaviour.Upgrades[i].Info.Name] = 0;
			}
		}

		private void OnGadgetSetLevel(GadgetBehaviour gadget, string upgradeName, int level)
		{
			if (!gadget.IsUpgradeInShop(upgradeName))
			{
				return;
			}
			this._gadgetUpgrades[upgradeName] = level;
			HudTabPlayer.HudTabGadgetInfo hudTabGadgetInfo = default(HudTabPlayer.HudTabGadgetInfo);
			for (int i = 0; i < this.GadgetInfos.Count; i++)
			{
				HudTabPlayer.HudTabGadgetInfo hudTabGadgetInfo2 = this.GadgetInfos[i];
				if (hudTabGadgetInfo2.GadgetSlot == gadget.Slot)
				{
					hudTabGadgetInfo = hudTabGadgetInfo2;
					break;
				}
			}
			if (hudTabGadgetInfo.SkillFullSprite != null)
			{
				hudTabGadgetInfo.SkillFullSprite.gameObject.SetActive(false);
			}
			hudTabGadgetInfo.Upgrade0Sprite.gameObject.SetActive(false);
			hudTabGadgetInfo.Upgrade1Sprite.gameObject.SetActive(false);
			if (hudTabGadgetInfo.Line0Sprite != null)
			{
				hudTabGadgetInfo.Line0Sprite.color = this.HudTabController.UpgradeBorderDisabledColor;
			}
			if (hudTabGadgetInfo.Line1Sprite != null)
			{
				hudTabGadgetInfo.Line1Sprite.color = this.HudTabController.UpgradeBorderDisabledColor;
			}
			int num = 0;
			for (int j = 0; j < gadget.Upgrades.Length; j++)
			{
				GadgetBehaviour.UpgradeInstance upgradeInstance = gadget.Upgrades[j];
				if (this._gadgetUpgrades[upgradeInstance.Info.Name] > 0)
				{
					string gadgetUpgradeIconName = HudUtils.GetGadgetUpgradeIconName(gadget.Combat.Player.Character.Asset, gadget.Slot, gadget.Info, upgradeInstance.Info, false);
					if (num == 0)
					{
						hudTabGadgetInfo.Upgrade0Sprite.gameObject.SetActive(true);
						hudTabGadgetInfo.Upgrade0BorderSprite.color = this.HudTabController.UpgradeBorderEnabledColor;
						if (hudTabGadgetInfo.Line0Sprite != null)
						{
							hudTabGadgetInfo.Line0Sprite.color = this.HudTabController.UpgradeBorderEnabledColor;
						}
					}
					else
					{
						hudTabGadgetInfo.Upgrade1Sprite.gameObject.SetActive(true);
						hudTabGadgetInfo.Upgrade1BorderSprite.color = this.HudTabController.UpgradeBorderEnabledColor;
						if (hudTabGadgetInfo.Line1Sprite != null)
						{
							hudTabGadgetInfo.Line1Sprite.color = this.HudTabController.UpgradeBorderEnabledColor;
						}
					}
					num++;
				}
			}
			if (num == hudTabGadgetInfo.NumMaxUpgrades)
			{
				if (hudTabGadgetInfo.SkillFullSprite != null)
				{
					hudTabGadgetInfo.SkillFullSprite.gameObject.SetActive(true);
				}
				hudTabGadgetInfo.Upgrade0BorderSprite.color = this.HudTabController.UpgradeBorderFullColor;
				hudTabGadgetInfo.Upgrade1BorderSprite.color = this.HudTabController.UpgradeBorderFullColor;
				if (hudTabGadgetInfo.Line0Sprite != null)
				{
					hudTabGadgetInfo.Line0Sprite.color = this.HudTabController.UpgradeBorderFullColor;
				}
				if (hudTabGadgetInfo.Line1Sprite != null)
				{
					hudTabGadgetInfo.Line1Sprite.color = this.HudTabController.UpgradeBorderFullColor;
				}
			}
			if (GameHubBehaviour.Hub.BombManager.Rules.InstancesEnabled)
			{
				this.InstanceSprite.SpriteName = HudUtils.GetCurrentInstanceIconName(this._combatObject.Player, true);
				this.InstanceTooltipTrigger.TooltipText = HudUtils.GetCurrentInstanceDescription(this._combatObject.Player);
			}
		}

		public PlayerStats RefreshInfo()
		{
			if (this.IsEmptySlot)
			{
				return null;
			}
			PlayerStats component = this._combatObject.GetComponent<PlayerStats>();
			this.PlayerKillsLabel.text = component.KillsAndAssists.ToString();
			this.DeathsLabel.text = component.Deaths.ToString();
			this.LastHitsLabel.text = component.Kills.ToString();
			this.DamageDoneLabel.text = string.Format(Language.Get("INFO_PLAYER_STATS_GENERAL_DAMAGE_VALUE", TranslationSheets.MatchEndScreen), (int)component.DamageDealtToPlayers);
			this.RepairDealtLabel.text = string.Format(Language.Get("INFO_PLAYER_STATS_GENERAL_REPAIR_VALUE", TranslationSheets.MatchEndScreen), (int)component.HealingProvided);
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

		public HudTabController HudTabController;

		public HMMUI2DDynamicSprite CharacterTexture;

		public HMMUI2DDynamicSprite FounderBorderSprite;

		public UILabel CharacterLabel;

		public UILabel PlayerLabel;

		public HMMUI2DDynamicSprite InstanceSprite;

		public HMMTooltipTrigger InstanceTooltipTrigger;

		public GameObject TitleGroupStatisticsGameObject;

		public GameObject TitleGroupUpgradeGameObject;

		public List<HudTabPlayer.HudTabGadgetInfo> GadgetInfos;

		public HMMUI2DDynamicSprite Gadget0Sprite;

		public HMMUI2DDynamicSprite Gadget1Sprite;

		public HMMUI2DDynamicSprite Gadget2Sprite;

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

		private CombatObject _combatObject;

		private SpawnController _spawnController;

		private PlayerStats _playerStats;

		private Dictionary<string, int> _gadgetUpgrades;

		private bool _isAlive;

		public bool isAlly;

		[Serializable]
		public struct HudTabGadgetInfo
		{
			public GadgetSlot GadgetSlot;

			public int NumMaxUpgrades;

			public UI2DSprite SkillSprite;

			public UI2DSprite SkillFullSprite;

			public HMMUI2DDynamicSprite Upgrade0Sprite;

			public UI2DSprite Upgrade0BorderSprite;

			public HMMUI2DDynamicSprite Upgrade1Sprite;

			public UI2DSprite Upgrade1BorderSprite;

			public UI2DSprite Line0Sprite;

			public UI2DSprite Line1Sprite;
		}
	}
}
