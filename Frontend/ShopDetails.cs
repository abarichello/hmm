using System;
using System.Collections;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Specializations;
using ClientAPI;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using ModelViewer;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ShopDetails : GameHubBehaviour
	{
		private void OnEnable()
		{
			this._driverConceptCache = this.DriverConcept.sprite2D;
			this._characterNameTextureCache = this.CharacterNameTexture.sprite2D;
			this._modelViewerTexture.gameObject.SetActive(false);
			UnityAnimation fadeOutAnimation = new UnityAnimation(this._mainAnimation, "3dModelOut");
			UnityAnimation fadeInAnimation = new UnityAnimation(this._mainAnimation, "3dModelin");
			DynamicAssetLoader assetLoader = new DynamicAssetLoader(SingletonMonoBehaviour<LoadingManager>.Instance.PrefabManager);
			this._animatedAssetPresenter = new AnimatedAssetPresenter(fadeInAnimation, fadeOutAnimation, assetLoader, this.ModelViewer);
		}

		private void OnDisable()
		{
			this.ModelViewer.ModelName = null;
			this._modelViewerTexture.gameObject.SetActive(false);
			this.DriverConcept.sprite2D = this._driverConceptCache;
			this.CharacterNameTexture.sprite2D = this._characterNameTextureCache;
			this._animatedAssetPresenter.Dispose();
			this._animatedAssetPresenter = null;
		}

		private void Update()
		{
			this._animatedAssetPresenter.Update();
		}

		public void Show()
		{
			this.rootGameObject.SetActive(true);
			this.DetailsBackButton.CacheDefaultColor();
			this.currentNavigationContext = ShopDetails.NavigationContext.buying;
			this.BackButton.SetActive(true);
		}

		public void Hide()
		{
			this.InternalHide(false);
		}

		private void InternalHide(bool goToShopCash)
		{
			this.DriverToggle.value = false;
			this.SkinToggle.value = false;
			if (UICamera.currentScheme == UICamera.ControlScheme.Controller && !goToShopCash && this.currentNavigationContext == ShopDetails.NavigationContext.buying)
			{
				this.currentNavigationContext = ShopDetails.NavigationContext.Toggling;
				return;
			}
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
		}

		private void ConfigureForDriver(ItemTypeScriptableObject charHierarchy)
		{
			this._currentCharacterHierarchy = charHierarchy;
			this.PopulateCharacterOrderedCustomization(charHierarchy.Id);
			this.CurrentSkinItemType = charHierarchy;
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.Setup(this._currentCharacterHierarchy, GameHubBehaviour.Hub.State.Current);
			this.skinDetailsGameObject.SetActive(false);
			this.driverDetailGameObject.SetActive(true);
			CharacterItemTypeComponent component = charHierarchy.GetComponent<CharacterItemTypeComponent>();
			this.CharacterDescription.text = Language.Get(component.DescriptionDraft, TranslationSheets.CharactersBaseInfo);
			Guid id = charHierarchy.Id;
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
			this.characterInfo = null;
			GameHubBehaviour.Hub.InventoryColletion.CharactersByTypeId.TryGetValue(id, out this.characterInfo);
			if (this.characterInfo)
			{
				this.CharacterNameLabel.text = this.characterInfo.LocalizedName;
				this.UpdateRole(this.characterInfo.Role, this.SuportRole, this.CarrierRole, this.TacklerRole);
				this.Setdifficulty(this.characterInfo);
				this.Skill01.SpriteName = this.characterInfo.Asset + "_Gadget01";
				this.Skill02.SpriteName = this.characterInfo.Asset + "_Gadget02";
				this.Skill03.SpriteName = this.characterInfo.Asset + "_Gadget03";
				this.SkillNitro.SpriteName = this.characterInfo.Asset + "_GadgetNitro";
				this.SkillPassive.transform.parent.gameObject.SetActive(this.characterInfo.HasPassive);
				if (this.characterInfo.HasPassive)
				{
					this.SkillPassive.SpriteName = this.characterInfo.Asset + "_GadgetPassive";
				}
				this.SkillsGrid.repositionNow = true;
				this.ConfigureCarPreview(this.CurrentSkinItemType);
				this.OpenGuideButton.SetActive(!string.IsNullOrEmpty(this.characterInfo.URLName));
			}
			int num;
			int num2;
			GameHubBehaviour.Hub.Store.GetItemPrice(id, out num, out num2, false);
			this.DriverSoftPrice.text = string.Format("{0}", num);
			this.DriverHardPrice.text = string.Format("{0}", num2);
			if (charHierarchy.IsHardPurchasable && (long)num2 > GameHubBehaviour.Hub.Store.HardCurrency)
			{
				this.DriverHardPrice.color = Color.grey;
				this.DriverHardIconPrice.color = Color.grey;
			}
			else
			{
				this.DriverHardPrice.color = this.Shop.CashColor;
				this.DriverHardIconPrice.color = Color.white;
			}
			if (charHierarchy.IsSoftPurchasable && num > GameHubBehaviour.Hub.Store.SoftCurrency)
			{
				this.DriverSoftPrice.color = Color.grey;
				this.DriverSoftIconPrice.color = Color.grey;
			}
			else
			{
				this.DriverSoftPrice.color = this.Shop.FameColor;
				this.DriverSoftIconPrice.color = Color.white;
			}
			this._topTabsGroup.SetActive(true);
		}

		private void UpdateRole(HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind role, GameObject supportRole, GameObject carrierRole, GameObject tacklerRole)
		{
			carrierRole.SetActive(false);
			supportRole.SetActive(false);
			tacklerRole.SetActive(false);
			switch (role)
			{
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Support:
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.SupportCarrierTackler:
				supportRole.SetActive(true);
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Carrier:
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.CarrierSupport:
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.CarrierTackler:
				carrierRole.SetActive(true);
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Tackler:
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.TacklerSupport:
				tacklerRole.SetActive(true);
				break;
			}
		}

		public void ShowDriverDetails(ItemTypeScriptableObject charHierarchy)
		{
			this.CurrentState = ShopDetails.State.Driver;
			base.StartCoroutine(this.WaitAndToggle(this.DriverToggle, true));
			this.ConfigureForDriver(charHierarchy);
			this.Show();
		}

		public void BuyCharacter()
		{
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.ShowBuyWindow(this._currentCharacterHierarchy, new System.Action(this.OnCompleteCharacterBuy), new System.Action(this.OnBuyWindowClosed), new System.Action(this.OnGoToShopCash), 1, false);
		}

		private void OnGoToShopCash()
		{
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.CloseWindow();
			this.InternalHide(true);
		}

		private void OnCompleteCharacterBuy()
		{
			this.ConfigureForDriver(this._currentCharacterHierarchy);
			UICamera.controllerNavigationObject = this.BackButton;
		}

		private void OnBuyWindowClosed()
		{
			if (this.CurrentState == ShopDetails.State.Driver)
			{
				UICamera.controllerNavigationObject = this.BuyButton;
			}
			else
			{
				UICamera.controllerNavigationObject = this.BackButton;
			}
			Guid id = this._currentCharacterHierarchy.Id;
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

		private void ConfigureForSkin(StoreItem storeitem)
		{
			this.ConfigureForSkin(storeitem.StoreItemType, storeitem.CharacterItemTypeScriptableObject);
		}

		private void ConfigureForSkin(ItemTypeScriptableObject skinItemTypeScriptableObject, ItemTypeScriptableObject characterItemTypeScriptableObject)
		{
			this.CurrentSkinItemType = skinItemTypeScriptableObject;
			this._currentCharacterHierarchy = characterItemTypeScriptableObject;
			this.PopulateCharacterOrderedCustomization(this._currentCharacterHierarchy.Id);
			this.currentSkinCardsCenteredIndex = this.GetSkinCardIndexByGuid(skinItemTypeScriptableObject.Id);
			Guid id = characterItemTypeScriptableObject.Id;
			this.characterInfo = null;
			GameHubBehaviour.Hub.InventoryColletion.CharactersByTypeId.TryGetValue(id, out this.characterInfo);
			this.ConfigureForSkin(this.currentSkinCardsCenteredIndex);
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

		private void ConfigureForSkin(int centeredSkinIndex)
		{
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.Setup(this._currentCharacterHierarchy, GameHubBehaviour.Hub.State.Current);
			this.skinDetailsGameObject.SetActive(true);
			this.driverDetailGameObject.SetActive(false);
			Guid id = this._currentCharacterHierarchy.Id;
			HeavyMetalMachines.Character.CharacterInfo characterInfo;
			GameHubBehaviour.Hub.InventoryColletion.CharactersByTypeId.TryGetValue(id, out characterInfo);
			if (characterInfo)
			{
				this.CharacterNameLabel.text = characterInfo.LocalizedName;
				this.UpdateRole(characterInfo.Role, this.SkinSuportRole, this.SkinCarrierRole, this.SkinTacklerRole);
			}
			int num = 0;
			List<Guid> list = GameHubBehaviour.Hub.InventoryColletion.CharacterToSkinGuids[id];
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[list[i]];
				if (itemTypeScriptableObject.IsItemEnableInShop())
				{
					num++;
				}
			}
			bool purchasable = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id);
			if (this.SkinCards.Length == 0 || this.SkinCards.Length < num || this.SkinCards.Length > num)
			{
				if (this.SkinCards != null && this.SkinCards.Length > 0)
				{
					for (int j = 0; j < this.SkinCards.Length; j++)
					{
						if (this.SkinCards[j] != null && this.SkinCards[j].gameObject != null)
						{
							this.SkinCards[j].carTexture.SpriteName = string.Empty;
							UnityEngine.Object.Destroy(this.SkinCards[j].gameObject);
						}
					}
				}
				this.SkinCards = new StoreItem[num];
				this.ConfigureCardsForTier(this.SkinCards, purchasable, true);
			}
			else
			{
				this.ConfigureCardsForTier(this.SkinCards, purchasable, false);
			}
			Array.Sort<StoreItem>(this.SkinCards, new Comparison<StoreItem>(this.SkinCardsSortComparer));
			this.ConfigureNavigationIndicator(num, centeredSkinIndex);
			ItemTypeScriptableObject storeItemType = this.SkinCards[this.currentSkinCardsCenteredIndex].StoreItemType;
			this.ChangeSkin(storeItemType, this.currentSkinCardsCenteredIndex);
			this._topTabsGroup.SetActive(true);
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
					this.SkinUnlockLevelLabel.text = string.Format(Language.Get("SKINDETAIL_ITEMUNLOCK_LEVEL_LABEL", TranslationSheets.Store), 1 + unlockLevelForSkin);
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

		private void ConfigureCardsForTier(StoreItem[] skinCards, bool purchasable, bool create)
		{
			int num = 0;
			List<Guid> list = GameHubBehaviour.Hub.InventoryColletion.CharacterToSkinGuids[this._currentCharacterHierarchy.Id];
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[list[i]];
				if (itemTypeScriptableObject.IsItemEnableInShop())
				{
					StoreItem storeItem = (!create) ? skinCards[num] : UnityEngine.Object.Instantiate<StoreItem>(this.SkinCardPrefab);
					storeItem.gameObject.transform.parent = this.skinCardsPivot;
					storeItem.transform.localPosition = Vector3.one;
					storeItem.transform.localScale = Vector3.one;
					storeItem.StoreItemType = itemTypeScriptableObject;
					storeItem.carTexture.SpriteName = itemTypeScriptableObject.Name;
					storeItem.gameObject.name = itemTypeScriptableObject.Name;
					storeItem.characterName.text = Language.Get(string.Format("{0}_name", itemTypeScriptableObject.Name), "Items");
					storeItem.gameObject.SetActive(true);
					skinCards[num] = storeItem;
					num++;
				}
			}
		}

		private void ConfigureNavigationIndicator(int amountofavailableskins, int centeredSkinIndex)
		{
			if (this.NavigationIndicators != null && this.NavigationIndicators.Length > 0)
			{
				for (int i = 0; i < this.NavigationIndicators.Length; i++)
				{
					if (this.NavigationIndicators[i] != null && this.NavigationIndicators[i].gameObject != null)
					{
						UnityEngine.Object.Destroy(this.NavigationIndicators[i].gameObject);
					}
				}
			}
			this.NavigationIndicators = new GameObject[amountofavailableskins];
			for (int j = 0; j < amountofavailableskins; j++)
			{
				this.NavigationIndicators[j] = this.skinCardsIndicationPivot.gameObject.AddChild(this.NavigationIndicatorPrefab);
				this.NavigationIndicators[j].SetActive(true);
			}
			this.NavigationIndicators[centeredSkinIndex].GetComponent<UIToggle>().value = true;
			this.skinCardsIndicationPivot.GetComponent<UIGrid>().repositionNow = true;
		}

		public void ShowSkinDetails(StoreItem storeitem)
		{
			this.CurrentState = ShopDetails.State.Skin;
			base.StartCoroutine(this.WaitAndToggle(this.SkinToggle, true));
			this.Show();
			this.ConfigureForSkin(storeitem);
			this.MoveToChosenStoreItemSkin(storeitem);
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
			CharacterItemTypeBag characterItemTypeBag = (CharacterItemTypeBag)((JsonSerializeable<T>)itemTypeScriptableObject2.Bag);
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
				targetSkinCardsChosenSkinIndex = this.SkinCards.Length - 1;
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
			if (this.currentSkinCardsCenteredIndex >= this.SkinCards.Length - 1)
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

		public void MoveToChosenStoreItemSkin(StoreItem storeItem)
		{
			this.MoveToChosenStoreItemSkin(storeItem.StoreItemType);
		}

		public void MoveToChosenStoreItemSkin(ItemTypeScriptableObject itemTypeScriptableObject)
		{
			this._targetSkinCardsChosenSkinIndex = this.GetSkinCardIndexByGuid(itemTypeScriptableObject.Id);
			if (this._targetSkinCardsChosenSkinIndex == this.currentSkinCardsCenteredIndex)
			{
				return;
			}
			int num = this.currentSkinCardsCenteredIndex - this._targetSkinCardsChosenSkinIndex;
			if (num < 0)
			{
				num = this.SkinCards.Length - Mathf.Abs(num);
			}
			this._wayToCicle = -1;
			if (num > this.SkinCards.Length / 2)
			{
				this._wayToCicle = 1;
			}
			this.CicleToChoosenSkin();
		}

		private void CicleToChoosenSkin()
		{
			if (this._targetSkinCardsChosenSkinIndex == this.currentSkinCardsCenteredIndex)
			{
				UIButton[] components = this.SkinCards[this._targetSkinCardsChosenSkinIndex].GetComponents<UIButton>();
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
			if (this.currentSkinCardsCenteredIndex + how > this.SkinCards.Length - 1)
			{
				num = 0;
			}
			else if (this.currentSkinCardsCenteredIndex + how < 0)
			{
				num = this.SkinCards.Length - 1;
			}
			else
			{
				num = this.currentSkinCardsCenteredIndex + how;
			}
			this.currentSkinCardsCenteredIndex = num;
			this.NavigationIndicators[this.currentSkinCardsCenteredIndex].GetComponent<UIToggle>().value = true;
			ItemTypeScriptableObject storeItemType = this.SkinCards[this.currentSkinCardsCenteredIndex].StoreItemType;
			this.ChangeSkin(storeItemType, this.currentSkinCardsCenteredIndex);
		}

		private void ChangeSkin(ItemTypeScriptableObject skinitemtype, int indexfocus)
		{
			this.CurrentSkinItemType = skinitemtype;
			Guid id = this._currentCharacterHierarchy.Id;
			bool hasPilotAlready = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id);
			this.ConfigureSkinDescriptions(skinitemtype, this._currentCharacterHierarchy.Name);
			if (this.SkinCards != null && this.SkinCards.Length > 0)
			{
				float num = 1f / (float)this.SkinCards.Length;
				int depth = this.baseDepth;
				int a = Mathf.Abs(this._targetSkinCardsChosenSkinIndex - this.currentSkinCardsCenteredIndex) + 1;
				float time = 1f / (float)Mathf.Max(a, 1);
				float duration = this.SkinsRotationCurveSpeed.Evaluate(time);
				for (int i = 0; i < this.SkinCards.Length; i++)
				{
					NGUITools.MarkParentAsChanged(this.SkinCards[i].gameObject);
					int num2 = Mathf.Abs(indexfocus - i);
					TweenPosition component = this.SkinCards[i].GetComponent<TweenPosition>();
					component.SetStartToCurrentValue();
					float num3 = num * (float)(i - indexfocus) - this.Tunning;
					float f = num3 * 3.14159274f * 2f;
					float num4 = Mathf.Sin(f);
					float num5 = Mathf.Cos(f);
					component.to = new Vector3(num5 * this.maxX, (num4 + 1f) * this.maxY, 0f);
					component.duration = duration;
					int num6 = (num5 < 0f) ? 2 : 1;
					if (num2 != 0)
					{
						depth = this.baseDepth - 10 - Mathf.FloorToInt(num4 * 10f) - num6;
					}
					else
					{
						depth = this.baseDepth;
					}
					this.SkinCards[i].GetComponent<UIPanel>().depth = depth;
					if (component.onFinished != null)
					{
						component.onFinished.Clear();
					}
					component.ResetToBeginning();
					component.PlayForward();
				}
				this.ConfigureCenteredSkin(skinitemtype, indexfocus, hasPilotAlready);
				TweenPosition component2 = this.SkinCards[0].GetComponent<TweenPosition>();
				EventDelegate.Add(component2.onFinished, new EventDelegate.Callback(this.CicleToChoosenSkin), true);
			}
			this.ConfigureCarPreview(this.CurrentSkinItemType);
		}

		private void ConfigureCenteredSkin(ItemTypeScriptableObject skinitemtype, int center, bool hasPilotAlready)
		{
			CharacterItemTypeBag characterItemTypeBag = (CharacterItemTypeBag)((JsonSerializeable<T>)this._currentCharacterHierarchy.Bag);
			bool isBasicSkin = skinitemtype.Id == characterItemTypeBag.DefaultSkinGuid;
			Guid id = skinitemtype.Id;
			bool itemBought = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id);
			CharacterBag characterBag = GameHubBehaviour.Hub.User.GetCharacterBag(this._currentCharacterHierarchy.Id);
			PlayerBag playerBag = (PlayerBag)GameHubBehaviour.Hub.User.PlayerSF.Bag;
			SkinItemTypeBag skinItemTypeBag = (SkinItemTypeBag)((JsonSerializeable<T>)skinitemtype.Bag);
			bool hasLevelToBuy;
			if (characterBag != null)
			{
				int levelForXP = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetLevelForXP(characterBag.Xp);
				ProgressionInfo.CanBuyReason canBuyReason;
				string text;
				hasLevelToBuy = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.CanBuySkin(hasPilotAlready, playerBag, skinItemTypeBag, levelForXP, (CharacterItemTypeBag)((JsonSerializeable<T>)this._currentCharacterHierarchy.Bag), out canBuyReason, out text);
			}
			else
			{
				hasLevelToBuy = (skinItemTypeBag.UnlockLevel == 0);
			}
			this.ConfigureSkinBuyGroup(hasPilotAlready, isBasicSkin, itemBought, hasLevelToBuy, skinItemTypeBag.UnlockLevel);
			int num = (int)skinitemtype.ItemTypePrices[0].Price;
			int referenceHardPrice = skinitemtype.ReferenceHardPrice;
			bool hasSoftFunds = GameHubBehaviour.Hub.Store.SoftCurrency >= num;
			bool hasHardFunds = GameHubBehaviour.Hub.Store.HardCurrency >= (long)referenceHardPrice;
			bool isSoftPurchasable = skinitemtype.IsSoftPurchasable;
			bool isHardPurchasable = skinitemtype.IsHardPurchasable;
			this.ConfigureSkinCurrencyGroup(num, referenceHardPrice, isSoftPurchasable, isHardPurchasable, hasSoftFunds, hasHardFunds);
		}

		private void ConfigureSkinDescriptions(ItemTypeScriptableObject skinitemtype, string drivername)
		{
			SkinPrefabItemTypeComponent component = skinitemtype.GetComponent<SkinPrefabItemTypeComponent>();
			ShopDetailsComponent component2 = skinitemtype.GetComponent<ShopDetailsComponent>();
			CharacterItemTypeComponent component3 = this._currentCharacterHierarchy.GetComponent<CharacterItemTypeComponent>();
			this.DriverName.text = component3.MainAttributes.LocalizedName;
			this.SkinName.text = Language.Get(component.CardSkinDraft, TranslationSheets.Items);
			this.SkinDescription.text = Language.Get(component2.SkinQuoteTextDraf, TranslationSheets.Items);
			this.SkinQuote.text = Language.Get(component2.SkinQuoterAuthorDraft, TranslationSheets.Items);
			this.None_RarityGO.SetActive(false);
			this.Basic_RarityGO.SetActive(false);
			this.Bronze_RarityGO.SetActive(false);
			this.Silver_RarityGO.SetActive(false);
			this.Gold_RarityGO.SetActive(false);
			this.Diamond_RarityGO.SetActive(false);
			switch (component.Tier)
			{
			case SkinPrefabItemTypeComponent.TierKind.None:
				this.None_RarityGO.SetActive(true);
				break;
			case SkinPrefabItemTypeComponent.TierKind.Default:
				this.Basic_RarityGO.SetActive(true);
				break;
			case SkinPrefabItemTypeComponent.TierKind.Idol:
				this.Bronze_RarityGO.SetActive(true);
				break;
			case SkinPrefabItemTypeComponent.TierKind.Rockstar:
				this.Silver_RarityGO.SetActive(true);
				break;
			case SkinPrefabItemTypeComponent.TierKind.MetalLegend:
				this.Gold_RarityGO.SetActive(true);
				break;
			case SkinPrefabItemTypeComponent.TierKind.HeavyMetal:
				this.Diamond_RarityGO.SetActive(true);
				break;
			}
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
			int num = 0;
			List<ItemTypeScriptableObject> currentCharacterOrderedCustomizations = this._currentCharacterOrderedCustomizations;
			for (int i = 0; i < currentCharacterOrderedCustomizations.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = this._currentCharacterOrderedCustomizations[i];
				if (itemTypeScriptableObject.Id == pGuid)
				{
					return num;
				}
				if (itemTypeScriptableObject.IsItemEnableInShop())
				{
					num++;
				}
			}
			UnityEngine.Debug.LogError(string.Concat(new object[]
			{
				"skin, guid == ",
				pGuid,
				" not found on CurrentCharacterHierarchy.SkinItems of character",
				this._currentCharacterHierarchy.Name
			}), this);
			return 0;
		}

		public void BuySkin()
		{
			this._targetSkinCardsChosenSkinIndex = this.currentSkinCardsCenteredIndex;
			CharacterItemTypeBag characterItemTypeBag = (CharacterItemTypeBag)((JsonSerializeable<T>)this._currentCharacterHierarchy.Bag);
			if (this.CurrentSkinItemType.Id != characterItemTypeBag.DefaultSkinGuid)
			{
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.ShowBuyWindow(this.CurrentSkinItemType, new System.Action(this.OnCompleteSkinBuy), new System.Action(this.OnBuyWindowClosed), new System.Action(this.OnGoToShopCash), 1, false);
			}
		}

		private void OnCompleteSkinBuy()
		{
			this.UpdateCharacterUnlock(this.CurrentSkinItemType);
		}

		private void CompleteSkinBuy()
		{
			this.currentSkinCardsCenteredIndex = this.GetSkinCardIndexByGuid(this.CurrentSkinItemType.Id);
			this.ConfigureForSkin(this.currentSkinCardsCenteredIndex);
			UICamera.controllerNavigationObject = this.BackButton;
		}

		private void UpdateCharacterUnlock(ItemTypeScriptableObject customizationItem)
		{
			CharacterBag characterBag = GameHubBehaviour.Hub.User.GetCharacterBag(this._currentCharacterHierarchy.Id);
			if (characterBag == null)
			{
				this.CompleteSkinBuy();
				return;
			}
			SkinItemTypeBag skinItemTypeBag = (SkinItemTypeBag)((JsonSerializeable<T>)customizationItem.Bag);
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
			NetResult netResult = (NetResult)((JsonSerializeable<T>)obj);
			if (!netResult.Success)
			{
				ShopDetails.Log.ErrorFormat(string.Format("Error on OnUpdateCharacterUnlockMaskSuccess. PlayerId: {0}, Error: {1}", GameHubBehaviour.Hub.User.PlayerSF.Id, netResult.Msg), new object[0]);
			}
			this.CompleteSkinBuy();
		}

		public void SwitchToDriverDetails()
		{
			if (this.CurrentState == ShopDetails.State.Driver)
			{
				return;
			}
			this.currentNavigationContext = ShopDetails.NavigationContext.buying;
			this.CurrentState = ShopDetails.State.Driver;
			this.ConfigureForDriver(this._currentCharacterHierarchy);
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
			List<Guid> list = GameHubBehaviour.Hub.InventoryColletion.CharacterToSkinGuids[this._currentCharacterHierarchy.Id];
			if (this.currentSkinCardsCenteredIndex == this.defaultSkinCardsCenteredIndex)
			{
				for (int i = 0; i < list.Count; i++)
				{
					ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[list[i]];
					if (itemTypeScriptableObject.IsItemEnableInShop())
					{
						this._targetSkinCardsChosenSkinIndex = this.GetSkinCardIndexByGuid(itemTypeScriptableObject.Id);
					}
				}
				this.defaultSkinCardsCenteredIndex = this._targetSkinCardsChosenSkinIndex;
			}
			this.currentSkinCardsCenteredIndex = this._targetSkinCardsChosenSkinIndex;
			this.ConfigureForSkin(this.currentSkinCardsCenteredIndex);
			this.skinDetailsGameObject.SetActive(true);
			this.driverDetailGameObject.SetActive(false);
		}

		private void ConfigureCarPreview(ItemTypeScriptableObject skinItemType)
		{
			this._modelViewerTexture.gameObject.SetActive(true);
			this._shopDetailsSkinInfo.Disable();
			ShopItemTypeComponent component = skinItemType.GetComponent<ShopItemTypeComponent>();
			switch (component.PreviewKind)
			{
			case ItemPreviewKind.None:
				break;
			case ItemPreviewKind.Sprite:
			case ItemPreviewKind.Video:
				throw new NotImplementedException(string.Format("Not Implemented ItemPreviewKind: {0}", component.PreviewKind));
			case ItemPreviewKind.Model3D:
			{
				ItemTypeComponent itemTypeComponent;
				if (skinItemType.GetComponentByEnum(ItemTypeComponent.Type.SkinPrefab, out itemTypeComponent))
				{
					this._shopDetailsSkinInfo.Setup(((SkinPrefabItemTypeComponent)itemTypeComponent).SkinCustomization);
				}
				base.StartCoroutine(this.ShowAssetCoroutine(component.ArtAssetName));
				break;
			}
			default:
				throw new ArgumentException(string.Format("Unknown ItemPreviewKind: {0}", component.PreviewKind));
			}
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

		private void Setdifficulty(HeavyMetalMachines.Character.CharacterInfo charInfo)
		{
			this.CharacterDificultLevel.value = (float)(20 * charInfo.Dificult) * 0.01f;
			this.CharacterDificultLevelLabel.text = charInfo.GetDifficultTranslatedText();
			Color color = Color.red;
			if (charInfo)
			{
				HeavyMetalMachines.Character.CharacterInfo.Difficulty difficultyKind = charInfo.GetDifficultyKind();
				if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.GuiScripts)
				{
					switch (difficultyKind)
					{
					case HeavyMetalMachines.Character.CharacterInfo.Difficulty.DifficultyLevel1:
						color = GUIColorsInfo.Instance.DifficultyLevel1;
						break;
					case HeavyMetalMachines.Character.CharacterInfo.Difficulty.DifficultyLevel2:
						color = GUIColorsInfo.Instance.DifficultyLevel2;
						break;
					case HeavyMetalMachines.Character.CharacterInfo.Difficulty.DifficultyLevel3:
						color = GUIColorsInfo.Instance.DifficultyLevel3;
						break;
					case HeavyMetalMachines.Character.CharacterInfo.Difficulty.DifficultyLevel4:
						color = GUIColorsInfo.Instance.DifficultyLevel4;
						break;
					case HeavyMetalMachines.Character.CharacterInfo.Difficulty.DifficultyLevel5:
						color = GUIColorsInfo.Instance.DifficultyLevel5;
						break;
					}
				}
				this.CharacterDificultLevel.foregroundWidget.color = color;
				this.CharacterDificultLevelLabel.color = color;
			}
		}

		private IEnumerator WaitAndToggle(UIToggle Toggle, bool value)
		{
			yield return UnityUtils.WaitForEndOfFrame;
			Toggle.value = value;
			Toggle.Set(value, true);
			yield break;
		}

		public void ShowSkillTooltip(object o)
		{
			HeavyMetalMachines.Character.CharacterInfo characterInfo = null;
			GameHubBehaviour.Hub.InventoryColletion.CharactersByTypeId.TryGetValue(this._currentCharacterHierarchy.Id, out characterInfo);
			string upgradeDescription;
			GadgetInfo gadgetInfo;
			GadgetSlot gadgetSlot;
			switch ((int)o)
			{
			case 0:
				upgradeDescription = HudGarageShopGadgetObject.GetUpgradeDescription(characterInfo.PassiveGadget, characterInfo.PassiveGadget.LocalizedDescription, characterInfo.PassiveGadget.LocalizedName);
				gadgetInfo = characterInfo.PassiveGadget;
				gadgetSlot = GadgetSlot.PassiveGadget;
				break;
			case 1:
				upgradeDescription = HudGarageShopGadgetObject.GetUpgradeDescription(characterInfo.CustomGadget0, characterInfo.CustomGadget0.LocalizedDescription, characterInfo.CustomGadget0.LocalizedName);
				gadgetInfo = characterInfo.CustomGadget0;
				gadgetSlot = GadgetSlot.CustomGadget0;
				break;
			case 2:
				upgradeDescription = HudGarageShopGadgetObject.GetUpgradeDescription(characterInfo.CustomGadget1, characterInfo.CustomGadget1.LocalizedDescription, characterInfo.CustomGadget1.LocalizedName);
				gadgetInfo = characterInfo.CustomGadget1;
				gadgetSlot = GadgetSlot.CustomGadget1;
				break;
			case 3:
				upgradeDescription = HudGarageShopGadgetObject.GetUpgradeDescription(characterInfo.CustomGadget2, characterInfo.CustomGadget2.LocalizedDescription, characterInfo.CustomGadget2.LocalizedName);
				gadgetInfo = characterInfo.CustomGadget2;
				gadgetSlot = GadgetSlot.CustomGadget2;
				break;
			case 4:
				upgradeDescription = HudGarageShopGadgetObject.GetUpgradeDescription(characterInfo.BoostGadget, characterInfo.BoostGadget.LocalizedDescription, characterInfo.BoostGadget.LocalizedName);
				gadgetInfo = characterInfo.BoostGadget;
				gadgetSlot = GadgetSlot.BoostGadget;
				break;
			default:
				upgradeDescription = HudGarageShopGadgetObject.GetUpgradeDescription(characterInfo.PassiveGadget, characterInfo.PassiveGadget.LocalizedDescription, characterInfo.PassiveGadget.LocalizedName);
				gadgetInfo = characterInfo.PassiveGadget;
				gadgetSlot = GadgetSlot.PassiveGadget;
				break;
			}
			TooltipInfo tooltipInfo = new TooltipInfo(TooltipInfo.TooltipType.Normal, TooltipInfo.DescriptionSummaryType.None, this.TooltipGadgetAnchor, null, HudUtils.GetGadgetIconName(characterInfo.Asset, gadgetSlot), gadgetInfo.LocalizedName, string.Empty, upgradeDescription, gadgetInfo.LocalizedCooldownDescription, string.Empty, string.Empty, this.TooltipGadgetPivot.position, string.Empty);
			if (GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.TooltipController.TryToOpenWindow(tooltipInfo);
			}
		}

		public void HideTooltip()
		{
			if (GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.TooltipController.HideWindow();
			}
		}

		public void ShowRoleTooltip(GameObject roleGameobject)
		{
			string simpleText = string.Empty;
			if (roleGameobject.name == this.TacklerRole.name)
			{
				simpleText = Language.Get("TACKLER_ROLE_DESCRIPTION", TranslationSheets.CharactersBaseInfo);
			}
			else if (roleGameobject.name == this.CarrierRole.name)
			{
				simpleText = Language.Get("CARRIER_ROLE_DESCRIPTION", TranslationSheets.CharactersBaseInfo);
			}
			else
			{
				simpleText = Language.Get("SUPPORT_ROLE_DESCRIPTION", TranslationSheets.CharactersBaseInfo);
			}
			TooltipInfo tooltipInfo = new TooltipInfo(TooltipInfo.TooltipType.SimpleText, TooltipInfo.DescriptionSummaryType.None, this.TooltipRoleAnchor, null, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, this.TooltipRolePivot.position, simpleText);
			if (GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.TooltipController.TryToOpenWindow(tooltipInfo);
			}
		}

		public bool IsVisible()
		{
			return this.rootGameObject.activeInHierarchy;
		}

		public void OpenGuideUrl()
		{
			HeavyMetalMachines.Utils.Debug.Assert(!string.IsNullOrEmpty(this.characterInfo.URLName), "[GD] Character Main Attribute URLName is not set. Guide Url is Invalid.", HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			if (string.IsNullOrEmpty(this.characterInfo.URLName))
			{
				return;
			}
			string value = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.CharacterGuideURL);
			OpenUrlUtils.OpenSteamUrl(GameHubBehaviour.Hub, string.Format(value, this.characterInfo.URLName, Language.CurrentLanguage()));
		}

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

		public GameObject BuyButton;

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

		public GameObject NavigationIndicatorPrefab;

		public GameObject[] NavigationIndicators;

		public Transform skinCardsIndicationPivot;

		public ItemTypeScriptableObject CurrentSkinItemType;

		public int currentSkinCardsCenteredIndex;

		public GameObject Basic_RarityGO;

		public GameObject Bronze_RarityGO;

		public GameObject Silver_RarityGO;

		public GameObject Gold_RarityGO;

		public GameObject Diamond_RarityGO;

		public GameObject None_RarityGO;

		public Sprite normalCardBorder;

		public Sprite boughtCardBorder;

		public Sprite nofundsCardBorder;

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

		private HeavyMetalMachines.Character.CharacterInfo characterInfo;

		private AnimatedAssetPresenter _animatedAssetPresenter;

		public ShopDetails.State CurrentState;

		public ShopDetails.NavigationContext currentNavigationContext;

		private ItemTypeScriptableObject _currentCharacterHierarchy;

		private List<ItemTypeScriptableObject> _currentCharacterOrderedCustomizations;

		private Sprite _driverConceptCache;

		private Sprite _characterNameTextureCache;

		private int defaultSkinCardsCenteredIndex;

		private float Tunning = 0.25f;

		private float maxX = 175f;

		private float maxY = 50f;

		private int baseDepth = 50;

		private int _targetSkinCardsChosenSkinIndex;

		private int _wayToCicle;

		public enum State
		{
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
