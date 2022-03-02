using System;
using System.Collections;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Specializations;
using ClientAPI;
using HeavyMetalMachines.CharacterHelp.Presenting;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Store;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Store.Business.GetStoreItem;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.VFX;
using Hoplon.Input.UiNavigation;
using Hoplon.Serialization;
using Hoplon.Unity.Loading;
using ModelViewer;
using Pocketverse;
using SharedUtils.Loading;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class ShopDetails : GameHubBehaviour
	{
		private IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void OnEnable()
		{
			this._driverConceptCache = this.DriverConcept.sprite2D;
			this._characterNameTextureCache = this.CharacterNameTexture.sprite2D;
			this._modelViewerTexture.gameObject.SetActive(false);
			UnityAnimation fadeOutAnimation = new UnityAnimation(this._mainAnimation, "3dModelOut");
			UnityAnimation fadeInAnimation = new UnityAnimation(this._mainAnimation, "3dModelin");
			DynamicAssetLoader assetLoader = new DynamicAssetLoader(Loading.PrefabManager);
			this._animatedAssetPresenter = new AnimatedAssetPresenter(fadeInAnimation, fadeOutAnimation, assetLoader, this.ModelViewer);
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.StoreItemDeactivated += this.OnStoreItemDeactivated;
		}

		private void OnDisable()
		{
			this.ModelViewer.ModelName = null;
			this._modelViewerTexture.gameObject.SetActive(false);
			this.DriverConcept.sprite2D = this._driverConceptCache;
			this.CharacterNameTexture.sprite2D = this._characterNameTextureCache;
			this._animatedAssetPresenter.Dispose();
			this._animatedAssetPresenter = null;
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.StoreItemDeactivated -= this.OnStoreItemDeactivated;
		}

		private void Update()
		{
			this._animatedAssetPresenter.Update();
		}

		private void LateUpdate()
		{
			if (this.CurrentState != ShopDetails.State.Skin)
			{
				return;
			}
			if (this._shouldCycleToCurrentSkin)
			{
				this._targetSkinCardsChosenSkinIndex = this.currentSkinCardsCenteredIndex;
				this._shouldCycleToCurrentSkin = false;
				this.Cicle(0);
			}
		}

		private void Show()
		{
			this.rootGameObject.SetActive(true);
			this.DetailsBackButton.CacheDefaultColor();
			this.currentNavigationContext = ShopDetails.NavigationContext.buying;
			this.BackButton.SetActive(true);
			this._characterHelpPresenter.EnableShortcut();
			this.UiNavigationGroupHolder.AddGroup();
		}

		public void Hide()
		{
			this.CurrentState = ShopDetails.State.None;
			this.InternalHide(false);
		}

		public void SwitchToDriverDetails()
		{
			if (this.CurrentState == ShopDetails.State.Driver)
			{
				return;
			}
			this.currentNavigationContext = ShopDetails.NavigationContext.buying;
			this.CurrentState = ShopDetails.State.Driver;
			this.ConfigureForDriver(this._currentCharItemType);
			this.skinDetailsGameObject.SetActive(false);
			this.driverDetailGameObject.SetActive(true);
		}

		public void SwitchToSkinDetails()
		{
			if (this.CurrentState == ShopDetails.State.Skin)
			{
				return;
			}
			this.currentNavigationContext = ShopDetails.NavigationContext.buying;
			this.CurrentState = ShopDetails.State.Skin;
			this.ConfigureForSkin();
			if (this.currentSkinCardsCenteredIndex == this.defaultSkinCardsCenteredIndex)
			{
				this._targetSkinCardsChosenSkinIndex = 0;
				this.defaultSkinCardsCenteredIndex = this._targetSkinCardsChosenSkinIndex;
			}
			this.currentSkinCardsCenteredIndex = this._targetSkinCardsChosenSkinIndex;
			ItemTypeScriptableObject storeItemType = this._activeSkinCards[this.currentSkinCardsCenteredIndex].StoreItemType;
			this.ChangeSkin(storeItemType);
			this.skinDetailsGameObject.SetActive(true);
			this.driverDetailGameObject.SetActive(false);
		}

		public bool IsVisible()
		{
			return this.rootGameObject.activeInHierarchy;
		}

		private void InternalHide(bool goToShopCash)
		{
			this.DriverToggle.value = false;
			this.SkinToggle.value = false;
			this.DisableTooltipColliders();
			this.HideTooltip();
			this._targetSkinCardsChosenSkinIndex = 0;
			this.currentSkinCardsCenteredIndex = 0;
			this.defaultSkinCardsCenteredIndex = 0;
			this.rootGameObject.SetActive(false);
			this.skinDetailsGameObject.SetActive(false);
			this.driverDetailGameObject.SetActive(false);
			this._itemTypeShopDetails.Hide();
			if (goToShopCash)
			{
				this.Shop.AnimateReturnToCash();
			}
			else
			{
				this.Shop.AnimateReturn();
			}
			this._characterHelpPresenter.DisableShortcut();
			this.UiNavigationGroupHolder.RemoveGroup();
		}

		private void OnStoreItemDeactivated()
		{
			if (!this.IsVisible())
			{
				return;
			}
			if (this.CurrentState == ShopDetails.State.Skin)
			{
				return;
			}
			this.Hide();
		}

		private void ConfigureForDriver(IItemType charItemType)
		{
			this._currentCharItemType = charItemType;
			this.PopulateCharacterOrderedCustomization(charItemType.Id);
			this.CurrentSkinItemType = charItemType;
			this._characterHelpPresenter.Set(this._currentCharItemType.Id);
			this.skinDetailsGameObject.SetActive(false);
			this.driverDetailGameObject.SetActive(true);
			CharacterItemTypeComponent component = charItemType.GetComponent<CharacterItemTypeComponent>();
			this.CharacterDescription.text = Language.Get(component.DescriptionDraft, TranslationContext.CharactersBaseInfo);
			Guid id = charItemType.Id;
			bool flag = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id);
			if (flag)
			{
				this.BoughtGroup.SetActive(true);
				this.BuyGroup.SetActive(false);
			}
			else
			{
				this.BoughtGroup.SetActive(false);
				this.BuyGroup.SetActive(true);
			}
			this.CharacterNameLabel.text = string.Empty;
			this.OpenGuideButton.SetActive(false);
			CharacterItemTypeComponent component2 = charItemType.GetComponent<CharacterItemTypeComponent>();
			if (component2)
			{
				this.CharacterNameLabel.text = component2.GetCharacterLocalizedName();
				this.UpdateRole(component2.Role, this.SuportRole, this.CarrierRole, this.TacklerRole);
				this.Setdifficulty(component2);
				this.Skill01.SpriteName = component2.AssetPrefix + "_Gadget01";
				this.Skill02.SpriteName = component2.AssetPrefix + "_Gadget02";
				this.Skill03.SpriteName = component2.AssetPrefix + "_Gadget03";
				this.SkillNitro.SpriteName = component2.AssetPrefix + "_GadgetNitro";
				this.SkillPassive.transform.parent.gameObject.SetActive(component2.HasPassive);
				if (component2.HasPassive)
				{
					this.SkillPassive.SpriteName = component2.AssetPrefix + "_GadgetPassive";
				}
				this.SkillsGrid.repositionNow = true;
				this.ConfigureCarPreview(this.CurrentSkinItemType);
				this.OpenGuideButton.SetActive(false);
			}
			IGetStoreItem getStoreItem = this._storeBusinessFactory.CreateGetStoreItem();
			StoreItem storeItem = getStoreItem.Get(id);
			this.ConfigureDriverPrice(storeItem);
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
			}
			this._storeItemObservation = ObservableExtensions.Subscribe<StoreItem>(this._storeBusinessFactory.CreateObserveStoreItem().CreateObservable(id), delegate(StoreItem storeItemPrices)
			{
				this.ConfigureDriverPrice(storeItemPrices);
			});
			this._topTabsGroup.SetActive(true);
			this.EnableTooltipColliders();
		}

		private void EnableTooltipColliders()
		{
			this.Skill01.GetComponent<BoxCollider>().enabled = true;
			this.Skill02.GetComponent<BoxCollider>().enabled = true;
			this.Skill03.GetComponent<BoxCollider>().enabled = true;
			this.SkillNitro.GetComponent<BoxCollider>().enabled = true;
			this.SkillPassive.GetComponent<BoxCollider>().enabled = true;
			this.SuportRole.GetComponent<BoxCollider>().enabled = true;
			this.CarrierRole.GetComponent<BoxCollider>().enabled = true;
			this.TacklerRole.GetComponent<BoxCollider>().enabled = true;
			this.SkinSuportRole.GetComponent<BoxCollider>().enabled = true;
			this.SkinCarrierRole.GetComponent<BoxCollider>().enabled = true;
			this.SkinTacklerRole.GetComponent<BoxCollider>().enabled = true;
		}

		private void DisableTooltipColliders()
		{
			this.Skill01.GetComponent<BoxCollider>().enabled = false;
			this.Skill02.GetComponent<BoxCollider>().enabled = false;
			this.Skill03.GetComponent<BoxCollider>().enabled = false;
			this.SkillNitro.GetComponent<BoxCollider>().enabled = false;
			this.SkillPassive.GetComponent<BoxCollider>().enabled = false;
			this.SuportRole.GetComponent<BoxCollider>().enabled = false;
			this.CarrierRole.GetComponent<BoxCollider>().enabled = false;
			this.TacklerRole.GetComponent<BoxCollider>().enabled = false;
			this.SkinSuportRole.GetComponent<BoxCollider>().enabled = false;
			this.SkinCarrierRole.GetComponent<BoxCollider>().enabled = false;
			this.SkinTacklerRole.GetComponent<BoxCollider>().enabled = false;
		}

		private void ConfigureDriverPrice(StoreItem storeItem)
		{
			this.DriverSoftPrice.text = string.Format("{0}", storeItem.SoftPrice);
			this.DriverHardPrice.text = string.Format("{0}", storeItem.HardPrice);
			if (storeItem.IsHardPurchasable && storeItem.HardPrice > this._localBalanceStorage.HardCurrency)
			{
				this.DriverHardPrice.color = Color.grey;
				this.DriverHardIconPrice.color = Color.grey;
			}
			else
			{
				this.DriverHardPrice.color = this.Shop.CashColor;
				this.DriverHardIconPrice.color = Color.white;
			}
			if (storeItem.IsSoftPurchasable && storeItem.SoftPrice > (long)this._localBalanceStorage.SoftCurrency)
			{
				this.DriverSoftPrice.color = Color.grey;
				this.DriverSoftIconPrice.color = Color.grey;
			}
			else
			{
				this.DriverSoftPrice.color = this.Shop.FameColor;
				this.DriverSoftIconPrice.color = Color.white;
			}
		}

		private void UpdateRole(DriverRoleKind role, GameObject supportRole, GameObject carrierRole, GameObject tacklerRole)
		{
			carrierRole.SetActive(false);
			supportRole.SetActive(false);
			tacklerRole.SetActive(false);
			switch (role)
			{
			case 0:
			case 6:
				supportRole.SetActive(true);
				break;
			case 1:
			case 3:
			case 5:
				carrierRole.SetActive(true);
				break;
			case 2:
			case 4:
				tacklerRole.SetActive(true);
				break;
			}
		}

		public void ShowDriverDetails(IItemType charHierarchy)
		{
			this.CurrentState = ShopDetails.State.Driver;
			base.StartCoroutine(this.WaitAndToggle(this.DriverToggle, true));
			this.ConfigureForDriver(charHierarchy);
			this.Show();
		}

		public void BuyCharacter()
		{
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.ShowBuyWindow(this._currentCharItemType, new Action(this.OnCompleteCharacterBuy), new Action(this.OnBuyWindowClosed), new Action(this.OnGoToShopCash), 1, false);
		}

		private void OnGoToShopCash()
		{
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.CloseWindow();
			this.InternalHide(true);
		}

		private void OnCompleteCharacterBuy()
		{
			this.ConfigureForDriver(this._currentCharItemType);
		}

		private void OnBuyWindowClosed()
		{
			Guid id = this._currentCharItemType.Id;
			if (GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id))
			{
				this.BoughtGroup.SetActive(true);
				this.BuyGroup.SetActive(false);
			}
			else
			{
				this.BoughtGroup.SetActive(false);
				this.BuyGroup.SetActive(true);
			}
		}

		private void ConfigureForSkin(IItemType storeItem)
		{
			ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[storeItem.Id];
			ShopItemTypeComponent component = itemTypeScriptableObject.GetComponent<ShopItemTypeComponent>();
			SkinPrefabItemTypeComponent component2 = itemTypeScriptableObject.GetComponent<SkinPrefabItemTypeComponent>();
			SkinItemTypeBag skinItemTypeBag = (SkinItemTypeBag)((JsonSerializeable<!0>)itemTypeScriptableObject.Bag);
			ItemTypeScriptableObject characterItemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[skinItemTypeBag.CharacterItemTypeId];
			this.ConfigureForSkin(itemTypeScriptableObject, characterItemTypeScriptableObject);
		}

		private void ConfigureForSkin(ItemTypeScriptableObject skinItemTypeScriptableObject, ItemTypeScriptableObject characterItemTypeScriptableObject)
		{
			this.CurrentSkinItemType = skinItemTypeScriptableObject;
			this._currentCharItemType = characterItemTypeScriptableObject;
			this.PopulateCharacterOrderedCustomization(this._currentCharItemType.Id);
			this.currentSkinCardsCenteredIndex = 0;
			this.ConfigureForSkin();
			this.currentSkinCardsCenteredIndex = this.GetSkinCardIndexByGuid(skinItemTypeScriptableObject.Id);
			ItemTypeScriptableObject storeItemType = this._activeSkinCards[this.currentSkinCardsCenteredIndex].StoreItemType;
			this.ChangeSkin(storeItemType);
		}

		private void PopulateCharacterOrderedCustomization(Guid characterGuid)
		{
			this._currentCharacterOrderedCustomizations = new List<ItemTypeScriptableObject>();
			List<Guid> list = GameHubBehaviour.Hub.InventoryColletion.CharacterToSkinGuids[characterGuid];
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[list[i]];
				if (itemTypeScriptableObject.ContainsComponent<ShopItemTypeComponent>())
				{
					this._currentCharacterOrderedCustomizations.Add(itemTypeScriptableObject);
				}
			}
			this._currentCharacterOrderedCustomizations.Sort(new Comparison<ItemTypeScriptableObject>(this.SkinItemTypeSortComparer));
		}

		private void ConfigureForSkin()
		{
			this._characterHelpPresenter.Set(this._currentCharItemType.Id);
			this.skinDetailsGameObject.SetActive(true);
			this.driverDetailGameObject.SetActive(false);
			CharacterItemTypeComponent component = this._currentCharItemType.GetComponent<CharacterItemTypeComponent>();
			if (component)
			{
				this.CharacterNameLabel.text = component.GetCharacterLocalizedName();
				this.UpdateRole(component.Role, this.SkinSuportRole, this.SkinCarrierRole, this.SkinTacklerRole);
			}
			int num = 0;
			Guid id = this._currentCharItemType.Id;
			List<Guid> list = GameHubBehaviour.Hub.InventoryColletion.CharacterToSkinGuids[id];
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[list[i]];
				if (itemTypeScriptableObject.IsItemEnableInShop())
				{
					num++;
				}
			}
			this.ConfigureSkinsCardsPool(num);
			this.ConfigureCardsForTier();
			Array.Sort<StoreItem>(this.SkinCards, new Comparison<StoreItem>(this.SkinCardsSortComparer));
			this._activeSkinCards.Sort(new Comparison<StoreItem>(this.SkinCardsSortComparer));
			if (this.currentSkinCardsCenteredIndex >= this._activeSkinCards.Count)
			{
				this.currentSkinCardsCenteredIndex = 0;
			}
			if (this.currentSkinCardsCenteredIndex < this._activeSkinCards.Count)
			{
				ItemTypeScriptableObject storeItemType = this._activeSkinCards[this.currentSkinCardsCenteredIndex].StoreItemType;
				this.ChangeSkin(storeItemType);
			}
			this._topTabsGroup.SetActive(true);
			this.EnableTooltipColliders();
		}

		private void ConfigureSkinsCardsPool(int amountofavailableskins)
		{
			if (this.SkinCards.Length < amountofavailableskins)
			{
				Array.Resize<StoreItem>(ref this.SkinCards, amountofavailableskins);
			}
			else if (this.SkinCards.Length > amountofavailableskins)
			{
				for (int i = amountofavailableskins; i < this.SkinCards.Length; i++)
				{
					this.SkinCards[i].gameObject.SetActive(false);
				}
			}
		}

		private int SkinCardsSortComparer(StoreItem x, StoreItem y)
		{
			return string.CompareOrdinal(x.gameObject.name, y.gameObject.name);
		}

		private int SkinItemTypeSortComparer(ItemTypeScriptableObject x, ItemTypeScriptableObject y)
		{
			return string.CompareOrdinal(x.Name, y.Name);
		}

		private void ConfigureSkinBuyGroup(bool hasPilotAlready, bool isBasicSkin, bool itemBought, bool hasLevelToBuy, int UnlockLevel)
		{
			if (hasPilotAlready)
			{
				if (isBasicSkin)
				{
					this.SkinLockedGroup.SetActive(false);
					this.SkinBoughtGroup.SetActive(true);
					this.SkinBuyGroup.SetActive(false);
					this._skinFeedbackDontHavePilot.SetActive(false);
				}
				else if (itemBought)
				{
					this.SkinLockedGroup.SetActive(false);
					this.SkinBoughtGroup.SetActive(true);
					this._skinFeedbackDontHavePilot.SetActive(false);
					this.SkinBuyGroup.SetActive(false);
				}
				else
				{
					int unlockLevelForSkin = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetUnlockLevelForSkin(UnlockLevel);
					this.SkinUnlockLevelLabel.text = Language.GetFormatted("SKINDETAIL_ITEMUNLOCK_LEVEL_LABEL", TranslationContext.Store, new object[]
					{
						1 + unlockLevelForSkin
					});
					this.SkinLockedGroup.SetActive(!hasLevelToBuy);
					this.SkinBoughtGroup.SetActive(false);
					this.SkinBuyGroup.SetActive(true);
					this._skinFeedbackDontHavePilot.SetActive(false);
					this._skinBuyButton.isEnabled = hasLevelToBuy;
				}
			}
			else
			{
				this.SkinLockedGroup.SetActive(false);
				this.SkinBoughtGroup.SetActive(false);
				this._skinFeedbackDontHavePilot.SetActive(!isBasicSkin);
				this.SkinBuyGroup.SetActive(true);
				this._skinBuyButton.isEnabled = isBasicSkin;
			}
		}

		private void ConfigureCardsForTier()
		{
			this._activeSkinCards.Clear();
			int num = 0;
			List<Guid> list = GameHubBehaviour.Hub.InventoryColletion.CharacterToSkinGuids[this._currentCharItemType.Id];
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[list[i]];
				if (itemTypeScriptableObject.IsItemEnableInShop())
				{
					StoreItem storeItem = (!(this.SkinCards[num] == null)) ? this.SkinCards[num] : Object.Instantiate<StoreItem>(this.SkinCardPrefab);
					storeItem.gameObject.transform.parent = this.skinCardsPivot;
					storeItem.transform.localPosition = Vector3.one;
					storeItem.transform.localScale = Vector3.one;
					storeItem.carTexture.SpriteName = itemTypeScriptableObject.Name;
					storeItem.IsPurchasableChanged -= this.OnStoreItemIsPurchasableChanged;
					storeItem.Setup(itemTypeScriptableObject, this._storeBusinessFactory);
					storeItem.IsPurchasableChanged += this.OnStoreItemIsPurchasableChanged;
					if (storeItem.IsPurchasable)
					{
						this._activeSkinCards.Add(storeItem);
					}
					storeItem.gameObject.name = itemTypeScriptableObject.Name;
					SkinPrefabItemTypeComponent component = itemTypeScriptableObject.GetComponent<SkinPrefabItemTypeComponent>();
					storeItem.characterName.text = Language.Get(component.CardSkinDraft, TranslationContext.Items);
					storeItem.gameObject.SetActive(storeItem.IsPurchasable);
					this.SkinCards[num] = storeItem;
					num++;
				}
			}
		}

		private void OnStoreItemIsPurchasableChanged(StoreItem storeItem, bool isPurchasable)
		{
			storeItem.gameObject.SetActive(isPurchasable);
			if (isPurchasable)
			{
				this._activeSkinCards.Add(storeItem);
				this._activeSkinCards.Sort(new Comparison<StoreItem>(this.SkinCardsSortComparer));
				this._shouldCycleToCurrentSkin = true;
			}
			else
			{
				this._activeSkinCards.Remove(storeItem);
				this._shouldCycleToCurrentSkin = true;
			}
		}

		public void ShowSkinDetails(IItemType storeItem)
		{
			this.CurrentState = ShopDetails.State.Skin;
			base.StartCoroutine(this.WaitAndToggle(this.SkinToggle, true));
			this.ConfigureForSkin(storeItem);
			this.Show();
			this.MoveToChosenStoreItemSkin(storeItem);
		}

		public bool TryToShowSkinDetails(Guid itemTypeId)
		{
			Guid skinItemTypeCharacterId = GameHubBehaviour.Hub.InventoryColletion.GetSkinItemTypeCharacterId(itemTypeId);
			if (skinItemTypeCharacterId == Guid.Empty)
			{
				return false;
			}
			ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[itemTypeId];
			ItemTypeScriptableObject itemTypeScriptableObject2 = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[skinItemTypeCharacterId];
			CharacterItemTypeBag characterItemTypeBag = (CharacterItemTypeBag)((JsonSerializeable<!0>)itemTypeScriptableObject2.Bag);
			if (itemTypeId == characterItemTypeBag.DefaultSkinGuid)
			{
				this.ShowDriverDetails(itemTypeScriptableObject2);
				return true;
			}
			if (!itemTypeScriptableObject.IsItemEnableInShop())
			{
				return false;
			}
			this.CurrentState = ShopDetails.State.Skin;
			base.StartCoroutine(this.WaitAndToggle(this.SkinToggle, true));
			this.ConfigureForSkin(itemTypeScriptableObject, itemTypeScriptableObject2);
			this.Show();
			this.MoveToChosenStoreItemSkin(itemTypeScriptableObject);
			return true;
		}

		public void CicleLeft()
		{
			int targetSkinCardsChosenSkinIndex;
			if (this.currentSkinCardsCenteredIndex <= 0)
			{
				targetSkinCardsChosenSkinIndex = this._activeSkinCards.Count - 1;
			}
			else
			{
				targetSkinCardsChosenSkinIndex = this.currentSkinCardsCenteredIndex - 1;
			}
			this._targetSkinCardsChosenSkinIndex = targetSkinCardsChosenSkinIndex;
			this.Cicle(-1);
		}

		public void CicleRight()
		{
			int targetSkinCardsChosenSkinIndex;
			if (this.currentSkinCardsCenteredIndex >= this._activeSkinCards.Count - 1)
			{
				targetSkinCardsChosenSkinIndex = 0;
			}
			else
			{
				targetSkinCardsChosenSkinIndex = this.currentSkinCardsCenteredIndex + 1;
			}
			this._targetSkinCardsChosenSkinIndex = targetSkinCardsChosenSkinIndex;
			this.Cicle(1);
		}

		public void MoveToChosenStoreItemSkin(IItemType itemTypeScriptableObject)
		{
			this._targetSkinCardsChosenSkinIndex = this.GetSkinCardIndexByGuid(itemTypeScriptableObject.Id);
			if (this._targetSkinCardsChosenSkinIndex == this.currentSkinCardsCenteredIndex)
			{
				return;
			}
			int num = this.currentSkinCardsCenteredIndex - this._targetSkinCardsChosenSkinIndex;
			if (num < 0)
			{
				num = this._activeSkinCards.Count - Mathf.Abs(num);
			}
			this._wayToCicle = -1;
			if (num > this._activeSkinCards.Count / 2)
			{
				this._wayToCicle = 1;
			}
			this.CicleToChoosenSkin();
		}

		private void CicleToChoosenSkin()
		{
			if (this._targetSkinCardsChosenSkinIndex == this.currentSkinCardsCenteredIndex)
			{
				UIButton[] components = this._activeSkinCards[this._targetSkinCardsChosenSkinIndex].GetComponents<UIButton>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].SetState(UIButtonColor.State.Normal, true);
				}
				return;
			}
			this.Cicle(this._wayToCicle);
		}

		public void Cicle(int how)
		{
			int num;
			if (this.currentSkinCardsCenteredIndex + how > this._activeSkinCards.Count - 1)
			{
				num = 0;
			}
			else if (this.currentSkinCardsCenteredIndex + how < 0)
			{
				num = this._activeSkinCards.Count - 1;
			}
			else
			{
				num = this.currentSkinCardsCenteredIndex + how;
			}
			this.currentSkinCardsCenteredIndex = num;
			if (this.currentSkinCardsCenteredIndex < this._activeSkinCards.Count)
			{
				ItemTypeScriptableObject storeItemType = this._activeSkinCards[this.currentSkinCardsCenteredIndex].StoreItemType;
				this.ChangeSkin(storeItemType);
			}
		}

		private void ChangeSkin(ItemTypeScriptableObject skinItemType)
		{
			this.CurrentSkinItemType = skinItemType;
			Guid id = this._currentCharItemType.Id;
			bool hasPilotAlready = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id);
			this.ConfigureSkinDescriptions(skinItemType);
			if (this._activeSkinCards.Count > 0)
			{
				float num = 1f / (float)this._activeSkinCards.Count;
				int num2 = Mathf.Abs(this._targetSkinCardsChosenSkinIndex - this.currentSkinCardsCenteredIndex) + 1;
				float num3 = 1f / (float)Mathf.Max(num2, 1);
				float duration = this.SkinsRotationCurveSpeed.Evaluate(num3);
				for (int i = 0; i < this._activeSkinCards.Count; i++)
				{
					NGUITools.MarkParentAsChanged(this._activeSkinCards[i].gameObject);
					TweenPosition component = this._activeSkinCards[i].GetComponent<TweenPosition>();
					component.SetStartToCurrentValue();
					float num4 = num * (float)(i - this.currentSkinCardsCenteredIndex) - this.Tunning;
					float num5 = num4 * 3.1415927f * 2f;
					float num6 = Mathf.Sin(num5);
					float num7 = Mathf.Cos(num5);
					component.to = new Vector3(num7 * this.maxX, (num6 + 1f) * this.maxY, 0f);
					component.duration = duration;
					int num8 = (num7 < 0f) ? 2 : 1;
					int num9 = Mathf.Abs(this.currentSkinCardsCenteredIndex - i);
					int depth;
					if (num9 != 0)
					{
						depth = this.baseDepth - 10 - Mathf.FloorToInt(num6 * 10f) - num8;
					}
					else
					{
						depth = this.baseDepth;
					}
					this._activeSkinCards[i].GetComponent<UIPanel>().depth = depth;
					if (component.onFinished != null)
					{
						component.onFinished.Clear();
					}
					component.ResetToBeginning();
					component.PlayForward();
				}
				this.ConfigureCenteredSkin(skinItemType, hasPilotAlready);
				TweenPosition component2 = this._activeSkinCards[0].GetComponent<TweenPosition>();
				EventDelegate.Add(component2.onFinished, new EventDelegate.Callback(this.CicleToChoosenSkin), true);
			}
			this.ConfigureCarPreview(this.CurrentSkinItemType);
		}

		private void ConfigureCenteredSkin(ItemTypeScriptableObject skinItemType, bool hasPilotAlready)
		{
			SkinItemTypeBag skinItemTypeBag = (SkinItemTypeBag)((JsonSerializeable<!0>)skinItemType.Bag);
			CharacterBag characterBag = GameHubBehaviour.Hub.User.GetCharacterBag(this._currentCharItemType.Id);
			bool hasLevelToBuy;
			if (characterBag != null)
			{
				PlayerBag playerBag = (PlayerBag)GameHubBehaviour.Hub.User.PlayerSF.Bag;
				int levelForXP = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetLevelForXP(characterBag.Xp);
				ProgressionInfo.CanBuyReason canBuyReason;
				string text;
				hasLevelToBuy = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.CanBuySkin(hasPilotAlready, playerBag, skinItemTypeBag, levelForXP, (CharacterItemTypeBag)((JsonSerializeable<!0>)this._currentCharItemType.Bag), ref canBuyReason, ref text);
			}
			else
			{
				hasLevelToBuy = (skinItemTypeBag.UnlockLevel == 0);
			}
			Guid id = skinItemType.Id;
			bool itemBought = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id);
			CharacterItemTypeBag characterItemTypeBag = (CharacterItemTypeBag)((JsonSerializeable<!0>)this._currentCharItemType.Bag);
			bool isBasicSkin = skinItemType.Id == characterItemTypeBag.DefaultSkinGuid;
			this.ConfigureSkinBuyGroup(hasPilotAlready, isBasicSkin, itemBought, hasLevelToBuy, skinItemTypeBag.UnlockLevel);
			IGetStoreItem getStoreItem = this._storeBusinessFactory.CreateGetStoreItem();
			StoreItem storeItem = getStoreItem.Get(id);
			this.ConfigureSkinPrice(storeItem);
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
			}
			this._storeItemObservation = ObservableExtensions.Subscribe<StoreItem>(this._storeBusinessFactory.CreateObserveStoreItem().CreateObservable(id), new Action<StoreItem>(this.ConfigureSkinPrice));
		}

		private void ConfigureSkinPrice(StoreItem storeItem)
		{
			bool hasSoftFunds = (long)this._localBalanceStorage.SoftCurrency >= storeItem.SoftPrice;
			bool hasHardFunds = this._localBalanceStorage.HardCurrency >= storeItem.HardPrice;
			bool isSoftPurchasable = storeItem.IsSoftPurchasable;
			bool isHardPurchasable = storeItem.IsHardPurchasable;
			this.ConfigureSkinCurrencyGroup((int)storeItem.SoftPrice, (int)storeItem.HardPrice, isSoftPurchasable, isHardPurchasable, hasSoftFunds, hasHardFunds);
		}

		private void ConfigureSkinDescriptions(ItemTypeScriptableObject skinitemtype)
		{
			SkinPrefabItemTypeComponent component = skinitemtype.GetComponent<SkinPrefabItemTypeComponent>();
			ShopDetailsComponent component2 = skinitemtype.GetComponent<ShopDetailsComponent>();
			CharacterItemTypeComponent component3 = this._currentCharItemType.GetComponent<CharacterItemTypeComponent>();
			this.DriverName.text = component3.GetCharacterLocalizedName();
			this.SkinName.text = Language.Get(component.CardSkinDraft, TranslationContext.Items);
			this.SkinDescription.text = Language.Get(component2.SkinQuoteTextDraf, TranslationContext.Items);
			this.SkinQuote.text = Language.Get(component2.SkinQuoterAuthorDraft, TranslationContext.Items);
		}

		private void ConfigureSkinCurrencyGroup(int softPrice, int hardPrice, bool isSoftPurchasable, bool isHardPurchasable, bool hasSoftFunds, bool hasHardFunds)
		{
			if (isSoftPurchasable)
			{
				this.softPriceLabel.text = string.Format("{0}", softPrice);
				if (!hasSoftFunds)
				{
					this.softPriceLabel.color = this.Shop.NoFundsColor;
					this.softPriceIcon.color = this.Shop.NoFundsColor;
				}
				else
				{
					this.softPriceLabel.color = this.Shop.FameColor;
					this.softPriceIcon.color = Color.white;
				}
			}
			else
			{
				this.softPriceLabel.text = "-";
			}
			if (isHardPurchasable)
			{
				this.hardPriceLabel.text = string.Format("{0}", hardPrice);
				if (!hasHardFunds)
				{
					this.hardPriceLabel.color = this.Shop.NoFundsColor;
					this.hardPriceIcon.color = this.Shop.NoFundsColor;
				}
				else
				{
					this.hardPriceLabel.color = this.Shop.CashColor;
					this.hardPriceIcon.color = Color.white;
				}
			}
			else
			{
				this.hardPriceLabel.text = "-";
			}
		}

		private int GetSkinCardIndexByGuid(Guid pGuid)
		{
			for (int i = 0; i < this._activeSkinCards.Count; i++)
			{
				StoreItem storeItem = this._activeSkinCards[i];
				if (storeItem.StoreItemType.Id == pGuid)
				{
					return i;
				}
			}
			Debug.LogError(string.Concat(new object[]
			{
				"skin, guid == ",
				pGuid,
				" not found on CurrentCharacterHierarchy.SkinItems of character",
				this._currentCharItemType.Name
			}), this);
			return 0;
		}

		public void BuySkin()
		{
			this._targetSkinCardsChosenSkinIndex = this.currentSkinCardsCenteredIndex;
			CharacterItemTypeBag characterItemTypeBag = (CharacterItemTypeBag)((JsonSerializeable<!0>)this._currentCharItemType.Bag);
			if (this.CurrentSkinItemType.Id != characterItemTypeBag.DefaultSkinGuid)
			{
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.ShowBuyWindow(this.CurrentSkinItemType, new Action(this.OnCompleteSkinBuy), new Action(this.OnBuyWindowClosed), new Action(this.OnGoToShopCash), 1, false);
			}
		}

		private void OnCompleteSkinBuy()
		{
			this.UpdateCharacterUnlock(this.CurrentSkinItemType);
		}

		private void CompleteSkinBuy()
		{
			this.currentSkinCardsCenteredIndex = this.GetSkinCardIndexByGuid(this.CurrentSkinItemType.Id);
			this.ConfigureForSkin();
		}

		private void UpdateCharacterUnlock(IItemType customizationItem)
		{
			CharacterBag characterBag = GameHubBehaviour.Hub.User.GetCharacterBag(this._currentCharItemType.Id);
			if (characterBag == null)
			{
				ShopDetails.Log.DebugFormat(string.Format("(UpdateCharacterUnlock) Character bag not found. ItemTypeId: {0}", customizationItem.Id), new object[0]);
				this.CompleteSkinBuy();
				return;
			}
			SkinItemTypeBag skinItemTypeBag = (SkinItemTypeBag)((JsonSerializeable<!0>)customizationItem.Bag);
			characterBag.SetUnlockSeen(skinItemTypeBag.UnlockLevel);
			CharacterCustomWS.UpdateCharacterUnlockMask(characterBag, new SwordfishClientApi.ParameterizedCallback<string>(this.OnUpdateCharacterUnlockMaskSuccess), new SwordfishClientApi.ErrorCallback(this.OnUpdateCharacterUnlockMaskError));
		}

		private void OnUpdateCharacterUnlockMaskError(object state, Exception exception)
		{
			ShopDetails.Log.ErrorFormat(string.Format("Error on OnUpdateCharacterUnlockMaskError. PlayerId: {0}, Error: {1}", GameHubBehaviour.Hub.User.PlayerSF.Id, exception.Message), new object[0]);
			this.CompleteSkinBuy();
		}

		private void OnUpdateCharacterUnlockMaskSuccess(object state, string obj)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<!0>)obj);
			if (!netResult.Success)
			{
				ShopDetails.Log.ErrorFormat(string.Format("Error on OnUpdateCharacterUnlockMaskSuccess. PlayerId: {0}, Error: {1}", GameHubBehaviour.Hub.User.PlayerSF.Id, netResult.Msg), new object[0]);
			}
			this.CompleteSkinBuy();
		}

		private void ConfigureCarPreview(IItemType skinItemType)
		{
			this._modelViewerTexture.gameObject.SetActive(true);
			this._shopDetailsSkinInfo.Disable();
			ShopItemTypeComponent component = skinItemType.GetComponent<ShopItemTypeComponent>();
			switch (component.PreviewKind)
			{
			case ItemPreviewKind.None:
				goto IL_C4;
			case ItemPreviewKind.Sprite:
			case ItemPreviewKind.Video:
			case ItemPreviewKind.SmallSprite:
				throw new NotImplementedException(string.Format("Not Implemented ItemPreviewKind: {0}", component.PreviewKind));
			case ItemPreviewKind.Model3D:
			{
				ItemTypeComponent itemTypeComponent;
				if (skinItemType.GetComponentByEnum(ItemTypeComponent.Type.SkinPrefab, out itemTypeComponent))
				{
					this._shopDetailsSkinInfo.Setup((SkinPrefabItemTypeComponent)itemTypeComponent);
				}
				base.StartCoroutine(this.ShowAssetCoroutine(component.ArtAssetName));
				goto IL_C4;
			}
			}
			throw new ArgumentException(string.Format("Unknown ItemPreviewKind: {0}", component.PreviewKind));
			IL_C4:
			ShopDetailsComponent component2 = skinItemType.GetComponent<ShopDetailsComponent>();
			string skinShopPreviewSpriteName = component2.SkinShopPreviewSpriteName;
			this.CharacterNameTexture.SpriteName = skinShopPreviewSpriteName;
			this.SkinCharacterNameTexture.SpriteName = skinShopPreviewSpriteName;
		}

		private IEnumerator ShowAssetCoroutine(string assetName)
		{
			yield return null;
			while (!this.ModelViewer.IsReady)
			{
				yield return null;
			}
			this._animatedAssetPresenter.ShowAsset(assetName);
			this._shopDetailsSkinInfo.GridReposition();
			yield break;
		}

		private string GetDifficultyLocalizedText(CharacterDifficulty difficulty)
		{
			switch (difficulty)
			{
			case CharacterDifficulty.DifficultyLevel1:
				return Language.Get("DIFFICULTY_LEVEL_1", TranslationContext.CharactersBaseInfo);
			case CharacterDifficulty.DifficultyLevel2:
				return Language.Get("DIFFICULTY_LEVEL_2", TranslationContext.CharactersBaseInfo);
			case CharacterDifficulty.DifficultyLevel3:
				return Language.Get("DIFFICULTY_LEVEL_3", TranslationContext.CharactersBaseInfo);
			case CharacterDifficulty.DifficultyLevel4:
				return Language.Get("DIFFICULTY_LEVEL_4", TranslationContext.CharactersBaseInfo);
			case CharacterDifficulty.DifficultyLevel5:
				return Language.Get("DIFFICULTY_LEVEL_5", TranslationContext.CharactersBaseInfo);
			default:
				return Language.Get("DIFFICULTY_LEVEL_1", TranslationContext.CharactersBaseInfo);
			}
		}

		private void Setdifficulty(CharacterItemTypeComponent charComponent)
		{
			this.CharacterDificultLevel.value = (float)((CharacterDifficulty)20 * charComponent.Difficulty) * 0.01f;
			this.CharacterDificultLevelLabel.text = this.GetDifficultyLocalizedText(charComponent.Difficulty);
			Color color = Color.red;
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.GuiScripts)
			{
				switch (charComponent.Difficulty)
				{
				case CharacterDifficulty.DifficultyLevel1:
					color = GUIColorsInfo.Instance.DifficultyLevel1;
					break;
				case CharacterDifficulty.DifficultyLevel2:
					color = GUIColorsInfo.Instance.DifficultyLevel2;
					break;
				case CharacterDifficulty.DifficultyLevel3:
					color = GUIColorsInfo.Instance.DifficultyLevel3;
					break;
				case CharacterDifficulty.DifficultyLevel4:
					color = GUIColorsInfo.Instance.DifficultyLevel4;
					break;
				case CharacterDifficulty.DifficultyLevel5:
					color = GUIColorsInfo.Instance.DifficultyLevel5;
					break;
				}
			}
			this.CharacterDificultLevel.foregroundWidget.color = color;
			this.CharacterDificultLevelLabel.color = color;
		}

		private IEnumerator WaitAndToggle(UIToggle Toggle, bool value)
		{
			yield return UnityUtils.WaitForEndOfFrame;
			Toggle.value = value;
			Toggle.Set(value, true);
			yield break;
		}

		private TooltipInfo GetGadgetTooltipData(string charName, string gadgetDraftKey, GadgetSlot slot)
		{
			string gadgetIconName = HudUtils.GetGadgetIconName(charName, slot);
			string text = string.Format("{0}_GADGET_{1}_NAME", charName, gadgetDraftKey);
			string text2 = string.Format("{0}_GADGET_{1}_DESC", charName, gadgetDraftKey);
			string text3 = string.Format("{0}_GADGET_{1}_COOLDOWN", charName, gadgetDraftKey);
			text = Language.Get(text, TranslationContext.CharactersMatchInfo);
			text2 = Language.Get(text2, TranslationContext.CharactersMatchInfo);
			if (Language.Has(text3, TranslationContext.CharactersMatchInfo))
			{
				text3 = Language.Get(text3, TranslationContext.CharactersMatchInfo);
			}
			else
			{
				text3 = string.Empty;
			}
			return new TooltipInfo(TooltipInfo.TooltipType.Normal, TooltipInfo.DescriptionSummaryType.None, this.TooltipGadgetAnchor, null, gadgetIconName, text, string.Empty, text2, text3, string.Empty, string.Empty, this.TooltipGadgetPivot.position, string.Empty);
		}

		[UnityUiComponentCall]
		public void ShowSkillTooltip(object gadgetIndex)
		{
			if (!GameHubBehaviour.Hub.GuiScripts)
			{
				return;
			}
			CharacterItemTypeComponent component = this._currentCharItemType.GetComponent<CharacterItemTypeComponent>();
			if (component == null)
			{
				return;
			}
			string gadgetDraftKey;
			GadgetSlot slot;
			switch ((int)gadgetIndex)
			{
			case 1:
				gadgetDraftKey = "00";
				slot = GadgetSlot.CustomGadget0;
				break;
			case 2:
				gadgetDraftKey = "01";
				slot = GadgetSlot.CustomGadget1;
				break;
			case 3:
				gadgetDraftKey = "02";
				slot = GadgetSlot.CustomGadget2;
				break;
			case 4:
				gadgetDraftKey = "03";
				slot = GadgetSlot.BoostGadget;
				break;
			default:
				gadgetDraftKey = "PASSIVE";
				slot = GadgetSlot.PassiveGadget;
				break;
			}
			TooltipInfo gadgetTooltipData = this.GetGadgetTooltipData(component.AssetPrefix.ToUpper(), gadgetDraftKey, slot);
			TooltipController tooltipController = GameHubBehaviour.Hub.GuiScripts.TooltipController;
			if (!tooltipController.IsVisible())
			{
				tooltipController.ToggleOpenWindow(gadgetTooltipData);
			}
		}

		public void HideTooltip()
		{
			if (GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.TooltipController.HideWindow();
			}
		}

		private void OnDestroy()
		{
			this.HideTooltip();
		}

		public void ShowRoleTooltip(GameObject roleGameobject)
		{
			string simpleText = string.Empty;
			if (roleGameobject.name == this.TacklerRole.name)
			{
				simpleText = Language.Get("TACKLER_ROLE_DESCRIPTION", TranslationContext.CharactersBaseInfo);
			}
			else if (roleGameobject.name == this.CarrierRole.name)
			{
				simpleText = Language.Get("CARRIER_ROLE_DESCRIPTION", TranslationContext.CharactersBaseInfo);
			}
			else
			{
				simpleText = Language.Get("SUPPORT_ROLE_DESCRIPTION", TranslationContext.CharactersBaseInfo);
			}
			TooltipInfo tooltipInfo = new TooltipInfo(TooltipInfo.TooltipType.SimpleText, TooltipInfo.DescriptionSummaryType.None, this.TooltipRoleAnchor, null, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, this.TooltipRolePivot.position, simpleText);
			if (GameHubBehaviour.Hub.GuiScripts && !GameHubBehaviour.Hub.GuiScripts.TooltipController.IsVisible())
			{
				GameHubBehaviour.Hub.GuiScripts.TooltipController.ToggleOpenWindow(tooltipInfo);
			}
		}

		[Inject]
		private readonly ILocalBalanceStorage _localBalanceStorage;

		public static readonly BitLogger Log = new BitLogger(typeof(ShopGUI));

		[Header("EXTERNAL")]
		public ShopGUI Shop;

		[Header("INTERNAL")]
		public GameObject rootGameObject;

		public GameObject skinDetailsGameObject;

		public GameObject driverDetailGameObject;

		public UIToggle DriverToggle;

		public UIToggle SkinToggle;

		public UI2DSprite DriverConcept;

		public BaseModelViewer ModelViewer;

		[SerializeField]
		private UITexture _modelViewerTexture;

		public UI2DSprite DriverBackground;

		public GameObject BackButton;

		public UIButton DetailsBackButton;

		[SerializeField]
		private GameObject _topTabsGroup;

		[SerializeField]
		private ItemTypeShopDetails _itemTypeShopDetails;

		[Header("DRIVER")]
		public HMMUI2DDynamicSprite CharacterNameTexture;

		public UILabel CharacterNameLabel;

		public UILabel CharacterDescription;

		public UISlider CharacterDificultLevel;

		public UILabel CharacterDificultLevelLabel;

		public GameObject SuportRole;

		public GameObject CarrierRole;

		public GameObject TacklerRole;

		public UIGrid SkillsGrid;

		public HMMUI2DDynamicSprite Skill01;

		public HMMUI2DDynamicSprite Skill02;

		public HMMUI2DDynamicSprite Skill03;

		public HMMUI2DDynamicSprite SkillNitro;

		public HMMUI2DDynamicSprite SkillPassive;

		public PreferredDirection TooltipGadgetAnchor;

		public Transform TooltipGadgetPivot;

		public PreferredDirection TooltipRoleAnchor;

		public Transform TooltipRolePivot;

		public UILabel DriverSoftPrice;

		public UILabel DriverHardPrice;

		public UI2DSprite DriverSoftIconPrice;

		public UI2DSprite DriverHardIconPrice;

		public GameObject BuyGroup;

		public GameObject BoughtGroup;

		public GameObject OpenGuideButton;

		[Header("SKIN")]
		[SerializeField]
		private Animation _mainAnimation;

		public UILabel DriverName;

		public HMMUI2DDynamicSprite SkinCharacterNameTexture;

		public GameObject SkinSuportRole;

		public GameObject SkinCarrierRole;

		public GameObject SkinTacklerRole;

		public AnimationCurve SkinsRotationCurveSpeed;

		public UILabel SkinName;

		public HMMUILabel SkinDescription;

		public UILabel SkinQuote;

		public StoreItem SkinCardPrefab;

		public StoreItem[] SkinCards;

		public Transform skinCardsPivot;

		public IItemType CurrentSkinItemType;

		public int currentSkinCardsCenteredIndex;

		public GameObject SkinBuyGroup;

		public GameObject SkinBoughtGroup;

		public GameObject SkinLockedGroup;

		[SerializeField]
		private GameObject _skinFeedbackDontHavePilot;

		[SerializeField]
		private UIButton _skinBuyButton;

		public UILabel SkinUnlockLevelLabel;

		public UILabel hardPriceLabel;

		public UI2DSprite hardPriceIcon;

		public UILabel softPriceLabel;

		public UI2DSprite softPriceIcon;

		public GameObject missionPrefab;

		[SerializeField]
		private ShopDetailsSkinInfo _shopDetailsSkinInfo;

		private AnimatedAssetPresenter _animatedAssetPresenter;

		[InjectOnClient]
		private IStoreBusinessFactory _storeBusinessFactory;

		private IDisposable _storeItemObservation;

		private List<StoreItem> _activeSkinCards = new List<StoreItem>();

		public ShopDetails.State CurrentState;

		public ShopDetails.NavigationContext currentNavigationContext;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		private IItemType _currentCharItemType;

		private List<ItemTypeScriptableObject> _currentCharacterOrderedCustomizations;

		private Sprite _driverConceptCache;

		private Sprite _characterNameTextureCache;

		private Texture _animatedTextureCache;

		private int defaultSkinCardsCenteredIndex;

		private bool _shouldCycleToCurrentSkin;

		[Inject]
		private ICharacterHelpPresenter _characterHelpPresenter;

		private float Tunning = 0.25f;

		private float maxX = 175f;

		private float maxY = 50f;

		private int baseDepth = 50;

		private int _targetSkinCardsChosenSkinIndex;

		private int _wayToCicle;

		public enum State
		{
			None,
			Driver,
			Skin
		}

		public enum NavigationContext
		{
			Toggling,
			buying
		}
	}
}
