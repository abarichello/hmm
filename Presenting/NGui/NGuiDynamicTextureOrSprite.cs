using System;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class NGuiDynamicTextureOrSprite : IDynamicImage, IValueHolder
	{
		public void SetImageName(string imageName)
		{
			if (this._mode == NGuiDynamicTextureOrSprite.Mode.Texture)
			{
				((HMMUI2DDynamicTexture)this._basic).TextureName = imageName;
			}
			else
			{
				((HMMUI2DDynamicSprite)this._basic).SpriteName = imageName;
			}
		}

		public bool IsActive
		{
			get
			{
				if (this._mode == NGuiDynamicTextureOrSprite.Mode.Texture)
				{
					return this._basic.enabled;
				}
				return this._basic.enabled;
			}
			set
			{
				if (this._mode == NGuiDynamicTextureOrSprite.Mode.Texture)
				{
					this._basic.enabled = value;
				}
				this._basic.enabled = value;
			}
		}

		public bool HasValue
		{
			get
			{
				return this._basic != null;
			}
		}

		public Color Color
		{
			get
			{
				return this._basic.color.ToHmmColor();
			}
			set
			{
				this._basic.color = value.ToUnityColor();
			}
		}

		public float Alpha
		{
			get
			{
				return this._basic.alpha;
			}
			set
			{
				this._basic.alpha = value;
			}
		}

		[SerializeField]
		private NGuiDynamicTextureOrSprite.Mode _mode;

		[SerializeField]
		private UIBasicSprite _basic;

		public enum Mode
		{
			Texture,
			Sprite
		}
	}
}
