using System;
using HeavyMetalMachines.Utils;
using Hoplon.Unity.Loading;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/HMM/HmmUiImage")]
	public class HmmUiImage : Image, IDynamicAssetListener<Texture2D>
	{
		public void TryToLoadAsset(string assetName)
		{
			if (string.IsNullOrEmpty(assetName))
			{
				HmmUiImage.Log.Warn("Asset name is null. Ignoring.");
				return;
			}
			if (base.IsDestroyed())
			{
				return;
			}
			if (assetName.Equals(this._assetName, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			this._assetName = assetName;
			if (!this._isLoadingAsset)
			{
				this._alphaBeforeLoadAsset = this.color.a;
				this._isLoadingAsset = true;
			}
			this.SetAlpha(0f);
			if (!Loading.TextureManager.GetAssetAsync(this._assetName, this))
			{
				this.SetAlpha(this._alphaBeforeLoadAsset);
				this._isLoadingAsset = false;
			}
		}

		public virtual void OnAssetLoaded(string textureName, Texture2D texture)
		{
			if (base.IsDestroyed())
			{
				return;
			}
			if (texture != null)
			{
				if (!this._assetName.Equals(textureName))
				{
					HmmUiImage.Log.Warn(string.Format("asset loaded is diferent from initial request, ignoring. AssetName: {0}, AssetLoaded: {1}", this._assetName, textureName));
					return;
				}
				base.sprite = Sprite.Create(texture, new Rect(0f, 0f, (float)texture.width, (float)texture.height), Vector2.zero);
				this.SetAlpha(this._alphaBeforeLoadAsset);
				this._isLoadingAsset = false;
				if (this._resizeAfterLoad)
				{
					this.SetNativeSize();
				}
			}
			else
			{
				Debug.Assert(false, string.Format("HmmUiImage: Image failed to load -> {0}", this._assetName), Debug.TargetTeam.GUI);
			}
		}

		public void SetSprite(Sprite sprite)
		{
			base.sprite = sprite;
			if (this._resizeAfterLoad)
			{
				this.SetNativeSize();
			}
		}

		public void ClearAsset()
		{
			this._assetName = string.Empty;
			base.sprite = null;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HmmUiImage));

		[SerializeField]
		private bool _resizeAfterLoad;

		private string _assetName;

		private float _alphaBeforeLoadAsset;

		private bool _isLoadingAsset;
	}
}
