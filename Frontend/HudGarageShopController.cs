using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using FMod;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudGarageShopController : HudWindow, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		public HudGarageShopGadgetObject[] GadgetObjects
		{
			get
			{
				HudGarageShopGadgetObject[] result;
				if ((result = this._gadgetObjects) == null)
				{
					result = (this._gadgetObjects = new HudGarageShopGadgetObject[]
					{
						this.GadgetObject0,
						this.GadgetObject1,
						this.UltimateObject,
						this.GenericObject
					});
				}
				return result;
			}
		}

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

		public override bool CanBeHiddenByEscKey()
		{
			return !GameHubBehaviour.Hub.Match.LevelIsTutorial() && base.CanBeHiddenByEscKey();
		}

		public void Start()
		{
			this._playerBalanceCache = 0;
			this.ChangeWindowVisibility(false);
			this.BalanceAnimationStop();
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnPhaseChange;
			PauseController.OnInGamePauseStateChanged += this.OnGamePauseStateChange;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			this._gadgetObjects = null;
			if (this._combatObject != null)
			{
				this._combatObject.CustomGadget0.ListenToGadgetSetLevel -= this.OnGadgetUpgraded;
				this._combatObject.CustomGadget1.ListenToGadgetSetLevel -= this.OnGadgetUpgraded;
				this._combatObject.CustomGadget2.ListenToGadgetSetLevel -= this.OnGadgetUpgraded;
				this._combatObject.GenericGadget.ListenToGadgetSetLevel -= this.OnGadgetUpgraded;
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnPhaseChange;
			PauseController.OnInGamePauseStateChanged -= this.OnGamePauseStateChange;
			HudGarageShopController.OnShopOpening = null;
			HudGarageShopController.OnShopOpened = null;
			HudGarageShopController.OnShopClosing = null;
			HudGarageShopController.OnShopClosed = null;
			HudGarageShopController.OnAnyGadgetUpgraded = null;
		}

		public void OnPlayerBuildComplete(PlayerBuildComplete evt)
		{
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(evt.Id);
			if (!this.IsCurrentPlayer(playerOrBotsByObjectId))
			{
				return;
			}
			this._combatObject = playerOrBotsByObjectId.CharacterInstance.GetBitComponent<CombatObject>();
			this._combatObject.CustomGadget0.ListenToGadgetSetLevel += this.OnGadgetUpgraded;
			this._combatObject.CustomGadget1.ListenToGadgetSetLevel += this.OnGadgetUpgraded;
			this._combatObject.CustomGadget2.ListenToGadgetSetLevel += this.OnGadgetUpgraded;
			this._combatObject.GenericGadget.ListenToGadgetSetLevel += this.OnGadgetUpgraded;
			this._playerStats = this._combatObject.GetComponent<PlayerStats>();
			this.GadgetObject0.Setup(this._combatObject.CustomGadget0, this._playerStats.Scrap, new HudGarageShopGadgetObject.OnBuyUpgradeDelegate(this.EventOnBuyUpgrade), new HudGarageShopGadgetObject.OnSellUpgradeDelegate(this.EventOnSellUpgrade), this);
			this.GadgetObject1.Setup(this._combatObject.CustomGadget1, this._playerStats.Scrap, new HudGarageShopGadgetObject.OnBuyUpgradeDelegate(this.EventOnBuyUpgrade), new HudGarageShopGadgetObject.OnSellUpgradeDelegate(this.EventOnSellUpgrade), this);
			this.UltimateObject.Setup(this._combatObject.CustomGadget2, this._playerStats.Scrap, new HudGarageShopGadgetObject.OnBuyUpgradeDelegate(this.EventOnBuyUpgrade), new HudGarageShopGadgetObject.OnSellUpgradeDelegate(this.EventOnSellUpgrade), this);
			this.GenericObject.Setup(this._combatObject.GenericGadget, this._playerStats.Scrap, new HudGarageShopGadgetObject.OnBuyUpgradeDelegate(this.EventOnBuyUpgrade), new HudGarageShopGadgetObject.OnSellUpgradeDelegate(this.EventOnSellUpgrade), this);
			this.MapKeyNavigationAfterGadgetSetup();
			this.AddUpgradesAudioListeners(this.GadgetObject0.GadgetUpgrades);
			this.AddUpgradesAudioListeners(this.GadgetObject1.GadgetUpgrades);
			this.AddUpgradesAudioListeners(this.UltimateObject.GadgetUpgrades);
			this.AddUpgradesAudioListeners(this.GenericObject.GadgetUpgrades);
		}

		private void AddUpgradesAudioListeners(List<HudGarageShopGadgetUpgrade> upgrades)
		{
			for (int i = 0; i < upgrades.Count; i++)
			{
				upgrades[i].ListenToGadgetDeniedByBalance += this.OnListenToGadgetDeniedByBalance;
				upgrades[i].ListenToGadgetDeniedByBlock += this.OnListenToGadgetDeniedByBlock;
			}
		}

		private void OnListenToGadgetDeniedByBalance()
		{
			if (this.HudGarageShopSettings.DeniedByBalanceAudioFmodAsset != null && this.IsVisible)
			{
				FMODAudioManager.PlayOneShotAt(this.HudGarageShopSettings.DeniedByBalanceAudioFmodAsset, Vector3.zero, 0);
			}
		}

		private void OnListenToGadgetDeniedByBlock()
		{
			if (this.HudGarageShopSettings.DeniedByLevelAudioFmodAsset != null && this.IsVisible)
			{
				FMODAudioManager.PlayOneShotAt(this.HudGarageShopSettings.DeniedByLevelAudioFmodAsset, Vector3.zero, 0);
			}
		}

		public void EventOnBuyUpgrade(GadgetSlot customGadgetSlot, string upgradeName)
		{
			this.GarageController.SelectInstance(upgradeName);
		}

		public void EventOnSellUpgrade(GadgetSlot customGadgetSlot, string upgradeName, bool isBuyback)
		{
		}

		private bool IsCurrentPlayer(PlayerData playerData)
		{
			return GameHubBehaviour.Hub.Players.CurrentPlayerData != null && GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance != null && GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId == playerData.CharacterInstance.ObjId;
		}

		public override bool CanOpen()
		{
			return this.CanOpenShopWindow();
		}

		private bool CanOpenShopWindow()
		{
			return false;
		}

		public void TryToOpenShop()
		{
			if (this.IsVisible)
			{
				if (!GameHubBehaviour.Hub.Match.LevelIsTutorial())
				{
					this.HideShop();
				}
				return;
			}
			if (!this.CanOpenShopWindow())
			{
				return;
			}
			base.SetWindowVisibility(true);
			this.Refresh();
		}

		private IEnumerator OnOpenWindowAsync()
		{
			if (this.HudGarageShopSettings.OpenWindowFmodAsset != null)
			{
				FMODAudioManager.PlayOneShotAt(this.HudGarageShopSettings.OpenWindowFmodAsset, Vector3.zero, 0);
			}
			if (this.audioSnapshotInstance == null)
			{
				this.audioSnapshotInstance = FMODAudioManager.PlayAt(this.HudGarageShopSettings.audioSnapshot, null);
			}
			yield return UnityUtils.WaitForEndOfFrame;
			for (int i = 0; i < this.GadgetObjects.Length; i++)
			{
				this.GadgetObjects[i].SetTooltipEnable(true);
			}
			PlayerData playerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
			CarComponentHub carComponentHub = playerData.CharacterInstance.GetComponentHub<CarComponentHub>();
			if (carComponentHub != null && carComponentHub.VoiceOverController != null)
			{
				carComponentHub.VoiceOverController.Play(VoiceOverEventGroup.OpenShop);
			}
			for (int j = 0; j < this.GadgetObjects.Length; j++)
			{
				HudGarageShopGadgetObject hudGarageShopGadgetObject = this.GadgetObjects[j];
				UIKeyNavigation component = hudGarageShopGadgetObject.GadgetSkill.GetComponent<UIKeyNavigation>();
				if (component != null && component.startsSelected)
				{
					GUIUtils.ControllerSetSelectedObject(component.gameObject);
					break;
				}
				bool flag = false;
				for (int k = 0; k < hudGarageShopGadgetObject.GadgetUpgrades.Count; k++)
				{
					HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade = hudGarageShopGadgetObject.GadgetUpgrades[k];
					UIKeyNavigation component2 = hudGarageShopGadgetUpgrade.gameObject.GetComponent<UIKeyNavigation>();
					if (component2 != null)
					{
						GUIUtils.ControllerSetSelectedObject(component2.gameObject);
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			yield break;
		}

		private IEnumerator UpdateTimeToStartRound(float timeToStartRoundInSec)
		{
			float startTime = timeToStartRoundInSec;
			while (timeToStartRoundInSec > 0f)
			{
				this.TimeToCloseLabel.text = timeToStartRoundInSec.ToString("00");
				this.TimeToCloseProgressBar.value = timeToStartRoundInSec / startTime;
				timeToStartRoundInSec -= Time.deltaTime;
				yield return null;
			}
			if (this.IsVisible)
			{
				this.HideShop();
			}
			yield break;
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			base.ChangeWindowVisibility(visible);
			if (this.GarageController != null)
			{
				if (visible)
				{
					if (HudGarageShopController.OnShopOpening != null)
					{
						HudGarageShopController.OnShopOpening();
					}
					for (int i = 0; i < this.GadgetObjects.Length; i++)
					{
						this.GadgetObjects[i].GadgetShopOpen();
					}
					this.GarageController.ClientPlayerOpenGadgetShop();
					PlayerController bitComponent = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<PlayerController>();
					bitComponent.ShopInterfaceOpen = true;
					base.StartCoroutine(this.OnOpenWindowAsync());
				}
				else
				{
					this.GarageController.ClientPlayerCloseGadgetShop();
				}
			}
		}

		public void HideShop()
		{
			if (this.HudGarageShopSettings.CloseWindowFmodAsset != null)
			{
				FMODAudioManager.PlayOneShotAt(this.HudGarageShopSettings.CloseWindowFmodAsset, Vector3.zero, 0);
			}
			if (this.audioSnapshotInstance != null)
			{
				this.audioSnapshotInstance.KeyOff();
				this.audioSnapshotInstance = null;
			}
			if (HudGarageShopController.OnShopClosing != null)
			{
				HudGarageShopController.OnShopClosing();
			}
			base.SetWindowVisibility(false);
			for (int i = 0; i < this.GadgetObjects.Length; i++)
			{
				HudGarageShopGadgetObject hudGarageShopGadgetObject = this.GadgetObjects[i];
				hudGarageShopGadgetObject.SetTooltipEnable(false);
				hudGarageShopGadgetObject.GadgetShopClose();
			}
		}

		private void RefreshIfVisible()
		{
			if (!this.IsVisible)
			{
				return;
			}
			this.Refresh();
		}

		private void Refresh()
		{
			this.SetBalance(this._playerStats.Scrap);
		}

		private void OnGadgetUpgraded(GadgetBehaviour gadget, string upgradename, int level)
		{
			if (!gadget.IsUpgradeInShop(upgradename))
			{
				return;
			}
			if (!string.IsNullOrEmpty(upgradename) && level > 0)
			{
				if (HudGarageShopController.OnAnyGadgetUpgraded != null)
				{
					HudGarageShopController.OnAnyGadgetUpgraded();
				}
				if (this.IsVisible)
				{
					CarComponentHub componentHub = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetComponentHub<CarComponentHub>();
					if (gadget.Slot == GadgetSlot.CustomGadget2)
					{
						componentHub.VoiceOverController.Play(VoiceOverEventGroup.BuyUltimateUpgrade);
					}
					else
					{
						componentHub.VoiceOverController.Play(VoiceOverEventGroup.BuyUpgrades);
					}
				}
			}
			this.RefreshIfVisible();
			GadgetSlot slot = gadget.Slot;
			for (int i = 0; i < this.GadgetObjects.Length; i++)
			{
				HudGarageShopGadgetObject hudGarageShopGadgetObject = this.GadgetObjects[i];
				if (slot == hudGarageShopGadgetObject.Slot)
				{
					if (hudGarageShopGadgetObject.GadgetUpgraded(upgradename, level) && this.IsVisible && this.HudGarageShopSettings.GadgetFullAudioFmodAsset != null)
					{
						FMODAudioManager.PlayOneShotAt(this.HudGarageShopSettings.GadgetFullAudioFmodAsset, Vector3.zero, 0);
					}
					break;
				}
			}
		}

		private void SetBalance(int playerBalanceValue)
		{
			this.BalanceLabel.text = playerBalanceValue.ToString(CultureInfo.InvariantCulture);
			int num = -1;
			for (int i = 0; i < this.GadgetObjects.Length; i++)
			{
				HudGarageShopGadgetObject hudGarageShopGadgetObject = this.GadgetObjects[i];
				hudGarageShopGadgetObject.BalanceUpdated(playerBalanceValue);
				int num2;
				if (hudGarageShopGadgetObject.GetMinorAvailableGadgetPrice(out num2) && (num == -1 || num2 < num))
				{
					num = num2;
				}
			}
			if (num == -1 || playerBalanceValue < num)
			{
				this.BalanceAnimationStop();
			}
			else
			{
				this.BalanceAnimationPlay();
			}
		}

		public bool HasEnoughBalanceToBuy()
		{
			if (this._playerStats == null)
			{
				return false;
			}
			int num = -1;
			for (int i = 0; i < this.GadgetObjects.Length; i++)
			{
				HudGarageShopGadgetObject hudGarageShopGadgetObject = this.GadgetObjects[i];
				int num2;
				if (hudGarageShopGadgetObject.GetMinorAvailableGadgetPrice(out num2) && (num == -1 || num2 < num))
				{
					num = num2;
				}
			}
			return num != -1 && this._playerStats.Scrap >= num;
		}

		public void Update()
		{
			this.RoundTimeUpdate();
			if (this._playerStats == null)
			{
				return;
			}
			if (!this.IsVisible)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.MatchOver || GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.PreReplay || GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.Replay)
			{
				this.HideShop();
				return;
			}
			if (this._playerBalanceCache != this._playerStats.Scrap)
			{
				this._playerBalanceCache = this._playerStats.Scrap;
				this.SetBalance(this._playerBalanceCache);
			}
			if (Input.GetKeyDown(KeyCode.Escape) && !GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this.HideShop();
			}
		}

		public void AnimationOnWindowOpen()
		{
			if (HudGarageShopController.OnShopOpened != null)
			{
				HudGarageShopController.OnShopOpened();
			}
		}

		public override void AnimationOnWindowExit()
		{
			base.AnimationOnWindowExit();
			this.BalanceAnimationStop();
			for (int i = 0; i < this.GadgetObjects.Length; i++)
			{
				this.GadgetObjects[i].GadgetShopClose();
			}
			if (HudGarageShopController.OnShopClosed != null)
			{
				HudGarageShopController.OnShopClosed();
			}
			PlayerController bitComponent = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<PlayerController>();
			bitComponent.ShopInterfaceOpen = false;
		}

		private void BalanceAnimationStop()
		{
			if (!this.BalanceAnimation.gameObject.activeSelf)
			{
				return;
			}
			this.BalanceAnimation.Stop();
			this.BalanceAnimation.gameObject.SetActive(false);
		}

		private void BalanceAnimationPlay()
		{
			if (this.BalanceAnimation.gameObject.activeSelf)
			{
				return;
			}
			this.BalanceAnimation.gameObject.SetActive(true);
			this.BalanceAnimation.Play();
		}

		private void BombManagerOnPhaseChange(BombScoreBoard.State bombScoreBoardState)
		{
			if (bombScoreBoardState == BombScoreBoard.State.Shop)
			{
				this.StartRoundTimeCounter();
			}
			else
			{
				this.StopRoundTimeCounter();
			}
		}

		private void StartRoundTimeCounter()
		{
			this._updateStartRoundTime = true;
			this._startRoundTimeInSec = (float)(GameHubBehaviour.Hub.BombManager.ScoreBoard.Timeout - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime()) / 1000f;
			this.TimeToCloseProgressBar.gameObject.SetActive(true);
		}

		private void StopRoundTimeCounter()
		{
			this._updateStartRoundTime = false;
			this.TimeToCloseLabel.text = "--";
			this.TimeToCloseProgressBar.value = 1f;
			this.TimeToCloseProgressBar.gameObject.SetActive(false);
		}

		private void RoundTimeUpdate()
		{
			if (!this._updateStartRoundTime)
			{
				return;
			}
			this._remainingRoundTimeInSec = (float)(GameHubBehaviour.Hub.BombManager.ScoreBoard.Timeout - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime()) / 1000f;
			if (this.IsVisible)
			{
				this.TimeToCloseLabel.text = this._remainingRoundTimeInSec.ToString("00");
				this.TimeToCloseProgressBar.value = 1f - this._remainingRoundTimeInSec / this._startRoundTimeInSec;
			}
		}

		private void OnGamePauseStateChange(PauseController.PauseState oldState, PauseController.PauseState newState, PlayerData playerData)
		{
			if (newState == PauseController.PauseState.Paused || newState == PauseController.PauseState.UnpauseCountDown)
			{
				this.HideShop();
			}
		}

		private void MapKeyNavigationAfterGadgetSetup()
		{
			UIKeyNavigation component = this.GadgetObject0.GadgetSkill.GetComponent<UIKeyNavigation>();
			UIKeyNavigation component2 = this.GadgetObject1.GadgetSkill.GetComponent<UIKeyNavigation>();
			UIKeyNavigation component3 = this.UltimateObject.GadgetSkill.GetComponent<UIKeyNavigation>();
			UIKeyNavigation component4 = this.GenericObject.GadgetSkill.GetComponent<UIKeyNavigation>();
			component.startsSelected = true;
			component2.startsSelected = false;
			component3.startsSelected = false;
			component4.startsSelected = false;
			component.onDown = component2.gameObject;
			component2.onUp = component.gameObject;
			component2.onDown = component3.gameObject;
			component3.onUp = component2.gameObject;
			component3.onDown = component4.gameObject;
			component4.onUp = component3.gameObject;
			if (this.GadgetObject0.GadgetUpgrades.Count > 0)
			{
				HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade = this.GadgetObject0.GadgetUpgrades[0];
				component.onRight = hudGarageShopGadgetUpgrade.gameObject;
				hudGarageShopGadgetUpgrade.GetComponent<UIKeyNavigation>().onLeft = component.gameObject;
			}
			if (this.GadgetObject1.GadgetUpgrades.Count > 0)
			{
				HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade2 = this.GadgetObject1.GadgetUpgrades[0];
				component2.onRight = hudGarageShopGadgetUpgrade2.gameObject;
				hudGarageShopGadgetUpgrade2.GetComponent<UIKeyNavigation>().onLeft = component2.gameObject;
			}
			if (this.UltimateObject.GadgetUpgrades.Count > 0)
			{
				HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade3 = this.UltimateObject.GadgetUpgrades[0];
				component3.onRight = hudGarageShopGadgetUpgrade3.gameObject;
				hudGarageShopGadgetUpgrade3.GetComponent<UIKeyNavigation>().onLeft = component3.gameObject;
			}
			if (this.GenericObject.GadgetUpgrades.Count > 0)
			{
				HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade4 = this.GenericObject.GadgetUpgrades[0];
				component4.onRight = hudGarageShopGadgetUpgrade4.gameObject;
				hudGarageShopGadgetUpgrade4.GetComponent<UIKeyNavigation>().onLeft = component4.gameObject;
			}
			this.MapKeyNavigationLeftRightList(this.GadgetObject0.GadgetUpgrades);
			this.MapKeyNavigationLeftRightList(this.GadgetObject1.GadgetUpgrades);
			this.MapKeyNavigationLeftRightList(this.GenericObject.GadgetUpgrades);
			bool flag = this.UltimateObject.GadgetUpgrades.Count > 0;
			HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade5 = null;
			if (flag)
			{
				hudGarageShopGadgetUpgrade5 = this.UltimateObject.GadgetUpgrades[0];
			}
			for (int i = 0; i < this.GadgetObject0.GadgetUpgrades.Count; i++)
			{
				HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade6 = this.GadgetObject0.GadgetUpgrades[i];
				UIKeyNavigation component5 = hudGarageShopGadgetUpgrade6.GetComponent<UIKeyNavigation>();
				if (i < this.GadgetObject1.GadgetUpgrades.Count)
				{
					HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade7 = this.GadgetObject1.GadgetUpgrades[i];
					component5.onDown = hudGarageShopGadgetUpgrade7.gameObject;
					hudGarageShopGadgetUpgrade7.GetComponent<UIKeyNavigation>().onUp = hudGarageShopGadgetUpgrade6.gameObject;
					if (flag)
					{
						hudGarageShopGadgetUpgrade7.GetComponent<UIKeyNavigation>().onDown = hudGarageShopGadgetUpgrade5.gameObject;
						if (i == 0)
						{
							hudGarageShopGadgetUpgrade5.GetComponent<UIKeyNavigation>().onUp = hudGarageShopGadgetUpgrade7.gameObject;
						}
					}
				}
			}
			if (flag)
			{
				for (int j = 0; j < this.GenericObject.GadgetUpgrades.Count; j++)
				{
					HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade8 = this.GenericObject.GadgetUpgrades[j];
					hudGarageShopGadgetUpgrade8.GetComponent<UIKeyNavigation>().onUp = hudGarageShopGadgetUpgrade5.gameObject;
					if (j == 0)
					{
						hudGarageShopGadgetUpgrade5.GetComponent<UIKeyNavigation>().onDown = hudGarageShopGadgetUpgrade8.gameObject;
					}
				}
			}
		}

		private void MapKeyNavigationLeftRightList(List<HudGarageShopGadgetUpgrade> upgrades)
		{
			for (int i = upgrades.Count - 1; i > 0; i--)
			{
				HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade = upgrades[i];
				UIKeyNavigation component = hudGarageShopGadgetUpgrade.GetComponent<UIKeyNavigation>();
				HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade2 = upgrades[i - 1];
				UIKeyNavigation component2 = hudGarageShopGadgetUpgrade2.GetComponent<UIKeyNavigation>();
				component.onLeft = hudGarageShopGadgetUpgrade2.gameObject;
				component2.onRight = hudGarageShopGadgetUpgrade.gameObject;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudGarageShopController));

		public HudGarageShopSettings HudGarageShopSettings;

		public static Action OnShopOpening;

		public static Action OnShopOpened;

		public static Action OnShopClosing;

		public static Action OnShopClosed;

		public static Action OnAnyGadgetUpgraded;

		public HudGarageShopGadgetObject GadgetObject0;

		public HudGarageShopGadgetObject GadgetObject1;

		public HudGarageShopGadgetObject UltimateObject;

		public HudGarageShopGadgetObject GenericObject;

		public UILabel BalanceLabel;

		public UILabel TimeToCloseLabel;

		public UIProgressBar TimeToCloseProgressBar;

		public Animation BalanceAnimation;

		[HideInInspector]
		public FMODAudioManager.FMODAudio audioSnapshotInstance;

		[HideInInspector]
		public GarageController GarageController;

		private CombatObject _combatObject;

		private PlayerStats _playerStats;

		private int _playerBalanceCache;

		private bool _updateStartRoundTime;

		private float _startRoundTimeInSec;

		private float _remainingRoundTimeInSec;

		private HudGarageShopGadgetObject[] _gadgetObjects;

		private GameGui _gameGui;
	}
}
