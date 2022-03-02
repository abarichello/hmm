using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityDynamicImage : IDynamicImage, IValueHolder
	{
		public UnityDynamicImage(HmmUiImage hmmUiImage)
		{
			this._hmmUiImage = hmmUiImage;
		}

		public void SetSprite(Sprite sprite)
		{
			this._hmmUiImage.SetSprite(sprite);
		}

		public void SetImageName(string imageName)
		{
			this._hmmUiImage.TryToLoadAsset(imageName);
		}

		public bool IsActive
		{
			get
			{
				GameObject gameObject = this._hmmUiImage.gameObject;
				return this.HasValue && gameObject != null && gameObject.activeSelf;
			}
			set
			{
				if (this.HasValue && this._hmmUiImage.gameObject != null)
				{
					this._hmmUiImage.gameObject.SetActive(value);
				}
			}
		}

		public Color Color
		{
			get
			{
				return this._hmmUiImage.color.ToHmmColor();
			}
			set
			{
				this._hmmUiImage.color = value.ToUnityColor();
			}
		}

		public bool HasValue
		{
			get
			{
				return this._hmmUiImage != null;
			}
		}

		public float Alpha
		{
			get
			{
				return this._hmmUiImage.color.a;
			}
			set
			{
				this._hmmUiImage.SetAlpha(value);
			}
		}

		[SerializeField]
		private HmmUiImage _hmmUiImage;
	}
}
