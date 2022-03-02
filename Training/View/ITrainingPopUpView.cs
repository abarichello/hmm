using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.Training.View
{
	public interface ITrainingPopUpView
	{
		ICanvas Canvas { get; }

		IButton PlayCasualButton { get; }

		IButton PlayTrainingButton { get; }

		IButton CustomMatchButton { get; }

		IButton CloseButton { get; }

		IToggle CheckBox { get; }

		ILabel MatchCountLabel { get; }

		ILabel DescriptionLabel { get; }

		IAnimation ScreenInAnimation { get; }

		IAnimation ScreenOutAnimation { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
