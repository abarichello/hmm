using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Battlepass.Business;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.Inventory.Business;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Localization.Business;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Store.Business.GetStoreItem;
using HeavyMetalMachines.Store.Business.ObserveStoreItem;
using HeavyMetalMachines.Store.Business.PlayerInventory;
using HeavyMetalMachines.Store.View;
using UniRx;

namespace HeavyMetalMachines.Store.Presenter
{
	public class StoreItemPresenter : IStoreItemPresenter
	{
		public StoreItemPresenter(IStoreBusinessFactory storeBusinessFactory, IStoreItemView view, IGetLocalizedCategoryName getLocalizedCategoryName, IGetCategoryIconName getCategoryIconName, IGetCharacterItemTypeFromSkinItemTypeId getCharacterItemTypeFromSkinItemTypeId)
		{
			this._getCategoryIconName = getCategoryIconName;
			this._getCharacterItemTypeFromSkinItemTypeId = getCharacterItemTypeFromSkinItemTypeId;
			this._getLocalizedCategoryName = getLocalizedCategoryName;
			this._view = view;
			this._getStoreItem = storeBusinessFactory.CreateGetStoreItem();
			this._getLocalUserHasItem = storeBusinessFactory.CreateGetLocalUserHasItem();
			this._observeStoreItem = storeBusinessFactory.CreateObserveStoreItem();
		}

		public IItemType ItemType
		{
			get
			{
				return this._storeItemType;
			}
		}

		public void Setup(IItemType storeItemType)
		{
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
			}
			this._storeItemType = storeItemType;
			IObservable<StoreItem> observable = this._observeStoreItem.CreateObservable(storeItemType.Id);
			this._storeItemObservation = ObservableExtensions.Subscribe<StoreItem>(observable, new Action<StoreItem>(this.ConfigureControls));
			this.ConfigureControls(this._getStoreItem.Get(storeItemType.Id));
			this.ConfigureVisuals();
		}

		private void ConfigureVisuals()
		{
			ShopItemTypeComponent component = this._storeItemType.GetComponent<ShopItemTypeComponent>();
			if (component == null)
			{
				return;
			}
			if (this._view.MainImage.HasValue)
			{
				this._view.MainImage.SetImageName(component.IconAssetName);
			}
			if (this._view.NameLabel.HasValue)
			{
				this._view.NameLabel.Text = Language.Get(component.TitleDraft, TranslationContext.Items);
			}
			if (this._view.CategoryLabel.HasValue)
			{
				this._view.CategoryLabel.Text = this._getLocalizedCategoryName.Get(this._storeItemType.ItemCategoryId);
			}
			if (this._view.CategoryIconImage.HasValue)
			{
				this._view.CategoryIconImage.SetImageName(this._getCategoryIconName.Get(this._storeItemType.ItemCategoryId));
			}
			if (this._view.CharacterIconImage.HasValue)
			{
				CharacterItemTypeComponent characterItemTypeComponent = this.GetCharacterItemTypeComponent(this._storeItemType);
				this._view.CharacterIconImage.SetImageName(characterItemTypeComponent.Round128LookRightIconName);
			}
			if (this._view.CharacterCarImage.HasValue)
			{
				CharacterItemTypeComponent characterItemTypeComponent2 = this.GetCharacterItemTypeComponent(this._storeItemType);
				this._view.CharacterCarImage.SetImageName(characterItemTypeComponent2.CarTextureName);
			}
			if (this._view.DescriptionLabel.HasValue)
			{
				this._view.DescriptionLabel.Text = Language.Get(component.DescriptionDraft, TranslationContext.Items);
			}
			if (this._view.SkinNameVariationLabel.HasValue)
			{
				SkinPrefabItemTypeComponent component2 = this._storeItemType.GetComponent<SkinPrefabItemTypeComponent>();
				if (component2 == null || string.IsNullOrEmpty(component2.SkinNameVariationDraft))
				{
					this._view.SetSkinNameVariation(false);
				}
				else
				{
					this._view.SkinNameVariationLabel.Text = Language.Get(component2.SkinNameVariationDraft, TranslationContext.Items);
					this._view.SetSkinNameVariation(true);
				}
			}
		}

		private CharacterItemTypeComponent GetCharacterItemTypeComponent(IItemType storeItemType)
		{
			CharacterItemTypeComponent component;
			if (this._storeItemType.ItemCategoryId == InventoryMapper.SkinsCategoryGuid)
			{
				IItemType fromSkinId = this._getCharacterItemTypeFromSkinItemTypeId.GetFromSkinId(this._storeItemType.Id);
				component = fromSkinId.GetComponent<CharacterItemTypeComponent>();
			}
			else
			{
				component = this._storeItemType.GetComponent<CharacterItemTypeComponent>();
			}
			return component;
		}

		public void Show()
		{
			this._view.Show();
		}

		public void Dispose()
		{
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
			}
		}

		private void ConfigureControls(StoreItem storeItemBusiness)
		{
			bool flag = this._getLocalUserHasItem.HasItemOfType(this._storeItemType.Id);
			this._view.SetHasItem(flag);
			if (flag)
			{
				this._view.SetSoftPurchasable(false);
				this._view.SetHardPurchasable(false);
				return;
			}
			this.SetPrices(storeItemBusiness);
		}

		private void SetPrices(StoreItem storeItemPrices)
		{
			if (this._view.SoftPriceLabel.HasValue)
			{
				this._view.SoftPriceLabel.Text = storeItemPrices.SoftPrice.ToString("0");
			}
			this._view.SetSoftPurchasable(this._storeItemType.IsActive && storeItemPrices.IsSoftPurchasable);
			if (this._view.HardPriceLabel.HasValue)
			{
				this._view.HardPriceLabel.Text = storeItemPrices.HardPrice.ToString("0");
			}
			this._view.SetHardPurchasable(this._storeItemType.IsActive && storeItemPrices.IsHardPurchasable);
		}

		private IItemType _storeItemType;

		private IDisposable _storeItemObservation;

		private readonly IStoreItemView _view;

		private readonly IGetStoreItem _getStoreItem;

		private readonly IGetLocalUserHasItem _getLocalUserHasItem;

		private readonly IObserveStoreItem _observeStoreItem;

		private readonly IGetLocalizedCategoryName _getLocalizedCategoryName;

		private readonly IGetCategoryIconName _getCategoryIconName;

		private readonly IGetCharacterItemTypeFromSkinItemTypeId _getCharacterItemTypeFromSkinItemTypeId;
	}
}
