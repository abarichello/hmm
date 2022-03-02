using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Utils;
using Hoplon.Unity.Loading;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX
{
	public class UIBundleWindow
	{
		public UIBundleWindow(string bundleName, DiContainer container)
		{
			this._container = container;
			this.windowName = bundleName;
		}

		public void LoadWindow(Transform parentTransform, UIBundleWindow.OnLoadDelegate onLoadCallbackDelegate, Action onLoadFailed)
		{
			Debug.Log(string.Format("UIBundleWindow OPEN Obj:{0} UIBundleWindowHash:{1}", this.windowName, this.GetHashCode()));
			if (this._isLoading)
			{
				return;
			}
			if (this._isLoaded)
			{
				if (onLoadCallbackDelegate != null)
				{
					onLoadCallbackDelegate(this._windowTransform);
				}
				return;
			}
			this._isLoading = true;
			this._loadingToken = new LoadingToken(typeof(UIBundleWindow));
			Content asset = Loading.Content.GetAsset(this.windowName);
			this._loadingToken.AddLoadable(Loading.GetResourceLoadable(asset));
			Debug.Log(string.Format("UIBundleWindow will start loading it's token. Hash:{0} Count:{1} WindowAssetName:{2}", this._loadingToken.GetHashCode(), this._loadingToken.LoadableContent.Count, asset.AssetName));
			Loading.Engine.LoadToken(this._loadingToken, delegate(LoadingResult result)
			{
				if (LoadStatusExtensions.IsError(result.Status))
				{
					Debug.LogErrorFormat("UIBundleWindow ({0}) load failed with status: {1}", new object[]
					{
						this.windowName,
						result.Status
					});
					return;
				}
				this.OnLoadWindowBundle(parentTransform, onLoadCallbackDelegate, onLoadFailed);
			});
		}

		private void OnLoadWindowBundle(Transform parentTransform, UIBundleWindow.OnLoadDelegate onLoadCallbackDelegate, Action onLoadFailed)
		{
			this._isLoading = false;
			Content asset = Loading.Content.GetAsset(this.windowName);
			Transform transform = asset.Asset as Transform;
			Debug.Assert(transform, string.Format("UIBundleWindow - Invalid Asset [{0}]", asset.AssetName), Debug.TargetTeam.All);
			if (!transform)
			{
				if (onLoadFailed != null)
				{
					onLoadFailed();
				}
				return;
			}
			transform.gameObject.SetActive(false);
			this._windowTransform = this._container.InstantiatePrefab(transform, Vector3.zero, Quaternion.identity, null).transform;
			this._windowTransform.name = "[GuiBundle]" + transform.name;
			this._isLoaded = true;
			if (this._unloadAfterLoad)
			{
				this.UnloadWindow();
				return;
			}
			if (parentTransform != null)
			{
				this.SetParent(parentTransform, transform);
			}
			if (onLoadCallbackDelegate != null)
			{
				onLoadCallbackDelegate(this._windowTransform);
			}
		}

		public void SetParent(Transform parentTransform, Transform reference)
		{
			if (!this._isLoaded)
			{
				return;
			}
			Vector3 localScale = this._windowTransform.localScale;
			this._windowTransform.SetParent(parentTransform);
			this._windowTransform.localScale = localScale;
			this._windowTransform.localPosition = reference.localPosition;
		}

		public void UnloadWindow()
		{
			Debug.Log(string.Format("UIBundleWindow CLOSE Obj:{0} UIBundleWindowHash{1}", this.windowName, this.GetHashCode()));
			if (this._isLoading)
			{
				this._unloadAfterLoad = true;
				return;
			}
			Debug.Log(string.Format("UIBundleWindow UnloadWindow2 Obj:{0} UIBundleWindowHash{1}", this.windowName, this.GetHashCode()));
			this._unloadAfterLoad = false;
			if (!this._isLoaded)
			{
				return;
			}
			Debug.Log(string.Format("UIBundleWindow UnloadWindow3 Obj:{0} UIBundleWindowHash{1}", this.windowName, this.GetHashCode()));
			Object.Destroy(this._windowTransform.gameObject);
			Loading.Engine.UnloadToken(this._loadingToken);
			this._windowTransform = null;
			this._isLoaded = false;
			Debug.Log(string.Format("UIBundleWindow UnloadWindow4 Obj:{0} UIBundleWindowHash{1}", this.windowName, this.GetHashCode()));
		}

		public bool IsLoading()
		{
			return this._isLoading;
		}

		public static void OpenWindow(DiContainer contanier, Transform parentTransform, string windowBundleName, ref UIBundleWindow bundleWindow, UIBundleWindow.OnLoadDelegate onLoadDelegate, Action onLoadFailed)
		{
			if (bundleWindow == null)
			{
				bundleWindow = new UIBundleWindow(windowBundleName, contanier);
			}
			bundleWindow.LoadWindow(parentTransform, onLoadDelegate, onLoadFailed);
		}

		public static void CloseWindow(Transform windowTransform, ref UIBundleWindow bundleWindow)
		{
			Debug.Log(string.Format("UIBundleWindow STATIC CLOSE Obj:{0} UIBundleWindowHash:{1}", (bundleWindow == null) ? "null name" : bundleWindow.windowName, (bundleWindow == null) ? "warning, null ref" : bundleWindow.GetHashCode().ToString()));
			if (windowTransform != null)
			{
				HudWindow component = windowTransform.GetComponent<HudWindow>();
				if (component != null)
				{
					component.SetWindowVisibility(false);
				}
			}
			if (bundleWindow != null)
			{
				bundleWindow.UnloadWindow();
			}
		}

		private Transform _windowTransform;

		private LoadingToken _loadingToken;

		private bool _isLoading;

		private bool _isLoaded;

		private bool _unloadAfterLoad;

		private DiContainer _container;

		private string windowName;

		public delegate void OnLoadDelegate(Transform windowTransform);
	}
}
