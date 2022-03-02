using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;

namespace HeavyMetalMachines.Training.View
{
	public class UnityTrainingPopUpViewV3 : MonoBehaviour, ITrainingPopUpViewV3
	{
		public ICanvas Canvas
		{
			get
			{
				return this._canvas;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IButton ConfirmButton
		{
			get
			{
				return this._confirmButton;
			}
		}

		public IAnimation ScreenInAnimation
		{
			get
			{
				return this._screenInAnimation;
			}
		}

		public IAnimation ScreenOutAnimation
		{
			get
			{
				return this._screenOutAnimation;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ITrainingPopUpViewV3>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ITrainingPopUpViewV3>(null);
		}

		[InjectOnClient]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _canvas;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UnityButton _confirmButton;

		[SerializeField]
		private UnityAnimation _screenInAnimation;

		[SerializeField]
		private UnityAnimation _screenOutAnimation;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
