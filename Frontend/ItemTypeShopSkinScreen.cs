using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ItemTypeShopSkinScreen : ItemTypeShopScreen
	{
		private void ConfigureSkinsFilters()
		{
			this._filterList = new List<SkinPrefabItemTypeComponent.TierKind>();
			this._filterList.Add(SkinPrefabItemTypeComponent.TierKind.None);
			for (int i = 0; i < this._storeItems.Length; i++)
			{
				StoreItem storeItem = this._storeItems[i];
				SkinPrefabItemTypeComponent component = storeItem.StoreItemType.GetComponent<SkinPrefabItemTypeComponent>();
				SkinPrefabItemTypeComponent.TierKind tier = component.Tier;
				if (tier != SkinPrefabItemTypeComponent.TierKind.Default)
				{
					if (!this._filterList.Contains(tier))
					{
						this._filterList.Add(tier);
					}
				}
			}
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		protected override void ConfigurePages()
		{
			this.ConfigureSkinsFilters();
			base.ConfigurePages();
		}

		protected override bool IsFiltered(int index)
		{
			if (this._filterList[this._currentFilterIndex] == SkinPrefabItemTypeComponent.TierKind.None)
			{
				return true;
			}
			StoreItem storeItem = this._storeItems[index];
			SkinPrefabItemTypeComponent component = storeItem.StoreItemType.GetComponent<SkinPrefabItemTypeComponent>();
			return this._filterList[this._currentFilterIndex] == component.Tier;
		}

		public override void OnStoreItemClick(StoreItem item)
		{
			this.Hide();
			this.Shop.ShowSkinDetails(item);
		}

		protected override void SetupItem(StoreItem storeItem, ItemTypeScriptableObject skinItemType)
		{
			storeItem.name = skinItemType.Name;
			storeItem.StoreItemType = skinItemType;
			ShopItemTypeComponent component = skinItemType.GetComponent<ShopItemTypeComponent>();
			SkinPrefabItemTypeComponent component2 = skinItemType.GetComponent<SkinPrefabItemTypeComponent>();
			if (component == null)
			{
				return;
			}
			SkinItemTypeBag skinItemTypeBag = (SkinItemTypeBag)((JsonSerializeable<T>)skinItemType.Bag);
			ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[skinItemTypeBag.CharacterItemTypeId];
			storeItem.CharacterItemTypeScriptableObject = itemTypeScriptableObject;
			CharacterItemTypeComponent component3 = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
			storeItem.carTexture.SpriteName = component2.SkinSpriteName;
			storeItem.characterName.text = Language.Get(component2.CardSkinDraft, TranslationSheets.Items);
			if (storeItem.categoryLabel != null)
			{
				ItemCategoryScriptableObject categoryById = GameHubBehaviour.Hub.InventoryColletion.GetCategoryById(skinItemType.ItemCategoryId);
				storeItem.categoryLabel.text = categoryById.LocalizedName;
			}
			if (storeItem.descriptionLabel != null)
			{
				storeItem.descriptionLabel.text = Language.Get(component.DescriptionDraft, TranslationSheets.Items);
			}
			if (storeItem.icon != null)
			{
				storeItem.icon.SpriteName = component3.CharacterIcon128Name;
			}
			this.UpdateBoughtStatus(storeItem, skinItemType);
		}

		public void GoNextFilter()
		{
			base.CurrentPage = 0;
			if (this._currentFilterIndex == this._filterList.Count - 1)
			{
				this._currentFilterIndex = 0;
			}
			else
			{
				this._currentFilterIndex++;
			}
			base.ShowPage(base.CurrentPage);
			this.PageControllersGridPivot.Reposition();
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		public void GoPreviousFilter()
		{
			base.CurrentPage = 0;
			if (this._currentFilterIndex == 0)
			{
				this._currentFilterIndex = this._filterList.Count - 1;
			}
			else
			{
				this._currentFilterIndex--;
			}
			base.ShowPage(base.CurrentPage);
			this.PageControllersGridPivot.Reposition();
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		private void LocalizeAndconfigureFilterTitleLabel()
		{
			switch (this._filterList[this._currentFilterIndex])
			{
			case SkinPrefabItemTypeComponent.TierKind.None:
				this._filterTitle.text = Language.Get("shop_skin_page_filter_ALL", TranslationSheets.Store);
				break;
			case SkinPrefabItemTypeComponent.TierKind.Idol:
				this._filterTitle.text = Language.Get("shop_skin_page_filter_ALL_BRONZE", TranslationSheets.Store);
				break;
			case SkinPrefabItemTypeComponent.TierKind.Rockstar:
				this._filterTitle.text = Language.Get("shop_skin_page_filter_ALL_PRATA", TranslationSheets.Store);
				break;
			case SkinPrefabItemTypeComponent.TierKind.MetalLegend:
				this._filterTitle.text = Language.Get("shop_skin_page_filter_ALL_OURO", TranslationSheets.Store);
				break;
			case SkinPrefabItemTypeComponent.TierKind.HeavyMetal:
				this._filterTitle.text = Language.Get("shop_skin_page_filter_ALL_HEAVYMETAL", TranslationSheets.Store);
				break;
			}
		}

		private int _currentFilterIndex;

		[SerializeField]
		private UILabel _filterTitle;

		private List<SkinPrefabItemTypeComponent.TierKind> _filterList;
	}
}
