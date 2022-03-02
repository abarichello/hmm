using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation.AxisSelector;

namespace HeavyMetalMachines.MainMenuPresenting.View
{
	public interface IStartMenuPlayModesView
	{
		IActivatable PlayActivatable { get; }

		IActivatable WaitingActivatable { get; }

		IActivatable TimerActivatable { get; }

		ILabel PlayLabel { get; }

		ILabel WaitingLabel { get; }

		IButton CancelSearchButton { get; }

		IUiNavigationAxisSelectorTransformHandler AxisSelectorTransformHandler { get; }

		void UiNavigationSelectionOnPlayButton();
	}
}
