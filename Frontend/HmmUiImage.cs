using System;
using HeavyMetalMachines.Utils;
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
			string text = assetName.ToLower();
			if (text.Equals(this._assetName))
			{
				return;
			}
			this._assetName = text;
			if (!this._isLoadingAsset)
			{
				this._alphaBeforeLoadAsset = this.color.a;
				this._isLoadingAsset = true;
			}
			this.SetAlpha(0f);
			if (!SingletonMonoBehaviour<LoadingManager>.Instance.TextureManager.GetAssetAsync(this._assetName, this))
			{
				HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("HmmUiImage: Image/Bundle not found -> {0}", this._assetName), HeavyMetalMachines.Utils.Debug.TargetTeam.GUI);
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
				HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("HmmUiImage: Image failed to load -> {0}", this._assetName), HeavyMetalMachines.Utils.Debug.TargetTeam.GUI);
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
