using System;
using System.Diagnostics;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Store;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Store.Business.GetStoreItem;
using HeavyMetalMachines.Store.Business.ObserveStoreItem;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	[Obsolete]
	public class CharactersItem : GameHubBehaviour
	{
		public StoreItem StoreItem { get; private set; }

		public IItemType CharacterItemTypeScriptableObject
		{
			get
			{
				return this._characterItemTypeScriptableObject;
			}
		}

		public bool IsPurchasable
		{
			get
			{
				return this._isPurchasable;
			}
			private set
			{
				this._isPurchasable = value;
				if (this.IsPurchasableChanged != null)
				{
					this.IsPurchasableChanged(this._isPurchasable);
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> IsPurchasableChanged;

		public void Setup(IItemType characterItemType, IStoreBusinessFactory storeBusinessFactory, ILocalBalanceStorage localBalanceStorage, Color fameColor, Color cashColor, Color noFundsColor)
		{
			this._fameColor = fameColor;
			this._cashColor = cashColor;
			this._noFundsColor = noFundsColor;
			this._characterItemTypeScriptableObject = characterItemType;
			this._localBalanceStorage = localBalanceStorage;
			IGetStoreItem getStoreItem = storeBusinessFactory.CreateGetStoreItem();
			StoreItem storeItem = getStoreItem.Get(characterItemType.Id);
			this.SetStoreItem(storeItem);
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
			}
			IObserveStoreItem observeStoreItem = storeBusinessFactory.CreateObserveStoreItem();
			IObservable<StoreItem> observable = observeStoreItem.CreateObservable(characterItemType.Id);
			this._storeItemObservation = ObservableExtensions.Subscribe<StoreItem>(observable, new Action<StoreItem>(this.SetStoreItem));
		}

		private void SetStoreItem(StoreItem storeItem)
		{
			this.StoreItem = storeItem;
			this.IsPurchasable = storeItem.IsPurchasable;
			if (this.softPrice)
			{
				this.softPrice.text = string.Format("{0}", storeItem.SoftPrice);
			}
			if (this.hardPrice)
			{
				this.hardPrice.text = string.Format("{0}", storeItem.HardPrice);
			}
			bool flag = true;
			bool flag2 = true;
			if (storeItem.SoftPrice > (long)this._localBalanceStorage.SoftCurrency)
			{
				flag = false;
			}
			if (storeItem.HardPrice > this._localBalanceStorage.HardCurrency)
			{
				flag2 = false;
			}
			Guid id = this._characterItemTypeScriptableObject.Id;
			bool flag3 = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id);
			bool boolValue = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.AllCharactersFree, false);
			this.boughtGO.SetActive(flag3 || boolValue);
			this.unboughtGO.SetActive(!flag3 && !boolValue);
			this.softPrice.color = ((!flag) ? this._noFundsColor : this._fameColor);
			this.hardPrice.color = ((!flag2) ? this._noFundsColor : this._cashColor);
			this.softIcon.color = ((!flag) ? this._noFundsColor : Color.white);
			this.hardIcon.color = ((!flag2) ? this._noFundsColor : Color.white);
		}

		public static int CompareCharactersName(CharactersItem c1, CharactersItem c2)
		{
			return c1.gameObject.name.CompareTo(c2.gameObject.name);
		}

		public static int CompareInvertCharactersName(CharactersItem c1, CharactersItem c2)
		{
			return c2.gameObject.name.CompareTo(c1.gameObject.name);
		}

		public virtual void CleanUp()
		{
			this.carTexture.sprite2D = null;
			if (this.icon != null)
			{
				this.icon.sprite2D = null;
			}
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
				this._storeItemObservation = null;
			}
		}

		public void SetSpriteNames(string iconSpriteName, string carSpriteName)
		{
			this._iconSpriteName = iconSpriteName;
			this._carSpriteName = carSpriteName;
		}

		public void LoadDynamicSprites()
		{
			if (!string.IsNullOrEmpty(this._iconSpriteName) && this.icon != null)
			{
				this.icon.SpriteName = this._iconSpriteName;
			}
			if (!string.IsNullOrEmpty(this._carSpriteName))
			{
				this.carTexture.SpriteName = this._carSpriteName;
			}
		}

		private void OnDestroy()
		{
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
			}
		}

		[Inject]
		private ILocalBalanceStorage _localBalanceStorage;

		public GUIEventListener button;

		public HMMUI2DDynamicSprite icon;

		public HMMUI2DDynamicSprite carTexture;

		public UILabel characterName;

		public UILabel characterType;

		public UILabel softPrice;

		public UILabel hardPrice;

		public UI2DSprite softIcon;

		public UI2DSprite hardIcon;

		public GameObject boughtGO;

		public GameObject unboughtGO;

		public GameObject notPurchasableGO;

		public UILabel categoryLabel;

		public UILabel descriptionLabel;

		public UIButton previewButton;

		public UI2DSprite Border;

		private string _iconSpriteName;

		private string _carSpriteName;

		private IItemType _characterItemTypeScriptableObject;

		private bool _isPurchasable;

		private IDisposable _storeItemObservation;

		private Color _fameColor;

		private Color _cashColor;

		private Color _noFundsColor;
	}
}
