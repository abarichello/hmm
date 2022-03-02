using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavymetalMachines.ReportSystem
{
	public interface IFeedbackWindowPresenterView
	{
		ICanvas MainCanvas { get; }

		ICanvasGroup MainCanvasGroup { get; }

		IAnimation InAnimation { get; }

		IAnimation OutAnimation { get; }

		IButton OkButton { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		ILabel TitleLabel { get; }

		ILabel DescriptionLabel { get; }

		IActivatable Description2TextActivatable { get; }

		IActivatable InfringementsTitleActivatable { get; }

		ILabel InfringementsListLabel { get; }

		IActivatable PunishmentsTitleActivatable { get; }

		ILabel PunishmentsListLabel { get; }
	}
}
