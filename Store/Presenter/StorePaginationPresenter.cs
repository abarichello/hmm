using System;
using HeavyMetalMachines.Store.Tabs.Presenter;
using HeavyMetalMachines.Store.View;
using Hoplon.Input.UiNavigation.AxisSelector;
using UniRx;

namespace HeavyMetalMachines.Store.Presenter
{
	public class StorePaginationPresenter : IStorePaginationPresenter
	{
		public StorePaginationPresenter(IStorePaginationView paginationView)
		{
			this._paginationView = paginationView;
		}

		public void InitializeTabPagination(IStoreTabPresenter tabPresenter)
		{
			this._currentTabPresenter = tabPresenter;
			this._totalNumberOfPages = this._currentTabPresenter.GetPageCount();
			if (this._totalNumberOfPages <= 1)
			{
				this.DisablePagination();
				return;
			}
			this.DisposeAndCreateNewCompositeDisposable();
			this.InitializeAxisSelector();
			this.InitializeView();
			this.InitializePageButtons();
			this._paginationView.RefreshPageToggles();
		}

		private void DisposeAndCreateNewCompositeDisposable()
		{
			if (this._compositeDisposable != null)
			{
				this._compositeDisposable.Dispose();
			}
			this._compositeDisposable = new CompositeDisposable();
		}

		private void InitializeAxisSelector()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<AxisSelectorEdge>(this._currentTabPresenter.ObserveOnAxisSelectorEdgeReached(), new Action<AxisSelectorEdge>(this.OnAxisSelectorEdgeReached));
			this._compositeDisposable.Add(disposable);
		}

		private void OnAxisSelectorEdgeReached(AxisSelectorEdge axisSelectorEdge)
		{
			if (axisSelectorEdge == 2)
			{
				this.TryToGoToPreviousPageByEdgeReached();
			}
			else if (axisSelectorEdge == 3)
			{
				this.TryToGoToNextPageByEdgeReached();
			}
		}

		private void InitializeView()
		{
			if (this._paginationView.PageToggles.Count >= this._totalNumberOfPages)
			{
				return;
			}
			int numberOfPages = this._totalNumberOfPages - this._paginationView.PageToggles.Count;
			this._paginationView.CreatePageToogles(numberOfPages);
		}

		private void InitializePageButtons()
		{
			this.InitializeToggleButtons();
			this.InitializeSideButtons();
			this.UpdatePageChange();
		}

		private void InitializeSideButtons()
		{
			this._paginationView.NextPageButton.SetActive(true);
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._paginationView.NextPageButton.OnClick(), delegate(Unit _)
			{
				this.GoToNextPage();
			}));
			this._compositeDisposable.Add(disposable);
			this._paginationView.PreviousPageButton.SetActive(true);
			disposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._paginationView.PreviousPageButton.OnClick(), delegate(Unit _)
			{
				this.GoToPreviousPage();
			}));
			this._compositeDisposable.Add(disposable);
		}

		private void GoToNextPage()
		{
			this._currentTabPresenter.GoToNextPage();
			this.UpdatePageChange();
		}

		private void GoToPreviousPage()
		{
			this._currentTabPresenter.GoToPreviousPage();
			this.UpdatePageChange();
		}

		private void TryToGoToNextPageByEdgeReached()
		{
			if (!this._paginationView.NextPageButton.IsInteractable)
			{
				return;
			}
			this._currentTabPresenter.GoToNextPageByEdgeReached();
			this.UpdatePageChange();
		}

		private void TryToGoToPreviousPageByEdgeReached()
		{
			if (!this._paginationView.PreviousPageButton.IsInteractable)
			{
				return;
			}
			this._currentTabPresenter.GoToPreviousPageByEdgeReached();
			this.UpdatePageChange();
		}

		private void InitializeToggleButtons()
		{
			for (int i = 0; i < this._totalNumberOfPages; i++)
			{
				IPageButtonToggle pageButton = this._paginationView.PageToggles[i];
				pageButton.PageIndex = i;
				int num = 1 + i;
				pageButton.PageLabel.Text = num.ToString();
				IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(pageButton.PageButton.OnClick(), delegate(Unit _)
				{
					this._currentTabPresenter.ShowPage(pageButton.PageIndex);
				}), delegate(Unit _)
				{
					this.UpdatePageChange();
				}));
				this._compositeDisposable.Add(disposable);
				pageButton.SetActive(true);
			}
			for (int j = this._totalNumberOfPages; j < this._paginationView.PageToggles.Count; j++)
			{
				IPageButtonToggle pageButtonToggle = this._paginationView.PageToggles[j];
				pageButtonToggle.SetActive(false);
			}
		}

		public void DisablePagination()
		{
			for (int i = 0; i < this._paginationView.PageToggles.Count; i++)
			{
				IPageButtonToggle pageButtonToggle = this._paginationView.PageToggles[i];
				pageButtonToggle.SetActive(false);
			}
			this._paginationView.NextPageButton.SetActive(false);
			this._paginationView.PreviousPageButton.SetActive(false);
			if (this._compositeDisposable != null)
			{
				this._compositeDisposable.Dispose();
				this._compositeDisposable = null;
			}
		}

		private void UpdatePageChange()
		{
			int currentPage = this._currentTabPresenter.CurrentPage;
			for (int i = 0; i < this._paginationView.PageToggles.Count; i++)
			{
				IPageButtonToggle pageButtonToggle = this._paginationView.PageToggles[i];
				pageButtonToggle.Toggle.IsOn = false;
				pageButtonToggle.Toggle.IsInteractable = true;
			}
			this._paginationView.PageToggles[currentPage].Toggle.IsOn = true;
			this._paginationView.PageToggles[currentPage].Toggle.IsInteractable = false;
			this._paginationView.NextPageButton.IsInteractable = (currentPage < this._totalNumberOfPages - 1);
			this._paginationView.PreviousPageButton.IsInteractable = (currentPage > 0);
		}

		private IStoreTabPresenter _currentTabPresenter;

		private readonly IStorePaginationView _paginationView;

		private int _totalNumberOfPages;

		private CompositeDisposable _compositeDisposable;
	}
}
