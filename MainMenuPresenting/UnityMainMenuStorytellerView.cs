using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class UnityMainMenuStorytellerView : MonoBehaviour, IMainMenuStorytellerView
	{
		public IButton OpenStorytellerButton
		{
			get
			{
				return this._openStorytellerButton;
			}
		}

		public IActivatable ButtonActivatable
		{
			get
			{
				return this._mainGameObject;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<IMainMenuStorytellerView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IMainMenuStorytellerView>(null);
		}

		[SerializeField]
		private NGuiButton _openStorytellerButton;

		[SerializeField]
		private GameObjectActivatable _mainGameObject;

		[InjectOnClient]
		private IViewProvider _viewProvider;
	}
}
