using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Frontend.BigTextScroller;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend.Welcome
{
	public class EulaView : MonoBehaviour, IEulaView
	{
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

		public IButton AcceptButton
		{
			get
			{
				return this._acceptButton;
			}
		}

		public IButton RefuseButton
		{
			get
			{
				return this._refuseButton;
			}
		}

		public ITextScroller Scroller
		{
			get
			{
				return this._bigTextScrollerDelegate;
			}
		}

		public IUiNavigationGroupHolder NavigationGroup
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void Awake()
		{
			this._bigTextScrollerDelegate = new BigTextScrollerDelegate(this._enhancedScroller, this._bigTextParagraphView);
			this._viewProvider.Bind<IEulaView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IEulaView>(null);
		}

		[SerializeField]
		private UnityAnimation _enterAnimation;

		[SerializeField]
		private UnityAnimation _exitAnimation;

		[SerializeField]
		private UnityButton _acceptButton;

		[SerializeField]
		private UnityButton _refuseButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private EnhancedScroller _enhancedScroller;

		[SerializeField]
		private BigTextParagraphView _bigTextParagraphView;

		private BigTextScrollerDelegate _bigTextScrollerDelegate;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
