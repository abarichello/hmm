using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.Horta.View
{
	public interface IHORTATimelinePresenter : IPresenter
	{
		IObservable<Unit> Initialize(ITimelineController timelineController);

		void ToggleVisibility();
	}
}
