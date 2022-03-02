using System;
using System.Diagnostics;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Store.Business.GetStoreItem;
using HeavyMetalMachines.Store.Business.ObserveStoreItem;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Obsolete]
	public class StoreItem : GameHubBehaviour
	{
		public ItemTypeScriptableObject StoreItemType
		{
			get
			{
				return this._storeItemType;
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
				if (this._isPurchasable == value)
				{
					return;
				}
				this._isPurchasable = value;
				if (this.IsPurchasableChanged != null)
				{
					this.IsPurchasableChanged(this, this._isPurchasable);
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<StoreItem, bool> IsPurchasableChanged;

		public ItemTypeScriptableObject CharacterItemType { get; set; }

		public void ToggleScroll(bool isEnabled)
		{
			UIDragScrollView uidragScrollView;
			if ((uidragScrollView = this._scrollView) == null)
			{
				uidragScrollView = (this._scrollView = this.button.GetComponent<UIDragScrollView>());
			}
			UIDragScrollView uidragScrollView2 = uidragScrollView;
			if (uidragScrollView2 != null)
			{
				uidragScrollView2.enabled = isEnabled;
			}
		}

		public void SetLocked(bool canBuySkin, bool isUnlockSeen, int level)
		{
			this._isReadyToOpenLock = false;
			this._canBuySkin = canBuySkin;
			this._isUnlockSeen = isUnlockSeen;
			if (this.UnlockGroupComponents.LevelLabel == null)
			{
				return;
			}
			this.UpdateUnlockGroupVisibility(canBuySkin, isUnlockSeen);
			this.UnlockGroupComponents.LevelLabel.text = (level + 1).ToString("0");
			this._isReadyToOpenLock = (canBuySkin && !isUnlockSeen);
			if (this._isReadyToOpenLock)
			{
				this.UnlockGroupComponents.UnlockSkinAnimation.Play();
			}
		}

		private void UpdateUnlockGroupVisibility(bool canBuySkin, bool isUnlockSeen)
		{
			bool active = !canBuySkin || !isUnlockSeen;
			this.UnlockGroupComponents.IconSprite.gameObject.SetActive(active);
			this.UnlockGroupComponents.LevelLabel.gameObject.SetActive(active);
			this.UnlockGroupComponents.ExplosionIconSprite.gameObject.SetActive(false);
		}

		public bool IsReadyToOpenLock()
		{
			return this._isReadyToOpenLock;
		}

		public void UnlockAnimation()
		{
			this.UnlockGroupComponents.IconSprite.gameObject.SetActive(false);
			this.UnlockGroupComponents.IconSpriteAura.gameObject.SetActive(false);
			this.UnlockGroupComponents.LevelLabel.gameObject.SetActive(false);
			this.UnlockGroupComponents.ExplosionIconSprite.gameObject.SetActive(true);
			this.UnlockGroupComponents.UnlockSkinAnimation.Stop();
			this.UnlockGroupComponents.ExplosionAnimation.Play();
			this._isReadyToOpenLock = false;
			this._canBuySkin = true;
			this._isUnlockSeen = true;
		}

		protected void OnEnable()
		{
			if (this.UnlockGroupComponents.IconSprite == null)
			{
				return;
			}
			this.UpdateUnlockGroupVisibility(this._canBuySkin, this._isUnlockSeen);
			if (this._isReadyToOpenLock)
			{
				this.UnlockGroupComponents.UnlockSkinAnimation.Play();
			}
		}

		protected void OnDisable()
		{
			if (this.UnlockGroupComponents.IconSprite == null)
			{
				return;
			}
			this.UnlockGroupComponents.IconSprite.gameObject.SetActive(false);
			this.UnlockGroupComponents.IconSpriteAura.gameObject.SetActive(false);
			this.UnlockGroupComponents.LevelLabel.gameObject.SetActive(false);
			this.UnlockGroupComponents.ExplosionIconSprite.gameObject.SetActive(false);
			this.UnlockGroupComponents.ExplosionAnimation.Stop();
			this.UnlockGroupComponents.UnlockSkinAnimation.Stop();
		}

		public void Setup(ItemTypeScriptableObject storeItemType, IStoreBusinessFactory storeBusinessFactory)
		{
			this._storeItemType = storeItemType;
			this._getStoreItem = storeBusinessFactory.CreateGetStoreItem();
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
			}
			IObserveStoreItem observeStoreItem = storeBusinessFactory.CreateObserveStoreItem();
			IObservable<StoreItem> observable = observeStoreItem.CreateObservable(storeItemType.Id);
			this._storeItemObservation = ObservableExtensions.Subscribe<StoreItem>(observable, new Action<StoreItem>(this.ConfigureControls));
			this.ConfigureControls(this._getStoreItem.Get(storeItemType.Id));
		}

		private void OnDestroy()
		{
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
			}
		}

		private void ConfigureControls(StoreItem storeItemBusiness)
		{
			this.IsPurchasable = storeItemBusiness.IsPurchasable;
			if (this.boughtGO == null || this.unboughtGO == null)
			{
				this.SetPrices(storeItemBusiness);
				return;
			}
			bool flag = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(this._storeItemType.Id);
			this.boughtGO.SetActive(flag);
			this.unboughtGO.SetActive(!flag);
			if (flag)
			{
				return;
			}
			this.SetPrices(storeItemBusiness);
		}

		private void SetPrices(StoreItem storeItemPrices)
		{
			if (this.softPrice != null)
			{
				this.softPrice.text = storeItemPrices.SoftPrice.ToString("0");
			}
			if (this.SoftPriceGroup != null)
			{
				this.SoftPriceGroup.SetActive(this._storeItemType.IsActive && storeItemPrices.IsSoftPurchasable);
			}
			if (this.hardPrice != null)
			{
				this.hardPrice.text = storeItemPrices.HardPrice.ToString("0");
			}
			if (this.HardPriceGroup != null)
			{
				this.HardPriceGroup.SetActive(this._storeItemType.IsActive && storeItemPrices.IsHardPurchasable);
			}
		}

		public void UpdateBoughtStatus()
		{
			StoreItem storeItemBusiness = this._getStoreItem.Get(this._storeItemType.Id);
			this.ConfigureControls(storeItemBusiness);
		}

		public GUIEventListener button;

		public HMMUI2DDynamicSprite carTexture;

		public UILabel categoryLabel;

		public UILabel descriptionLabel;

		public HMMUI2DDynamicSprite icon;

		public UILabel characterName;

		public GameObject boughtGO;

		public GameObject unboughtGO;

		public UILabel softPrice;

		public UILabel hardPrice;

		private ItemTypeScriptableObject _storeItemType;

		private bool _isPurchasable;

		[SerializeField]
		private GameObject HardPriceGroup;

		[SerializeField]
		private GameObject SoftPriceGroup;

		public UILabel itemName;

		public UIButton UnlockButton;

		public GameObject NameVariationGroupGameObject;

		public UILabel NameVariationLabel;

		public UILabel RarityLabel;

		public UI2DSprite BorderSprite;

		public int Index;

		[Header("[Unlock Group]")]
		[SerializeField]
		protected StoreItem.UnlockGroup UnlockGroupComponents;

		private IGetStoreItem _getStoreItem;

		private IDisposable _storeItemObservation;

		private UIDragScrollView _scrollView;

		private bool _isReadyToOpenLock;

		private bool _canBuySkin;

		private bool _isUnlockSeen;

		[Serializable]
		public struct UnlockGroup
		{
			public Animation UnlockSkinAnimation;

			public Animation ExplosionAnimation;

			public UILabel LevelLabel;

			public UI2DSprite IconSprite;

			public UI2DSprite IconSpriteAura;

			public UI2DSprite ExplosionIconSprite;
		}
	}
}
