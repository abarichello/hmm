using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using UnityEngine;
using Zenject;

namespace HeavymetalMachines.ReportSystem
{
	public class UnityReportSystemPresenterView : MonoBehaviour, IReportSystemPresenterView
	{
		private void OnEnable()
		{
			this._viewProvider.Bind<IReportSystemPresenterView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IReportSystemPresenterView>(null);
		}

		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
			}
		}

		public ICanvasGroup MainCanvasGroup
		{
			get
			{
				return this._mainCanvasGroup;
			}
		}

		public IAnimator WindowAnimator
		{
			get
			{
				return this._windowAnimator;
			}
		}

		public IButton ReportButton
		{
			get
			{
				return this._reportButton;
			}
		}

		public IButton CancelButton
		{
			get
			{
				return this._cancelButton;
			}
		}

		public IButton OkButton
		{
			get
			{
				return this._okButton;
			}
		}

		public IActivatable LoadingActivatable
		{
			get
			{
				return this._loadingActivatable;
			}
		}

		public IInputField ReportInputField
		{
			get
			{
				return this._reportInputField;
			}
		}

		public ICanvasGroup TogglesAndInputFieldCanvasGroup
		{
			get
			{
				return this._togglesAndInputFieldCanvasGroup;
			}
		}

		public IReportSystemPlayerView ReportSystemPlayerView
		{
			get
			{
				return this._reportSystemPlayerView;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public IAnimator FeedbacksAnimator
		{
			get
			{
				return this._feedbacksAnimator;
			}
		}

		public IReportSystemToggleView CreateToggleView()
		{
			this._toggleViewCount++;
			if (this._toggleViewCount > 1)
			{
				UnityReportSystemToggleView unityReportSystemToggleView = Object.Instantiate<UnityReportSystemToggleView>(this._reportSystemToggleView, this._reportSystemToggleView.transform.parent);
				unityReportSystemToggleView.transform.SetSiblingIndex(this._reportSystemToggleView.transform.GetSiblingIndex() + (this._toggleViewCount - 1));
				return unityReportSystemToggleView;
			}
			return this._reportSystemToggleView;
		}

		public double GetOutAnimationLength()
		{
			return (double)this._outAnimationClip.length;
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroup;

		[SerializeField]
		private UnityAnimator _windowAnimator;

		[SerializeField]
		private AnimationClip _outAnimationClip;

		[SerializeField]
		private UnityButton _reportButton;

		[SerializeField]
		private UnityButton _cancelButton;

		[SerializeField]
		private UnityButton _okButton;

		[SerializeField]
		private GameObjectActivatable _loadingActivatable;

		[SerializeField]
		private UnityInputField _reportInputField;

		[SerializeField]
		private UnityCanvasGroup _togglesAndInputFieldCanvasGroup;

		[SerializeField]
		private UnityReportSystemPlayerView _reportSystemPlayerView;

		[SerializeField]
		private UnityReportSystemToggleView _reportSystemToggleView;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[SerializeField]
		private UnityAnimator _feedbacksAnimator;

		private int _toggleViewCount;
	}
}
