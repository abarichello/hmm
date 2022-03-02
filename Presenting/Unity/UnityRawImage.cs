using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityRawImage : ITextureImage, IImage
	{
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

		public float Width
		{
			get
			{
				return (this._rawImage.gameObject.transform as RectTransform).sizeDelta.x;
			}
		}

		public void SetEulerAngles(Vector3 angles)
		{
			this._rawImage.gameObject.transform.eulerAngles = angles;
		}

		public ITexture Texture
		{
			get
			{
				return new UnityTexture(this._rawImage.texture);
			}
			set
			{
				this._rawImage.texture = ((value != null) ? ((UnityTexture)value).GetTexture() : null);
			}
		}

		[SerializeField]
		[UsedImplicitly]
		private RawImage _rawImage;
	}
}
