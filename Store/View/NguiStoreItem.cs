using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using UnityEngine;

namespace HeavyMetalMachines.Store.View
{
	public class NguiStoreItem : MonoBehaviour, IStoreItemView
	{
		public IButton Button
		{
			get
			{
				return this._button;
			}
		}

		public IDynamicImage MainImage
		{
			get
			{
				return this._mainImage;
			}
		}

		public IDynamicImage CategoryIconImage
		{
			get
			{
				return this._categoryIconImage;
			}
		}

		public IDynamicImage CharacterIconImage
		{
			get
			{
				return this._characterIconImage;
			}
		}

		public ILabel NameLabel
		{
			get
			{
				return this._nameLabel;
			}
		}

		public ILabel CategoryLabel
		{
			get
			{
				return this._categoryLabel;
			}
		}

		public ILabel DescriptionLabel
		{
			get
			{
				return this._descriptionLabel;
			}
		}

		public ILabel SkinNameVariationLabel
		{
			get
			{
				return this._skinNameVariationLabel;
			}
		}

		public ILabel HardPriceLabel
		{
			get
			{
				return this._hardPriceLabel;
			}
		}

		public ILabel SoftPriceLabel
		{
			get
			{
				return this._softPriceLabel;
			}
		}

		public IDynamicImage CharacterCarImage
		{
			get
			{
				return this._characterCarImage;
			}
		}

		public void Show()
		{
			base.gameObject.SetActive(true);
		}

		public void Hide()
		{
			base.gameObject.SetActive(false);
		}

		public void SetHasItem(bool hasItem)
		{
			this._hasItem.SetActive(hasItem);
		}

		public void SetHardPurchasable(bool isHardPurchasable)
		{
			if (null != this._hardPriceGroup)
			{
				this._hardPriceGroup.SetActive(isHardPurchasable);
			}
		}

		public void SetSoftPurchasable(bool isSoftPurchasable)
		{
			if (null != this._softPriceGroup)
			{
				this._softPriceGroup.SetActive(isSoftPurchasable);
			}
		}

		public void SetSkinNameVariation(bool hasNameVariation)
		{
			if (null != this._skinNameVariationGroup)
			{
				this._skinNameVariationGroup.SetActive(hasNameVariation);
			}
		}

		[SerializeField]
		private NGuiButton _button;

		[SerializeField]
		private NGuiRawImage _mainImage;

		[SerializeField]
		private NGuiRawImage _categoryIconImage;

		[SerializeField]
		private NGuiRawImage _characterIconImage;

		[SerializeField]
		private NGuiRawImage _characterCarImage;

		[SerializeField]
		private NGuiLabel _nameLabel;

		[SerializeField]
		private NGuiLabel _skinNameVariationLabel;

		[SerializeField]
		private NGuiLabel _categoryLabel;

		[SerializeField]
		private NGuiLabel _descriptionLabel;

		[SerializeField]
		private NGuiLabel _hardPriceLabel;

		[SerializeField]
		private NGuiLabel _softPriceLabel;

		[SerializeField]
		private GameObject _hasItem;

		[SerializeField]
		private GameObject _hardPriceGroup;

		[SerializeField]
		private GameObject _softPriceGroup;

		[SerializeField]
		private GameObject _skinNameVariationGroup;
	}
}
