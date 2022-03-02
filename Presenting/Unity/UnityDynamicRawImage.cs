using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityDynamicRawImage : IDynamicImage, IValueHolder
	{
		public void SetImageName(string imageName)
		{
			this._rawImage.TryToLoadAsset(imageName);
		}

		public bool IsActive
		{
			get
			{
				return this._rawImage.gameObject.activeSelf;
			}
			set
			{
				this._rawImage.gameObject.SetActive(value);
			}
		}

		public bool HasValue
		{
			get
			{
				return this._rawImage != null;
			}
		}

		public Color Color
		{
			get
			{
				return this._rawImage.color.ToHmmColor();
			}
			set
			{
				this._rawImage.color = value.ToUnityColor();
			}
		}

		public float Alpha
		{
			get
			{
				return this._rawImage.color.a;
			}
			set
			{
				this._rawImage.SetAlpha(value);
			}
		}

		[SerializeField]
		private HmmUiRawImage _rawImage;
	}
}
