using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.Training.View
{
	public interface ITrainingSelectionScreenView
	{
		ICanvas Canvas { get; }

		ITitle Title { get; }

		IButton BackButton { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		ITrainingSelectionView[] SelectionViews { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
