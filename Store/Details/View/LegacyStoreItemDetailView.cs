using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Store.Details.View
{
	public class LegacyStoreItemDetailView : MonoBehaviour, IStoreItemDetailView
	{
		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IObservable<Unit> ShowDriver(IItemType driverItemType)
		{
			this._driverSkinShopDetails.ShowDriverDetails(driverItemType);
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> ShowSkin(IItemType driverItemType)
		{
			this._driverSkinShopDetails.ShowSkinDetails(driverItemType);
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> ShowGeneric(IItemType itemType)
		{
			this._genericItemDetails.Show(itemType);
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> Hide()
		{
			this._driverSkinShopDetails.Hide();
			this._genericItemDetails.Hide();
			return Observable.ReturnUnit();
		}

		private void Awake()
		{
			this._viewProvider.Bind<IStoreItemDetailView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IStoreItemDetailView>(null);
		}

		[SerializeField]
		private NGuiButton _backButton;

		[SerializeField]
		private ShopDetails _driverSkinShopDetails;

		[SerializeField]
		private ItemTypeShopDetails _genericItemDetails;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
