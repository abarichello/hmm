using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Customization.Skins;
using HeavyMetalMachines.Customizations.Skins;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using Hoplon.Serialization;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ItemTypeShopSkinScreen : ItemTypeShopScreen
	{
		private void ConfigureSkinsFilters()
		{
			this._filterList = new List<TierKind>();
			this._filterList.Add(0);
			for (int i = 0; i < this._storeItems.Length; i++)
			{
				StoreItem storeItem = this._storeItems[i];
				SkinPrefabItemTypeComponent component = storeItem.StoreItemType.GetComponent<SkinPrefabItemTypeComponent>();
				TierKind tier = component.Tier;
				if (tier != 1)
				{
					if (!this._filterList.Contains(tier))
					{
						this._filterList.Add(tier);
					}
				}
			}
			this._filterList.Sort();
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		protected override void ConfigurePages()
		{
			this.ConfigureSkinsFilters();
			base.ConfigurePages();
		}

		protected override int Comparison(StoreItem thisStoreItem, StoreItem otherStoreItem)
		{
			CharacterItemTypeComponent component = thisStoreItem.CharacterItemType.GetComponent<CharacterItemTypeComponent>();
			CharacterItemTypeComponent component2 = otherStoreItem.CharacterItemType.GetComponent<CharacterItemTypeComponent>();
			string characterLocalizedName = component.GetCharacterLocalizedName();
			string characterLocalizedName2 = component2.GetCharacterLocalizedName();
			int num = characterLocalizedName.CompareTo(characterLocalizedName2);
			if (num == 0)
			{
				num = thisStoreItem.StoreItemType.name.CompareTo(otherStoreItem.StoreItemType.name);
			}
			return num;
		}

		protected override bool IsFiltered(int index)
		{
			if (this._filterList[this._currentFilterIndex] == null)
			{
				return true;
			}
			StoreItem storeItem = this._storeItems[index];
			SkinPrefabItemTypeComponent component = storeItem.StoreItemType.GetComponent<SkinPrefabItemTypeComponent>();
			return this._filterList[this._currentFilterIndex] == component.Tier;
		}

		public override void OnStoreItemClick(StoreItem item)
		{
			if (this.IsVisible())
			{
				this.Hide();
				this.Shop.ShowSkinDetails(item.StoreItemType);
			}
		}

		protected override void SetupItem(StoreItem storeItem, ItemTypeScriptableObject skinItemType)
		{
			storeItem.name = skinItemType.Name;
			storeItem.Setup(skinItemType, this._storeBusinessFactory);
			storeItem.IsPurchasableChanged += delegate(StoreItem sender, bool isPurchasable)
			{
				this._shouldReconfigurePages = true;
			};
			ShopItemTypeComponent component = skinItemType.GetComponent<ShopItemTypeComponent>();
			SkinPrefabItemTypeComponent component2 = skinItemType.GetComponent<SkinPrefabItemTypeComponent>();
			if (component == null)
			{
				return;
			}
			SkinItemTypeBag skinItemTypeBag = (SkinItemTypeBag)((JsonSerializeable<!0>)skinItemType.Bag);
			ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[skinItemTypeBag.CharacterItemTypeId];
			CharacterItemTypeComponent component3 = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
			storeItem.CharacterItemType = itemTypeScriptableObject;
			storeItem.carTexture.SpriteName = component2.SkinSpriteName;
			storeItem.characterName.text = Language.Get(component2.CardSkinDraft, TranslationContext.Items);
			if (storeItem.categoryLabel != null)
			{
				ItemCategoryScriptableObject categoryById = GameHubBehaviour.Hub.InventoryColletion.GetCategoryById(skinItemType.ItemCategoryId);
				storeItem.categoryLabel.text = categoryById.LocalizedName;
			}
			if (storeItem.descriptionLabel != null)
			{
				storeItem.descriptionLabel.text = Language.Get(component.DescriptionDraft, TranslationContext.Items);
			}
			if (storeItem.icon != null)
			{
				storeItem.icon.SpriteName = component3.Round128LookRightIconName;
			}
			storeItem.name = string.Format("{0}_{1}", component3.GetCharacterLocalizedName(), skinItemType.Name);
			if (storeItem.NameVariationGroupGameObject != null)
			{
				storeItem.NameVariationGroupGameObject.SetActive(false);
				if (!string.IsNullOrEmpty(component2.SkinNameVariationDraft))
				{
					storeItem.NameVariationLabel.text = Language.Get(component2.SkinNameVariationDraft, TranslationContext.Inventory);
					storeItem.NameVariationGroupGameObject.SetActive(true);
				}
			}
			if (storeItem.RarityLabel != null)
			{
				string rarityText = this.GetRarityText(component2.Tier);
				if (string.IsNullOrEmpty(rarityText))
				{
					storeItem.RarityLabel.gameObject.SetActive(false);
				}
				else
				{
					storeItem.RarityLabel.gameObject.SetActive(true);
					storeItem.RarityLabel.text = rarityText;
					storeItem.RarityLabel.color = this.GetRarityColor(component2.Tier);
				}
			}
			if (storeItem.BorderSprite != null)
			{
				storeItem.BorderSprite.sprite2D = this.GetRarityBorderSprite(component2.Tier);
				storeItem.BorderSprite.MakePixelPerfect();
			}
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
			case 0:
				this._filterTitle.text = Language.Get("shop_skin_page_filter_ALL", TranslationContext.Store);
				break;
			case 2:
				this._filterTitle.text = Language.Get("shop_skin_page_filter_ALL_BRONZE", TranslationContext.Store);
				break;
			case 3:
				this._filterTitle.text = Language.Get("shop_skin_page_filter_ALL_PRATA", TranslationContext.Store);
				break;
			case 4:
				this._filterTitle.text = Language.Get("shop_skin_page_filter_ALL_OURO", TranslationContext.Store);
				break;
			case 5:
				this._filterTitle.text = Language.Get("shop_skin_page_filter_ALL_HEAVYMETAL", TranslationContext.Store);
				break;
			}
		}

		private string GetRarityText(TierKind tier)
		{
			SkinRarityInfo skinRarityInfo;
			if (this._skinRarityProvider.TryGetSkinRarityInfo(tier, out skinRarityInfo))
			{
				return Language.Get(skinRarityInfo.ShortDraftName, TranslationContext.Store);
			}
			return string.Empty;
		}

		private Color GetRarityColor(TierKind tier)
		{
			SkinRarityInfo skinRarityInfo;
			if (this._skinRarityProvider.TryGetSkinRarityInfo(tier, out skinRarityInfo))
			{
				return skinRarityInfo.TierColor;
			}
			return Color.white;
		}

		private Sprite GetRarityBorderSprite(TierKind tier)
		{
			SkinRarityInfo skinRarityInfo;
			if (this._skinRarityProvider.TryGetSkinRarityInfo(tier, out skinRarityInfo))
			{
				return skinRarityInfo.TierBorderSprite;
			}
			return null;
		}

		private int _currentFilterIndex;

		[SerializeField]
		private UILabel _filterTitle;

		[InjectOnClient]
		private ISkinRarityProvider _skinRarityProvider;

		private List<TierKind> _filterList;
	}
}
