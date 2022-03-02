using System;
using HeavyMetalMachines.Frontend;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Chat.Presenting.Infra
{
	public class HudChatLoader : IHudChatLoader
	{
		public HudChatLoader(DiContainer diContainer, HudChatController hudChatController)
		{
			this._diContainer = diContainer;
			this._hudChatController = hudChatController;
		}

		public IObservable<Unit> Load(HudChatState state)
		{
			return Observable.Defer<Unit>(delegate()
			{
				Transform parentTransform;
				if (this.TryToGetParentTransform(out parentTransform))
				{
					this._instantiatedHudChatController = this._diContainer.InstantiatePrefabForComponent<HudChatController>(this._hudChatController, parentTransform);
					if (state == 1)
					{
						this._instantiatedHudChatController.SetPickModeOn();
					}
					this.FixPosition(this._instantiatedHudChatController);
				}
				return Observable.ReturnUnit();
			});
		}

		private void FixPosition(HudChatController hudChatControllerGameObject)
		{
			Transform transform = hudChatControllerGameObject.transform;
			Vector3 localPosition = transform.localPosition;
			localPosition.y = -520f;
			transform.localPosition = localPosition;
		}

		public IObservable<Unit> Unload()
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (null != this._instantiatedHudChatController)
				{
					Object.Destroy(this._instantiatedHudChatController.gameObject);
				}
				return Observable.ReturnUnit();
			});
		}

		private bool TryToGetParentTransform(out Transform parentTransform)
		{
			HMMHub hub = GameHubBehaviour.Hub;
			if (null == hub)
			{
				Debug.LogWarning("{HudChatLoader} Hub not found: Chat will not load.");
				parentTransform = null;
				return false;
			}
			parentTransform = hub.State.MainDynamicUiHolder.transform;
			return true;
		}

		private const int VerticalPosition = -520;

		private readonly DiContainer _diContainer;

		private readonly HudChatController _hudChatController;

		private HudChatController _instantiatedHudChatController;
	}
}
