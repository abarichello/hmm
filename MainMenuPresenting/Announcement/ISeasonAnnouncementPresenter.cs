using System;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.MainMenuPresenting.Announcement
{
	public interface ISeasonAnnouncementPresenter : IPresenter
	{
		IObservable<Unit> OnOpenBattlepassRequested { get; }

		bool ShouldShow();
	}
}
