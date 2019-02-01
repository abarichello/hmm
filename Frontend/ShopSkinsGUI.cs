using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ShopSkinsGUI : ShopScreen
	{
		public void OnHideMainWindow()
		{
			if (!this.IsVisible())
			{
				this.DisposeAll();
				return;
			}
			this._disposeAllOnAlphaZero = true;
		}

		public override void AnimationAlphaZero()
		{
			base.AnimationAlphaZero();
			if (this._disposeAllOnAlphaZero)
			{
				this._disposeAllOnAlphaZero = false;
				this.DisposeAll();
			}
		}

		private void DisposeAll()
		{
			for (int i = 0; i < this.AllItems.Count; i++)
			{
				StoreItem storeItem = this.AllItems[i];
				storeItem.CleanUp();
				UnityEngine.Object.Destroy(storeItem.gameObject);
			}
			this.AllItems.Clear();
			this.CurrentItems.Clear();
			this.CurrentItemsById.Clear();
		}

		public override void Setup()
		{
			base.Setup();
			this._disposeAllOnAlphaZero = false;
			if (this.AllItems.Count == 0)
			{
				this.SetupSkins();
				this.TotalNumberOfFilters = Enum.GetNames(typeof(ShopSkinsGUI.SkinFilter)).Length;
			}
			else
			{
				this.UpdateSkins();
			}
		}

		private void UpdateSkins()
		{
			for (int i = 0; i < this.AllItems.Count; i++)
			{
				StoreItem storeItem = this.AllItems[i];
				Guid id = storeItem.StoreItemType.Id;
				bool flag = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id);
				bool isUnlocked = flag;
				this.SetupAvaliableSkinBoughtInformation(storeItem, isUnlocked, id);
			}
		}

		private void SetupSkins()
		{
			List<ItemTypeScriptableObject> list = GameHubBehaviour.Hub.InventoryColletion.CategoriesIdToItemTypes[this._characterItemTypeCategoryScriptableObject.Id];
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = list[i];
				if (!itemTypeScriptableObject.Deleted)
				{
					CharacterItemTypeComponent component = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
					string characterIcon128Name = component.CharacterIcon128Name;
					List<Guid> list2 = GameHubBehaviour.Hub.InventoryColletion.CharacterToSkinGuids[itemTypeScriptableObject.Id];
					for (int j = 0; j < list2.Count; j++)
					{
						ItemTypeScriptableObject itemTypeScriptableObject2 = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[list2[j]];
						if (!itemTypeScriptableObject2.Deleted)
						{
							SkinPrefabItemTypeComponent component2 = itemTypeScriptableObject2.GetComponent<SkinPrefabItemTypeComponent>();
							string skinSpriteName = component2.SkinSpriteName;
							StoreItem storeItem = this.AddSkin(itemTypeScriptableObject2, component2, itemTypeScriptableObject, skinSpriteName, characterIcon128Name);
							this.AllItems.Add(storeItem);
							this.CurrentItemsById[storeItem.StoreItemType.Id.ToString()] = storeItem;
						}
					}
				}
			}
			List<StoreItem> allItems = this.AllItems;
			if (ShopSkinsGUI.<>f__mg$cache0 == null)
			{
				ShopSkinsGUI.<>f__mg$cache0 = new Comparison<StoreItem>(CharactersItem.CompareCharactersName);
			}
			allItems.Sort(ShopSkinsGUI.<>f__mg$cache0);
			base.SetupPageToggleControllers(Mathf.CeilToInt((float)this.AllItems.Count / 8f));
		}

		private StoreItem AddSkin(ItemTypeScriptableObject skinItemType, SkinPrefabItemTypeComponent skinComponent, ItemTypeScriptableObject characterItemType, string cartexturename, string iconname)
		{
			StoreItem storeItem = this.skinItemPrefab;
			StoreItem storeItem2 = UnityEngine.Object.Instantiate<StoreItem>(storeItem, new Vector3(-10000f, -10000f, 0f), Quaternion.identity);
			storeItem2.StoreItemType = skinItemType;
			storeItem2.CharacterItemTypeScriptableObject = characterItemType;
			storeItem2.SetSpriteNames(iconname, cartexturename);
			EventDelegate eventDelegate = new EventDelegate(this, "GetFocusOnSkinDetailsScreen");
			eventDelegate.parameters[0] = new EventDelegate.Parameter(storeItem2, "gameObject");
			storeItem2.previewButton.onClick.Add(eventDelegate);
			storeItem2.UnlockButton.onClick.Add(eventDelegate);
			storeItem2.transform.parent = storeItem.transform.parent;
			storeItem2.transform.localScale = storeItem.transform.localScale;
			Guid id = skinItemType.Id;
			Guid id2 = characterItemType.Id;
			bool hasPilot = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id2);
			bool flag = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id);
			bool flag2 = flag;
			this.SetupAvaliableSkinBoughtInformation(storeItem2, flag2, id);
			int index = skinComponent.Index;
			storeItem2.gameObject.name = index.ToString("000") + "_" + skinItemType.Name;
			storeItem2.characterName.text = Language.Get(string.Format("{0}_name", skinItemType.Name), "Items");
			storeItem2.Index = index;
			this.UpdateStoreItemLockState(hasPilot, flag2, storeItem2);
			return storeItem2;
		}

		private void SetupAvaliableSkinBoughtInformation(StoreItem skin, bool isUnlocked, Guid itemTypeId)
		{
			bool flag = true;
			bool flag2 = true;
			skin.boughtGO.SetActive(isUnlocked);
			skin.unboughtGO.SetActive(!isUnlocked);
			if (isUnlocked)
			{
				skin.UnlockAnimation();
			}
			int num;
			int num2;
			GameHubBehaviour.Hub.Store.GetItemPrice(itemTypeId, out num, out num2, false);
			if (skin.softPrice)
			{
				skin.softPrice.text = string.Format("{0}", num);
			}
			skin.SoftPriceGroup.gameObject.SetActive(skin.StoreItemType.IsSoftPurchasable);
			if (skin.hardPrice)
			{
				skin.hardPrice.text = string.Format("{0}", num2);
			}
			skin.HardPriceGroup.SetActive(skin.StoreItemType.IsHardPurchasable);
			if (num > GameHubBehaviour.Hub.Store.SoftCurrency)
			{
				flag = false;
			}
			if ((long)num2 > GameHubBehaviour.Hub.Store.HardCurrency)
			{
				flag2 = false;
			}
			skin.softPrice.color = ((!flag) ? this.Shop.NoFundsColor : this.Shop.FameColor);
			skin.hardPrice.color = ((!flag2) ? this.Shop.NoFundsColor : this.Shop.CashColor);
			skin.softIcon.color = ((!flag) ? this.Shop.NoFundsColor : Color.white);
			skin.hardIcon.color = ((!flag2) ? this.Shop.NoFundsColor : Color.white);
		}

		public override void Show()
		{
			base.Show();
			this._itemSelected = false;
			this.ShowPage(base.CurrentPage);
			this.gridScript.Reposition();
			this.PageControllersGridPivot.Reposition();
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		public void ShowPage(int page)
		{
			base.CurrentPage = page;
			int num = page * 8;
			int num2 = page * 8 + 7;
			this.FilterItemList();
			base.UpdateTotalNumberofPages(Mathf.CeilToInt((float)this.CurrentItems.Count / 8f));
			for (int i = 0; i < this.CurrentItems.Count; i++)
			{
				if (i >= num && i <= num2)
				{
					this.CurrentItems[i].gameObject.SetActive(true);
					this.CurrentItems[i].LoadDynamicSprites();
				}
				else
				{
					GameObject gameObject = this.CurrentItems[i].gameObject;
					if (gameObject.activeSelf)
					{
						gameObject.SetActive(false);
					}
				}
			}
			this.gridScript.repositionNow = true;
			base.UpdatePageButtonControllers();
		}

		private void FilterItemList()
		{
			this.CurrentItems = this.AllItems;
		}

		public void GoPreviewsPage()
		{
			if (base.CurrentPage == 0)
			{
				return;
			}
			base.CurrentPage--;
			this.ShowPage(base.CurrentPage);
		}

		public void GoNextPage()
		{
			if (base.CurrentPage == base.TotalNumberofPages - 1)
			{
				return;
			}
			base.CurrentPage++;
			this.ShowPage(base.CurrentPage);
		}

		public void GoNextFilter()
		{
			base.CurrentPage = 0;
			if (this.CurrentFilter + 1 >= (ShopSkinsGUI.SkinFilter)this.TotalNumberOfFilters)
			{
				this.CurrentFilter = ShopSkinsGUI.SkinFilter.All;
			}
			else
			{
				this.CurrentFilter++;
			}
			this.ShowPage(base.CurrentPage);
			this.PageControllersGridPivot.Reposition();
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		public void GoPreviousFilter()
		{
			base.CurrentPage = 0;
			if (this.CurrentFilter - ShopSkinsGUI.SkinFilter.Bronze < 0)
			{
				this.CurrentFilter = (ShopSkinsGUI.SkinFilter)(this.TotalNumberOfFilters - 1);
			}
			else
			{
				this.CurrentFilter--;
			}
			this.ShowPage(base.CurrentPage);
			this.PageControllersGridPivot.Reposition();
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		private void LocalizeAndconfigureFilterTitleLabel()
		{
			switch (this.CurrentFilter)
			{
			case ShopSkinsGUI.SkinFilter.All:
				this.FilterTitle.text = Language.Get("shop_skin_page_filter_ALL", "Store");
				break;
			case ShopSkinsGUI.SkinFilter.Bronze:
				this.FilterTitle.text = Language.Get("shop_skin_page_filter_ALL_BRONZE", "Store");
				break;
			case ShopSkinsGUI.SkinFilter.Silver:
				this.FilterTitle.text = Language.Get("shop_skin_page_filter_ALL_PRATA", "Store");
				break;
			case ShopSkinsGUI.SkinFilter.Gold:
				this.FilterTitle.text = Language.Get("shop_skin_page_filter_ALL_OURO", "Store");
				break;
			}
		}

		public void GetFocusOnSkinDetailsScreen(UnityEngine.Object itemtype)
		{
			if (this._itemSelected)
			{
				return;
			}
			StoreItem component = ((GameObject)itemtype).GetComponent<StoreItem>();
			if (component.IsReadyToOpenLock())
			{
				component.UnlockAnimation();
				this.UpdateCharacterUnlock(component);
				return;
			}
			this._itemSelected = true;
			this.Hide();
			this.Shop.ShowSkinDetails(this.CurrentItemsById[component.StoreItemType.Id.ToString()]);
		}

		private void UpdateCharacterUnlock(StoreItem storeItem)
		{
			Guid id = storeItem.CharacterItemTypeScriptableObject.Id;
			ItemTypeScriptableObject storeItemType = storeItem.StoreItemType;
			SkinItemTypeBag skinItemTypeBag = (SkinItemTypeBag)((JsonSerializeable<T>)storeItemType.Bag);
			CharacterBag bag = GameHubBehaviour.Hub.User.SetCharacterBagItemUnlockSeen(id, skinItemTypeBag.UnlockLevel);
			CharacterCustomWS.UpdateCharacterUnlockMask(bag, new SwordfishClientApi.ParameterizedCallback<string>(this.OnUpdateCharacterUnlockMaskSuccess), new SwordfishClientApi.ErrorCallback(this.OnUpdateCharacterUnlockMaskError));
		}

		private void OnUpdateCharacterUnlockMaskError(object state, Exception exception)
		{
			Debug.LogError(string.Format("Error on OnUpdateCharacterUnlockMaskError. PlayerId: {0}, Error: {1}", GameHubBehaviour.Hub.User.PlayerSF.Id, exception.Message));
		}

		private void OnUpdateCharacterUnlockMaskSuccess(object state, string obj)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<T>)obj);
			if (!netResult.Success)
			{
				Debug.LogError(string.Format("Error on OnUpdateCharacterUnlockMaskSuccess. PlayerId: {0}, Error: {1}", GameHubBehaviour.Hub.User.PlayerSF.Id, netResult.Msg));
			}
		}

		private void UpdateStoreItemLockState(bool hasPilot, bool isItemBoughtOrFree, StoreItem storeItem)
		{
			if (isItemBoughtOrFree)
			{
				storeItem.SetLocked(true, true, 0);
				return;
			}
			CharacterBag characterBag = GameHubBehaviour.Hub.User.GetCharacterBag(storeItem.CharacterItemTypeScriptableObject.Id);
			ItemTypeScriptableObject storeItemType = storeItem.StoreItemType;
			SkinItemTypeBag skinItemTypeBag = (SkinItemTypeBag)((JsonSerializeable<T>)storeItemType.Bag);
			bool canBuySkin = hasPilot && skinItemTypeBag.UnlockLevel == 0;
			bool isUnlockSeen = true;
			if (characterBag != null)
			{
				PlayerBag playerBag = (PlayerBag)GameHubBehaviour.Hub.User.PlayerSF.Bag;
				ProgressionInfo.CanBuyReason canBuyReason;
				string text;
				canBuySkin = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.CanBuySkin(hasPilot, playerBag, skinItemTypeBag, GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetLevelForXP(characterBag.Xp), (CharacterItemTypeBag)((JsonSerializeable<T>)storeItem.CharacterItemTypeScriptableObject.Bag), out canBuyReason, out text);
				isUnlockSeen = (skinItemTypeBag.UnlockLevel == 0 || characterBag.GetUnlockSeen(skinItemTypeBag.UnlockLevel));
			}
			storeItem.SetLocked(canBuySkin, isUnlockSeen, GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetUnlockLevelForSkin(skinItemTypeBag.UnlockLevel));
		}

		[Header("INTERNAL")]
		public StoreItem skinItemPrefab;

		public UIGrid gridScript;

		public UILabel FilterTitle;

		[Header("ITEMS")]
		protected List<StoreItem> AllItems = new List<StoreItem>();

		protected List<StoreItem> CurrentItems = new List<StoreItem>();

		protected Dictionary<string, StoreItem> CurrentItemsById = new Dictionary<string, StoreItem>();

		public ShopSkinsGUI.SkinFilter CurrentFilter;

		public int TotalNumberOfFilters;

		private bool _itemSelected;

		private ShopSkinsGUI.MissionLastStepDoneDelegate _onMissionLastStepDoneCallback;

		private bool _disposeAllOnAlphaZero;

		[SerializeField]
		private ItemCategoryScriptableObject _characterItemTypeCategoryScriptableObject;

		[SerializeField]
		private ItemCategoryScriptableObject _skinItemTypeCategoryScriptableObject;

		[CompilerGenerated]
		private static Comparison<StoreItem> <>f__mg$cache0;

		public delegate void MissionLastStepDoneDelegate();

		public enum SkinFilter
		{
			All,
			Bronze,
			Silver,
			Gold
		}
	}
}
