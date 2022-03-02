using System;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class NGuiImage : ISpriteImage, IImage
	{
		public bool IsActive
		{
			get
			{
				return this._sprite.gameObject.activeSelf;
			}
			set
			{
				this._sprite.gameObject.SetActive(value);
			}
		}

		public Color Color
		{
			get
			{
				return this._sprite.color.ToHmmColor();
			}
			set
			{
				this._sprite.color = value.ToUnityColor();
			}
		}

		public ISprite Sprite
		{
			get
			{
				return new UnitySprite(this._sprite.sprite2D);
			}
			set
			{
				this._sprite.sprite2D = ((value != null) ? ((UnitySprite)value).GetSprite() : null);
			}
		}

		public float Width
		{
			get
			{
				return (float)this._sprite.width;
			}
		}

		[SerializeField]
		private UI2DSprite _sprite;
	}
}
