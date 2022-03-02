using System;
using Hoplon.Unity.Loading;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMUI2DDynamicTexture : UITexture, IDynamicAssetListener<Texture2D>
	{
		public string TextureName
		{
			get
			{
				if (!string.IsNullOrEmpty(this._textureName))
				{
					return this._textureName;
				}
				if (this.mainTexture)
				{
					this._textureName = this.mainTexture.name;
					return this._textureName;
				}
				return string.Empty;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					Debug.LogWarning("You are trying to set a null or empty spriteName");
					return;
				}
				this._textureName = value;
				Loading.TextureManager.GetAssetAsync(this._textureName, this);
			}
		}

		public string GetAssetName()
		{
			return this.TextureName;
		}

		public virtual void OnAssetLoaded(string textureName, Texture2D texture)
		{
			this.mainTexture = texture;
		}

		private string _textureName;
	}
}
