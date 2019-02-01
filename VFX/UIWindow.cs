using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public abstract class UIWindow : GameHubBehaviour
	{
		public virtual void OnDestroy()
		{
			HeavyMetalMachines.Utils.Debug.Assert(this._loadingToken != null, string.Format("[SD] UIWindow loading token is null. GameObject:[{0}]", base.gameObject.name), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			this._loadingToken.Unload();
		}

		public static void LoadWindow(string windowName, Transform parentTransform, UIWindow.OnLoadDelegate onLoadCallbackDelegate)
		{
			LoadingManager.LoadingToken loadingToken = LoadingManager.GetLoadingToken(typeof(UIWindow));
			ResourcesContent.Content asset = LoadingManager.ResourceContent.GetAsset(windowName);
			loadingToken.AddLoadable(new ResourcesLoadable(asset));
			loadingToken.StartLoading(delegate
			{
				ResourcesContent.Content asset2 = LoadingManager.ResourceContent.GetAsset(windowName);
				UIWindow component = ((Transform)asset2.Asset).GetComponent<UIWindow>();
				UIWindow uiwindow = UnityEngine.Object.Instantiate<UIWindow>(component);
				uiwindow.transform.parent = parentTransform;
				uiwindow.transform.localScale = component.transform.localScale;
				uiwindow.transform.localPosition = Vector3.zero;
				uiwindow._loadingToken = loadingToken;
				onLoadCallbackDelegate(uiwindow);
			});
		}

		private LoadingManager.LoadingToken _loadingToken;

		public delegate void OnLoadDelegate(UIWindow window);
	}
}
