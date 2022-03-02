using System;
using System.Diagnostics;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Store.Business.GetStoreItem;
using HeavyMetalMachines.Store.Business.ObserveStoreItem;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassPremiumShopItem : MonoBehaviour, IBattlepassPremiumShopItem
	{
		public bool IsPurchasable
		{
			get
			{
				return this._isPurchasable;
			}
			private set
			{
				if (this._isPurchasable == value)
				{
					return;
				}
				this._isPurchasable = value;
				if (this.IsPurchasableChanged != null)
				{
					this.IsPurchasableChanged(this._isPurchasable);
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> IsPurchasableChanged;

		public void Setup(ItemTypeScriptableObject package)
		{
			PremiumItemTypeComponent component = package.GetComponent<PremiumItemTypeComponent>();
			this._title.text = Language.GetFormatted(component.TitleDraft, TranslationContext.Battlepass, new object[]
			{
				component.LevelCount
			});
			IGetStoreItem getStoreItem = this._storeBusinessFactory.CreateGetStoreItem();
			StoreItem price = getStoreItem.Get(package.Id);
			this.SetPrice(price);
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
			}
			IObserveStoreItem observeStoreItem = this._storeBusinessFactory.CreateObserveStoreItem();
			IObservable<StoreItem> observable = observeStoreItem.CreateObservable(package.Id);
			this._storeItemObservation = ObservableExtensions.Subscribe<StoreItem>(observable, new Action<StoreItem>(this.SetPrice));
		}

		private void SetPrice(StoreItem storeItem)
		{
			this.IsPurchasable = storeItem.IsPurchasable;
			this._value.text = storeItem.HardPrice.ToString();
		}

		private void OnDestroy()
		{
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
			}
		}

		[SerializeField]
		private Text _title;

		[SerializeField]
		private Text _value;

		[InjectOnClient]
		private IStoreBusinessFactory _storeBusinessFactory;

		private IDisposable _storeItemObservation;

		private bool _isPurchasable;
	}
}
