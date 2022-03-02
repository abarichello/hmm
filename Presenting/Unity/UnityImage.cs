using System;
using Hoplon.Assertions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityImage : ISpriteImage, IImage
	{
		public bool IsActive
		{
			get
			{
				return this._image.gameObject.activeSelf;
			}
			set
			{
				this._image.gameObject.SetActive(value);
			}
		}

		public Color Color
		{
			get
			{
				return this._image.color.ToHmmColor();
			}
			set
			{
				this._image.color = value.ToUnityColor();
			}
		}

		public float Width
		{
			get
			{
				return (this._image.gameObject.transform as RectTransform).sizeDelta.x;
			}
		}

		public void SetEulerAngles(Vector3 angles)
		{
			this._image.gameObject.transform.eulerAngles = angles;
		}

		public ISprite Sprite
		{
			get
			{
				return new UnitySprite(this._image.sprite);
			}
			set
			{
				Assert.IsNotNull<Image>(this._image, "Image was not set.");
				this._image.sprite = ((value != null) ? ((UnitySprite)value).GetSprite() : null);
			}
		}

		[SerializeField]
		[UsedImplicitly]
		private Image _image;
	}
}
