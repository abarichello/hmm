using System;
using HeavyMetalMachines.Utils;
using Hoplon.Unity.Loading;
using Pocketverse;
using SharedUtils.Loading;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/HMM/HmmUiRawImage")]
	public class HmmUiRawImage : RawImage, IDynamicAssetListener<Texture2D>
	{
		public void TryToLoadAsset(string assetName)
		{
			if (string.IsNullOrEmpty(assetName))
			{
				HmmUiRawImage.Log.Warn("Asset name is null. Ignoring.");
				return;
			}
			if (base.IsDestroyed())
			{
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
			if (!Loading.TextureManager.GetAssetAsync(this._assetName, this))
			{
				Debug.Assert(false, string.Format("HmmUiRawImage: Image/Bundle not found -> {0}", this._assetName), Debug.TargetTeam.GUI);
				this.SetAlpha(this._alphaBeforeLoadAsset);
				this._isLoadingAsset = false;
			}
		}

		public IObservable<Unit> LoadAsset(string assetName)
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (string.IsNullOrEmpty(assetName))
				{
					HmmUiRawImage.Log.Warn("Asset name is null. Ignoring.");
					return Observable.ReturnUnit();
				}
				string text = assetName.ToLower();
				if (text.Equals(this._assetName))
				{
					return Observable.ReturnUnit();
				}
				this._assetName = text;
				if (!this._isLoadingAsset)
				{
					this._alphaBeforeLoadAsset = this.color.a;
					this._isLoadingAsset = true;
				}
				this._loadingObservation = new Subject<Unit>();
				return Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					this.ListenToLoading(),
					this.CallLoading()
				});
			});
		}

		private IObservable<Unit> CallLoading()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this.SetAlpha(0f);
				if (!Loading.TextureManager.GetAssetAsync(this._assetName, this))
				{
					Debug.Assert(false, string.Format("HmmUiRawImage: Image/Bundle not found -> {0}", this._assetName), Debug.TargetTeam.GUI);
					this._isLoadingAsset = false;
				}
				return Observable.ReturnUnit();
			});
		}

		private IObservable<Unit> ListenToLoading()
		{
			return this._loadingObservation;
		}

		public virtual void OnAssetLoaded(string textureName, Texture2D loadedTexture)
		{
			if (base.IsDestroyed())
			{
				if (this._loadingObservation != null)
				{
					this._loadingObservation.OnCompleted();
				}
				return;
			}
			if (loadedTexture != null)
			{
				if (!this._assetName.Equals(textureName))
				{
					HmmUiRawImage.Log.Warn(string.Format("asset loaded is diferent from initial request, ignoring. AssetName: {0}, AssetLoaded: {1}", this._assetName, textureName));
					return;
				}
				base.texture = loadedTexture;
				this.SetAlpha(this._alphaBeforeLoadAsset);
				this._isLoadingAsset = false;
				if (this._resizeAfterLoad)
				{
					this.SetNativeSize();
				}
			}
			else
			{
				Debug.Assert(false, string.Format("HmmUiRawImage: Image failed to load -> {0}", this._assetName), Debug.TargetTeam.GUI);
			}
			if (this._loadingObservation != null)
			{
				this._loadingObservation.OnCompleted();
			}
		}

		public void ClearAsset()
		{
			this._assetName = string.Empty;
			base.texture = null;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HmmUiRawImage));

		[SerializeField]
		private bool _resizeAfterLoad;

		private string _assetName;

		private float _alphaBeforeLoadAsset;

		private bool _isLoadingAsset;

		private Subject<Unit> _loadingObservation;
	}
}
