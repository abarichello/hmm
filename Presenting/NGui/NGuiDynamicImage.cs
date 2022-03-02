using System;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class NGuiDynamicImage : IDynamicImage, IValueHolder
	{
		public void SetImageName(string imageName)
		{
			this._dynamicSprite.SpriteName = imageName;
		}

		public bool IsActive
		{
			get
			{
				return this._dynamicSprite.enabled;
			}
			set
			{
				this._dynamicSprite.enabled = value;
			}
		}

		public bool HasValue
		{
			get
			{
				return this._dynamicSprite != null;
			}
		}

		public Color Color
		{
			get
			{
				return this._dynamicSprite.color.ToHmmColor();
			}
			set
			{
				this._dynamicSprite.color = value.ToUnityColor();
			}
		}

		public float Alpha
		{
			get
			{
				return this._dynamicSprite.alpha;
			}
			set
			{
				this._dynamicSprite.alpha = value;
			}
		}

		[SerializeField]
		private HMMUI2DDynamicSprite _dynamicSprite;
	}
}
