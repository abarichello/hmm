using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Horta.View
{
	public interface IHORTATimelineView
	{
		ICanvas MainCanvas { get; }

		ICanvasGroup MainCanvasGroup { get; }

		IButton PlayPauseButton { get; }

		ISlider TimeSlider { get; }

		IButton SlowButton { get; }

		IButton AccelerateButton { get; }

		ILabel MultiplierLabel { get; }

		ILabel CurrentTimeLabel { get; }

		ILabel MaxTimeLabel { get; }

		void SetAsPlayButton();

		void SetAsPauseButton();

		void SetSpeedButtonsContext(HORTATimelineViewSpeedContext timelineViewSpeedContext);
	}
}
