using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend.Welcome
{
	public class WelcomeView : MonoBehaviour, IWelcomeView
	{
		public ICanvas Canvas
		{
			get
			{
				return this._canvas;
			}
		}

		public ICanvasGroup EulaCanvasGroup
		{
			get
			{
				return this._eulaCanvasGroup;
			}
		}

		public ICanvasGroup MainCanvasGroup
		{
			get
			{
				return this._mainCanvasGroup;
			}
		}

		public IUiNavigationGroupHolder NavigationGroupHolder
		{
			get
			{
				return this._navigationGroupHolder;
			}
		}

		public IButton ExitButton
		{
			get
			{
				return this._exitButton;
			}
		}

		public IAnimation EnterAnimation
		{
			get
			{
				return this._enterAnimation;
			}
		}

		public IAnimation ExitAnimation
		{
			get
			{
				return this._exitAnimation;
			}
		}

		public IAnimation ButtonLabelEnterAnimation
		{
			get
			{
				return this._buttonLabelEnterAnimation;
			}
		}

		public IAnimation ButtonLabelIdleAnimation
		{
			get
			{
				return this._buttonLabelIdleAnimation;
			}
		}

		public IAnimation ButtonLabelExitAnimation
		{
			get
			{
				return this._buttonLabelExitAnimation;
			}
		}

		public IAnimation FeedbackLabelEnterAnimation
		{
			get
			{
				return this._feedbackLabelEnterAnimation;
			}
		}

		public IAnimation FeedbackLabelExitAnimation
		{
			get
			{
				return this._feedbackLabelExitAnimation;
			}
		}

		public ILabel FeedbackLabel
		{
			get
			{
				return this._feedbackLabel;
			}
		}

		public ILabel ConnectedRegionLabel
		{
			get
			{
				return this._regionLabel;
			}
		}

		public IActivatable LoadingActivatable
		{
			get
			{
				return this._loadingGameObject;
			}
		}

		public IAnimation LoadingEnterAnimation
		{
			get
			{
				return this._loadingEnterAnimation;
			}
		}

		public IAnimation LoadingExitAnimation
		{
			get
			{
				return this._loadingExitAnimation;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<IWelcomeView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IWelcomeView>(null);
		}

		[SerializeField]
		private UnityCanvas _canvas;

		[SerializeField]
		private UnityCanvasGroup _eulaCanvasGroup;

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroup;

		[SerializeField]
		private UiNavigationGroupHolder _navigationGroupHolder;

		[SerializeField]
		private UnityButton _exitButton;

		[SerializeField]
		private UnityAnimation _enterAnimation;

		[SerializeField]
		private UnityAnimation _exitAnimation;

		[SerializeField]
		private UnityAnimation _buttonLabelEnterAnimation;

		[SerializeField]
		private UnityAnimation _buttonLabelIdleAnimation;

		[SerializeField]
		private UnityAnimation _buttonLabelExitAnimation;

		[SerializeField]
		private UnityAnimation _feedbackLabelEnterAnimation;

		[SerializeField]
		private UnityAnimation _feedbackLabelExitAnimation;

		[SerializeField]
		private UnityLabel _feedbackLabel;

		[SerializeField]
		private UnityLabel _regionLabel;

		[SerializeField]
		private GameObjectActivatable _loadingGameObject;

		[SerializeField]
		private UnityAnimation _loadingEnterAnimation;

		[SerializeField]
		private UnityAnimation _loadingExitAnimation;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
