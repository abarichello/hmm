using System;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class NGuiRawImage : IDynamicImage, IValueHolder
	{
		public void SetImageName(string imageName)
		{
			this._texture.TextureName = imageName;
		}

		public bool IsActive
		{
			get
			{
				return this._texture.enabled;
			}
			set
			{
				this._texture.enabled = value;
			}
		}

		public bool HasValue
		{
			get
			{
				return this._texture != null;
			}
		}

		public Color Color
		{
			get
			{
				return this._texture.color.ToHmmColor();
			}
			set
			{
				this._texture.color = value.ToUnityColor();
			}
		}

		public float Alpha
		{
			get
			{
				return this._texture.alpha;
			}
			set
			{
				this._texture.alpha = value;
			}
		}

		[SerializeField]
		private HMMUI2DDynamicTexture _texture;
	}
}
