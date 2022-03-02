using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.HACKS.Views;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.VFX.HACKS.Business;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.HACKS.Presenters
{
	public class InventoryInGameToolPresenter : GameHubObject, IInventoryInGameToolPresenter, IPresenter
	{
		public InventoryInGameToolPresenter(IViewProvider viewProvider, ICollectionScriptableObject collectionScriptableObject, ISetLocalPlayerCustomization setLocalPlayerCustomization)
		{
			this._setLocalPlayerCustomization = setLocalPlayerCustomization;
			this._viewProvider = viewProvider;
			this._collectionScriptableObject = (collectionScriptableObject as CollectionScriptableObject);
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this.Start();
				return Observable.ReturnUnit();
			});
		}

		public IObservable<Unit> Show()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._view.Canvas.Enable();
			});
		}

		private void Start()
		{
			this._togglesDisposables = new CompositeDisposable();
			this._currentItens = new List<ItemInventoryToolView>();
			this._view = this._viewProvider.Provide<IInventoryInGameToolView>(null);
			foreach (ItemCategoryScriptableObject itemCategoryScriptableObject in this._collectionScriptableObject.ItemCategories)
			{
				if (itemCategoryScriptableObject.Kind == 3)
				{
					this.CreateToggleFor(itemCategoryScriptableObject);
				}
			}
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.Repeat<Unit>(Observable.Do<Unit>(Observable.First<Unit>(this._view.EquipButton.OnClick()), delegate(Unit _)
			{
				this.EquipItem();
			})));
			this._togglesDisposables.Add(disposable);
		}

		private void CreateToggleFor(ItemCategoryScriptableObject category)
		{
			ToggleInventoryToolView toggleView = this._view.InstantiateToggleArmoryToolView();
			toggleView.SetIcon(category.IconName);
			toggleView.SetCategoryGuid(category.Id);
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.Repeat<Unit>(Observable.Do<Unit>(Observable.First<Unit>(toggleView.Button.OnToggleOn()), delegate(Unit _)
			{
				this.UpdateItemList(toggleView.CategoryGuid);
			})));
			this._togglesDisposables.Add(disposable);
		}

		public void UpdateItemList(Guid categoryId)
		{
			this.DisposeItensClick();
			List<IItemType> itemsFromCategoryId = this._collectionScriptableObject.GetItemsFromCategoryId(categoryId);
			int i = 0;
			int num = 0;
			while (num < this._currentItens.Count && i < itemsFromCategoryId.Count)
			{
				IItemType item = itemsFromCategoryId[i];
				if (this.IsValidInventoryItem(item))
				{
					this.SetItemView(this._currentItens[num], itemsFromCategoryId[i]);
					i++;
				}
				num++;
			}
			for (int j = i; j < this._currentItens.Count; j++)
			{
				this.DisableView(this._currentItens[j]);
			}
			while (i < itemsFromCategoryId.Count)
			{
				IItemType item2 = itemsFromCategoryId[i];
				if (this.IsValidInventoryItem(item2))
				{
					this.CreateItemFor(item2);
				}
				i++;
			}
			this._view.Rebuild();
			this.UpdateSelectedCategory(categoryId);
			this._selectedItemId = Guid.Empty;
			this._view.ResetItemToggles();
		}

		private void DisposeItensClick()
		{
			if (this._itemDisposables != null)
			{
				this._itemDisposables.Dispose();
			}
		}

		private void CreateItemFor(IItemType item)
		{
			ItemInventoryToolView itemInventoryToolView = this._view.InstantiateItemArmoryToolView();
			this.SetItemView(itemInventoryToolView, item);
			this._currentItens.Add(itemInventoryToolView);
		}

		private bool IsValidInventoryItem(IItemType item)
		{
			if (!this._collectionScriptableObject.Exists(item.Id))
			{
				return false;
			}
			ItemTypeScriptableObject itemTypeScriptableObject = this._collectionScriptableObject.AllItemTypes[item.Id];
			return itemTypeScriptableObject.ContainsComponent<InventoryItemTypeComponent>();
		}

		private void DisableView(ItemInventoryToolView view)
		{
			view.gameObject.SetActive(false);
		}

		private void SetItemView(ItemInventoryToolView view, IItemType item)
		{
			InventoryItemTypeComponent component = item.GetComponent<InventoryItemTypeComponent>();
			view.SetIcon(component.InventoryIconName);
			view.SetGuid(item.Id);
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.Repeat<Unit>(Observable.Do<Unit>(Observable.First<Unit>(view.Button.OnToggleOn()), delegate(Unit _)
			{
				this.SelectItem(view.ItemGuid);
			})));
			this._togglesDisposables.Add(disposable);
			view.gameObject.SetActive(true);
		}

		private void SelectItem(Guid selected)
		{
			ItemTypeScriptableObject itemTypeScriptableObject = this._collectionScriptableObject.AllItemTypes[selected];
			this._view.ItemLabel.Text = string.Format("{0} : {1}", itemTypeScriptableObject.name, itemTypeScriptableObject.Id);
			this._selectedItemId = selected;
		}

		private void UpdateSelectedCategory(Guid categoryId)
		{
			ItemCategoryScriptableObject categoryById = this._collectionScriptableObject.GetCategoryById(categoryId);
			this._view.CategoryLabel.Text = categoryById.name;
			this._equipableSlot = categoryById.CustomizationSlots[0];
		}

		private void EquipItem()
		{
			if (this._selectedItemId == Guid.Empty)
			{
				return;
			}
			this._setLocalPlayerCustomization.Set(this._equipableSlot, this._selectedItemId);
		}

		private void DisposeTogglesClick()
		{
			if (this._togglesDisposables != null)
			{
				this._togglesDisposables.Dispose();
			}
		}

		public IObservable<Unit> Hide()
		{
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this.DisposeTogglesClick();
				this.DisposeItensClick();
				return Observable.ReturnUnit();
			});
		}

		public IObservable<Unit> ObserveHide()
		{
			throw new NotImplementedException();
		}

		private readonly CollectionScriptableObject _collectionScriptableObject;

		private IInventoryInGameToolView _view;

		private CompositeDisposable _togglesDisposables;

		private CompositeDisposable _itemDisposables;

		private List<ItemInventoryToolView> _currentItens;

		private Guid _selectedItemId;

		private PlayerCustomizationSlot _equipableSlot;

		private readonly IViewProvider _viewProvider;

		private readonly ISetLocalPlayerCustomization _setLocalPlayerCustomization;
	}
}
