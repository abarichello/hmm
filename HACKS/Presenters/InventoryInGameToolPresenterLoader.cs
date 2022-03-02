using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.HACKS.Presenters
{
	public class InventoryInGameToolPresenterLoader : MonoBehaviour
	{
		private void Start()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(this._inventoryInGameToolPresenter.Initialize(), this._inventoryInGameToolPresenter.Show()));
		}

		private void OnDestroy()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(this._inventoryInGameToolPresenter.Hide(), this._inventoryInGameToolPresenter.Dispose()));
		}

		[Inject]
		private IInventoryInGameToolPresenter _inventoryInGameToolPresenter;
	}
}
