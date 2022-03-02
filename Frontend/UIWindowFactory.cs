using System;
using Hoplon.Unity.Loading;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class UIWindowFactory : IUIWindowFactory
	{
		public UIWindowFactory(DiContainer container)
		{
			this._container = container;
		}

		public void LoadWindow(string windowName, Transform parentTransform, Action<UIWindow> onLoadCallback)
		{
			LoadingToken loadingToken = new LoadingToken(typeof(UIWindow));
			Content asset = Loading.Content.GetAsset(windowName);
			loadingToken.AddLoadable(Loading.GetResourceLoadable(asset));
			Loading.Engine.LoadToken(loadingToken, delegate(LoadingResult result)
			{
				if (LoadStatusExtensions.IsError(result.Status))
				{
					return;
				}
				Content asset2 = Loading.Content.GetAsset(windowName);
				Transform transform = (Transform)asset2.Asset;
				UIWindow component = this._container.InstantiatePrefab(transform).GetComponent<UIWindow>();
				component.transform.parent = parentTransform;
				component.transform.localScale = transform.transform.localScale;
				component.transform.localPosition = Vector3.zero;
				onLoadCallback(component);
			});
		}

		private DiContainer _container;
	}
}
