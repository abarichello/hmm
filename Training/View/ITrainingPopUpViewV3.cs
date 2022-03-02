using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.Training.View
{
	public interface ITrainingPopUpViewV3
	{
		ICanvas Canvas { get; }

		IButton BackButton { get; }

		IButton ConfirmButton { get; }

		IAnimation ScreenInAnimation { get; }

		IAnimation ScreenOutAnimation { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
