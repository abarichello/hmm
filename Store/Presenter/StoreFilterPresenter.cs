using System;
using HeavyMetalMachines.Localization.Business;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Store.Business.Filter;
using HeavyMetalMachines.Store.View;
using Hoplon.Input;
using UniRx;

namespace HeavyMetalMachines.Store.Presenter
{
	public class StoreFilterPresenter : IStoreFilterPresenter
	{
		public StoreFilterPresenter(IStoreFilterView storeFilterView, IGetLocalizedStoreSorterTitle getLocalizedStoreSorterTitle, IInputActiveDeviceChangeNotifier inputActiveDeviceChangeNotifier)
		{
			this._storeFilterView = storeFilterView;
			this._getLocalizedStoreSorterTitle = getLocalizedStoreSorterTitle;
			this._inputActiveDeviceChangeNotifier = inputActiveDeviceChangeNotifier;
		}

		public void DisableFilter()
		{
			ActivatableExtensions.Deactivate(this._storeFilterView.MainActivatable);
		}

		public void InitializeFilter(IStoreFilterContract storeFilterContract, IStoreFilterChangeReceiver storeFilterChangeReceiver)
		{
			this._currentStoreFilterContract = storeFilterContract;
			this._currentStoreFilterChangeReceiver = storeFilterChangeReceiver;
			this._currentFilterIndex = storeFilterContract.GetStoreFilterTypeIndex(storeFilterChangeReceiver.GetCurrentFilter());
			this.DisposeAndCreateNewCompositeDisposable();
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._storeFilterView.LeftButton.OnClick(), delegate(Unit _)
			{
				this.OnLeftButtonClick();
			});
			IDisposable disposable2 = ObservableExtensions.Subscribe<Unit>(this._storeFilterView.RightButton.OnClick(), delegate(Unit _)
			{
				this.OnRightButtonClick();
			});
			IDisposable disposable3 = ObservableExtensions.Subscribe<InputDevice>(this._inputActiveDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), new Action<InputDevice>(this.UpdateVisibilityOnDeviceChange));
			this._compositeDisposable.Add(disposable);
			this._compositeDisposable.Add(disposable2);
			this._compositeDisposable.Add(disposable3);
			this.UpdateCurrentFilterName();
			this.UpdateFilterButtonsInteractivity();
			ActivatableExtensions.Activate(this._storeFilterView.MainActivatable);
			this.NotifyFilterChange();
		}

		private void UpdateVisibilityOnDeviceChange(InputDevice inputDevice)
		{
			if (inputDevice == 3)
			{
				ActivatableExtensions.Deactivate(this._storeFilterView.ArrowLeftActivatable);
				ActivatableExtensions.Deactivate(this._storeFilterView.ArrowRightActivatable);
				ActivatableExtensions.Deactivate(this._storeFilterView.BorderActivatable);
				return;
			}
			ActivatableExtensions.Activate(this._storeFilterView.ArrowLeftActivatable);
			ActivatableExtensions.Activate(this._storeFilterView.ArrowRightActivatable);
			ActivatableExtensions.Activate(this._storeFilterView.BorderActivatable);
		}

		private void OnLeftButtonClick()
		{
			this.DecrementCurrentFilterIndex();
			this.UpdateCurrentFilterName();
			this.NotifyFilterChange();
			this.UpdateFilterButtonsInteractivity();
		}

		private void OnRightButtonClick()
		{
			this.IncrementCurrentFilterIndex();
			this.UpdateCurrentFilterName();
			this.NotifyFilterChange();
			this.UpdateFilterButtonsInteractivity();
		}

		private void IncrementCurrentFilterIndex()
		{
			this._currentFilterIndex++;
			if (this._currentStoreFilterContract.IsOverflow && this._currentFilterIndex >= this._currentStoreFilterContract.FilterTypes.Count)
			{
				this._currentFilterIndex = 0;
			}
		}

		private void DecrementCurrentFilterIndex()
		{
			this._currentFilterIndex--;
			if (this._currentStoreFilterContract.IsOverflow && this._currentFilterIndex < 0)
			{
				this._currentFilterIndex = this._currentStoreFilterContract.FilterTypes.Count - 1;
			}
		}

		private void NotifyFilterChange()
		{
			StoreFilterType filterType = this._currentStoreFilterContract.FilterTypes[this._currentFilterIndex];
			this._currentStoreFilterChangeReceiver.OnFilterChange(filterType);
		}

		private void UpdateFilterButtonsInteractivity()
		{
			if (this._currentStoreFilterContract.IsOverflow)
			{
				this._storeFilterView.LeftButton.IsInteractable = true;
				this._storeFilterView.RightButton.IsInteractable = true;
			}
			else
			{
				this._storeFilterView.LeftButton.IsInteractable = (this._currentFilterIndex > 0);
				int num = this._currentStoreFilterContract.FilterTypes.Count - 1;
				this._storeFilterView.RightButton.IsInteractable = (this._currentFilterIndex < num);
			}
		}

		private void DisposeAndCreateNewCompositeDisposable()
		{
			if (this._compositeDisposable != null)
			{
				this._compositeDisposable.Dispose();
			}
			this._compositeDisposable = new CompositeDisposable();
		}

		private void UpdateCurrentFilterName()
		{
			StoreFilterType storeFilterType = this._currentStoreFilterContract.FilterTypes[this._currentFilterIndex];
			this._storeFilterView.CurrentFilterLabel.Text = this._getLocalizedStoreSorterTitle.Get((int)storeFilterType);
		}

		private readonly IStoreFilterView _storeFilterView;

		private readonly IGetLocalizedStoreSorterTitle _getLocalizedStoreSorterTitle;

		private readonly IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		private CompositeDisposable _compositeDisposable;

		private IStoreFilterContract _currentStoreFilterContract;

		private int _currentFilterIndex;

		private IStoreFilterChangeReceiver _currentStoreFilterChangeReceiver;
	}
}
