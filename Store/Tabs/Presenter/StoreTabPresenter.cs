using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Store.Business.Filter;
using HeavyMetalMachines.Store.Business.GetStoreItem;
using HeavyMetalMachines.Store.Business.ObserveStoreItem;
using HeavyMetalMachines.Store.Presenter;
using HeavyMetalMachines.Store.Tabs.View;
using HeavyMetalMachines.Store.View;
using Hoplon.Input.UiNavigation.AxisSelector;
using Hoplon.Math;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Store.Tabs.Presenter
{
	public class StoreTabPresenter : IStoreTabPresenter, IPresenter, IStoreFilterChangeReceiver
	{
		public StoreTabPresenter(Guid[] itemCategoryGuids, IStoreTabView storeTabView, ICollectionScriptableObject collection, IStoreBusinessFactory storeBusinessFactory, StoreFilterType startFilterType, IStorePaginationPresenter storePaginationPresenter)
		{
			this._itemCategoryGuids = itemCategoryGuids;
			this._storeTabView = storeTabView;
			this._collection = collection;
			this._itemTypes = new List<IItemType>();
			this._filteredItemTypes = new List<IItemType>();
			this._activeStorePresenters = new List<IStoreItemPresenter>();
			this._disposableStoreItemObservations = new List<IDisposable>();
			this._storeBusinessFactory = storeBusinessFactory;
			this._currentFilterType = startFilterType;
			this._storePaginationPresenter = storePaginationPresenter;
			this._getStoreItem = this._storeBusinessFactory.CreateGetStoreItem();
			this._storeFilterMapper = this._storeBusinessFactory.GetStoreFilterMapper(this._getStoreItem);
		}

		public IObservable<IItemType> OnClick
		{
			get
			{
				return this._onClick;
			}
		}

		public int CurrentPage
		{
			get
			{
				return this._currentPage;
			}
		}

		public IObservable<AxisSelectorEdge> ObserveOnAxisSelectorEdgeReached()
		{
			return this._storeTabView.UiNavigationAxisSelector.ObserveOnEdgeReached();
		}

		public IObservable<Unit> Initialize()
		{
			this._currentPage = 0;
			this.PopulateItemTypeList();
			this.SortItemTypesByCurrentFilter();
			this._onClick = new Subject<IItemType>();
			this.InitializeButtonClicks();
			return Observable.ReturnUnit();
		}

		private void InitializeButtonClicks()
		{
			if (this._viewClickDisposables != null)
			{
				this._viewClickDisposables.Dispose();
			}
			this._viewClickDisposables = new CompositeDisposable();
			List<IStoreItemView> storeItems = this._storeTabView.StoreItems;
			for (int i = 0; i < storeItems.Count; i++)
			{
				IStoreItemView storeItemView = storeItems[i];
				int index = i;
				IDisposable disposable = ObservableExtensions.Subscribe<Unit>(storeItemView.Button.OnClick(), delegate(Unit _)
				{
					this.GoToDetails(index);
				});
				this._viewClickDisposables.Add(disposable);
			}
		}

		private void GoToDetails(int activeStorePresenterIndex)
		{
			IItemType itemType = this._activeStorePresenters[activeStorePresenterIndex].ItemType;
			int index = this._itemTypes.IndexOf(itemType);
			this._onClick.OnNext(this._itemTypes[index]);
		}

		private void PopulateItemTypeList()
		{
			this._itemTypes.Clear();
			for (int i = 0; i < this._itemCategoryGuids.Length; i++)
			{
				Guid categoryGuid = this._itemCategoryGuids[i];
				List<IItemType> itemsFromCategoryId = this._collection.GetItemsFromCategoryId(categoryGuid);
				for (int j = 0; j < itemsFromCategoryId.Count; j++)
				{
					IItemType itemType = itemsFromCategoryId[j];
					if (itemType.IsItemEnableInShop())
					{
						StoreItem storeItem = this._getStoreItem.Get(itemType.Id);
						if (storeItem.IsPurchasable)
						{
							this._itemTypes.Add(itemType);
						}
					}
				}
			}
		}

		public IObservable<Unit> Show()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._storeTabView.Show();
				this._storeTabView.UiNavigationSubGroupHolder.SubGroupFocusGet();
				this.ShowPage(this._currentPage);
				this.InitializeButtonClicks();
				this._storeTabView.UiNavigationAxisSelector.RebuildAndSelect();
			});
		}

		public IObservable<Unit> Hide()
		{
			if (this._viewClickDisposables != null)
			{
				this._viewClickDisposables.Dispose();
				this._viewClickDisposables = null;
			}
			this.ClearStoreItemReferences();
			this._storeTabView.Hide();
			this._storeTabView.UiNavigationSubGroupHolder.SubGroupFocusRelease();
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				if (this._onClick != null)
				{
					this._onClick.Dispose();
				}
			});
		}

		public IObservable<Unit> ObserveHide()
		{
			return Observable.ReturnUnit();
		}

		public void ShowPage(int page)
		{
			this._currentPage = page;
			List<IStoreItemView> storeItems = this._storeTabView.StoreItems;
			this.ClearStoreItemReferences();
			this.ActivateCurrentItems(page, storeItems);
			this.DisableUnusedItens(this._activeStorePresenters.Count, storeItems);
			this._storeTabView.Reposition();
		}

		private void ActivateCurrentItems(int page, List<IStoreItemView> storeItems)
		{
			int count = storeItems.Count;
			int num = page * count;
			while (num < this._filteredItemTypes.Count && this._activeStorePresenters.Count < count)
			{
				IStoreItemView view = storeItems[this._activeStorePresenters.Count];
				IItemType itemType = this._filteredItemTypes[num];
				IObserveStoreItem observeStoreItem = this._storeBusinessFactory.CreateObserveStoreItem();
				IObservable<StoreItem> observable = observeStoreItem.CreateObservable(itemType.Id);
				IDisposable item = ObservableExtensions.Subscribe<StoreItem>(observable, new Action<StoreItem>(this.StoreItemChanged));
				this._disposableStoreItemObservations.Add(item);
				IStoreItemPresenter storeItemPresenter = this._storeBusinessFactory.CreateGetStoreItemPresenter(view);
				storeItemPresenter.Setup(itemType);
				storeItemPresenter.Show();
				this._activeStorePresenters.Add(storeItemPresenter);
				num++;
			}
		}

		private int GetActiveItensCount()
		{
			int num = 0;
			for (int i = 0; i < this._filteredItemTypes.Count; i++)
			{
				IItemType itemType = this._filteredItemTypes[i];
				if (itemType.IsItemEnableInShop())
				{
					StoreItem storeItem = this._storeBusinessFactory.CreateGetStoreItem().Get(itemType.Id);
					if (storeItem.IsPurchasable)
					{
						num++;
					}
				}
			}
			return num;
		}

		private void DisableUnusedItens(int currentPresenterCount, List<IStoreItemView> storeItems)
		{
			for (int i = currentPresenterCount; i < storeItems.Count; i++)
			{
				IStoreItemView storeItemView = storeItems[i];
				storeItemView.Hide();
			}
		}

		private void StoreItemChanged(StoreItem storeItemBusiness)
		{
			this.ShowPage(this._currentPage);
		}

		public void GoToNextPage()
		{
			this.ShowPage((this._currentPage + 1) % this.GetPageCount());
		}

		public void GoToPreviousPage()
		{
			this._currentPage--;
			int pageCount = this.GetPageCount();
			if (this._currentPage < 0)
			{
				this._currentPage = pageCount - 1;
			}
			else
			{
				this._currentPage %= pageCount;
			}
			this.ShowPage(this._currentPage);
		}

		public void GoToNextPageByEdgeReached()
		{
			this.GoToNextPage();
			this._storeTabView.UiNavigationAxisSelector.Navigate(Vector2.right, true);
		}

		public void GoToPreviousPageByEdgeReached()
		{
			this.GoToPreviousPage();
			this._storeTabView.UiNavigationAxisSelector.Navigate(Vector2.left, true);
		}

		public int GetPageCount()
		{
			int activeItensCount = this.GetActiveItensCount();
			List<IStoreItemView> storeItems = this._storeTabView.StoreItems;
			int count = storeItems.Count;
			return Mathf.CeilToInt((float)activeItensCount / (float)count);
		}

		private void ClearStoreItemReferences()
		{
			for (int i = 0; i < this._disposableStoreItemObservations.Count; i++)
			{
				this._disposableStoreItemObservations[i].Dispose();
			}
			this._disposableStoreItemObservations.Clear();
			for (int j = 0; j < this._activeStorePresenters.Count; j++)
			{
				IStoreItemPresenter storeItemPresenter = this._activeStorePresenters[j];
				storeItemPresenter.Dispose();
			}
			this._activeStorePresenters.Clear();
		}

		public StoreFilterType GetCurrentFilter()
		{
			return this._currentFilterType;
		}

		public void OnFilterChange(StoreFilterType filterType)
		{
			bool flag = this._currentFilterType != filterType;
			this._currentFilterType = filterType;
			this.SortItemTypesByCurrentFilter();
			if (flag)
			{
				if (this._filteredItemTypes.Count != this._itemTypes.Count)
				{
					this._currentPage = 0;
				}
				this._storePaginationPresenter.InitializeTabPagination(this);
			}
			this.ShowPage(this._currentPage);
		}

		private void SortItemTypesByCurrentFilter()
		{
			this._filteredItemTypes = new List<IItemType>(this._itemTypes);
			IStoreFilterSorter storeFilterSorter = this._storeFilterMapper.Resolve(this._currentFilterType);
			storeFilterSorter.Sort(this._filteredItemTypes);
		}

		private readonly Guid[] _itemCategoryGuids;

		private readonly IStoreTabView _storeTabView;

		private readonly List<IItemType> _itemTypes;

		private List<IItemType> _filteredItemTypes;

		private readonly ICollectionScriptableObject _collection;

		private readonly List<IStoreItemPresenter> _activeStorePresenters;

		private readonly List<IDisposable> _disposableStoreItemObservations;

		private int _currentPage = -1;

		private Subject<IItemType> _onClick;

		private readonly IStoreBusinessFactory _storeBusinessFactory;

		private readonly IStoreFilterSorterMapper _storeFilterMapper;

		private CompositeDisposable _viewClickDisposables;

		private readonly IGetStoreItem _getStoreItem;

		private StoreFilterType _currentFilterType;

		private readonly IStorePaginationPresenter _storePaginationPresenter;
	}
}
