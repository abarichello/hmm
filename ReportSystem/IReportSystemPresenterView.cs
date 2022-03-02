using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;

namespace HeavymetalMachines.ReportSystem
{
	public interface IReportSystemPresenterView
	{
		ICanvas MainCanvas { get; }

		ICanvasGroup MainCanvasGroup { get; }

		IAnimator WindowAnimator { get; }

		IButton ReportButton { get; }

		IButton CancelButton { get; }

		IButton OkButton { get; }

		IActivatable LoadingActivatable { get; }

		IInputField ReportInputField { get; }

		ICanvasGroup TogglesAndInputFieldCanvasGroup { get; }

		IReportSystemPlayerView ReportSystemPlayerView { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IUiNavigationAxisSelector UiNavigationAxisSelector { get; }

		IAnimator FeedbacksAnimator { get; }

		IReportSystemToggleView CreateToggleView();

		double GetOutAnimationLength();
	}
}
