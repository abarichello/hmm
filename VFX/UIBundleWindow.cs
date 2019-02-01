using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Utils;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class UIBundleWindow
	{
		public UIBundleWindow(string bundleName)
		{
			this.windowName = bundleName;
		}

		public void LoadWindow(Transform parentTransform, UIBundleWindow.OnLoadDelegate onLoadCallbackDelegate, Action onLoadFailed)
		{
			UnityEngine.Debug.Log(string.Format("UIBundleWindow OPEN Obj:{0} UIBundleWindowHash:{1}", this.windowName, this.GetHashCode()));
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
			this._loadingToken = LoadingManager.GetLoadingToken(typeof(UIBundleWindow));
			ResourcesContent.Content asset = LoadingManager.ResourceContent.GetAsset(this.windowName);
			this._loadingToken.AddLoadable(new ResourcesLoadable(asset));
			UnityEngine.Debug.Log(string.Format("UIBundleWindow will start loading it's token. Hash:{0} Count:{1} WindowAssetName:{2}", this._loadingToken.GetHashCode(), this._loadingToken.loadableContent.Count, asset.AssetName));
			this._loadingToken.StartLoading(delegate
			{
				this.OnLoadWindowBundle(parentTransform, onLoadCallbackDelegate, onLoadFailed);
			});
		}

		private void OnLoadWindowBundle(Transform parentTransform, UIBundleWindow.OnLoadDelegate onLoadCallbackDelegate, Action onLoadFailed)
		{
			this._isLoading = false;
			ResourcesContent.Content asset = LoadingManager.ResourceContent.GetAsset(this.windowName);
			Transform transform = asset.Asset as Transform;
			HeavyMetalMachines.Utils.Debug.Assert(transform, string.Format("UIBundleWindow - Invalid Asset [{0}]", asset.AssetName), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			if (!transform)
			{
				if (onLoadFailed != null)
				{
					onLoadFailed();
				}
				return;
			}
			transform.gameObject.SetActive(false);
			this._windowTransform = UnityEngine.Object.Instantiate<Transform>(transform, Vector3.zero, Quaternion.identity);
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
			UnityEngine.Debug.Log(string.Format("UIBundleWindow CLOSE Obj:{0} UIBundleWindowHash{1}", this.windowName, this.GetHashCode()));
			if (this._isLoading)
			{
				this._unloadAfterLoad = true;
				return;
			}
			UnityEngine.Debug.Log(string.Format("UIBundleWindow UnloadWindow2 Obj:{0} UIBundleWindowHash{1}", this.windowName, this.GetHashCode()));
			this._unloadAfterLoad = false;
			if (!this._isLoaded)
			{
				return;
			}
			UnityEngine.Debug.Log(string.Format("UIBundleWindow UnloadWindow3 Obj:{0} UIBundleWindowHash{1}", this.windowName, this.GetHashCode()));
			UnityEngine.Object.Destroy(this._windowTransform.gameObject);
			this._loadingToken.Unload();
			this._windowTransform = null;
			this._isLoaded = false;
			UnityEngine.Debug.Log(string.Format("UIBundleWindow UnloadWindow4 Obj:{0} UIBundleWindowHash{1}", this.windowName, this.GetHashCode()));
		}

		public bool IsLoading()
		{
			return this._isLoading;
		}

		public static void OpenWindow(Transform parentTransform, string windowBundleName, ref UIBundleWindow bundleWindow, UIBundleWindow.OnLoadDelegate onLoadDelegate, Action onLoadFailed)
		{
			if (bundleWindow == null)
			{
				bundleWindow = new UIBundleWindow(windowBundleName);
			}
			bundleWindow.LoadWindow(parentTransform, onLoadDelegate, onLoadFailed);
		}

		public static void CloseWindow(Transform windowTransform, ref UIBundleWindow bundleWindow)
		{
			UnityEngine.Debug.Log(string.Format("UIBundleWindow STATIC CLOSE Obj:{0} UIBundleWindowHash:{1}", (bundleWindow == null) ? "null name" : bundleWindow.windowName, (bundleWindow == null) ? "warning, null ref" : bundleWindow.GetHashCode().ToString()));
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

		private LoadingManager.LoadingToken _loadingToken;

		private bool _isLoading;

		private bool _isLoaded;

		private bool _unloadAfterLoad;

		private string windowName;

		public delegate void OnLoadDelegate(Transform windowTransform);
	}
}
