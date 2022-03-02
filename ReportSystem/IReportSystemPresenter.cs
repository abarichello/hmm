using System;
using HeavyMetalMachines.Match;
using UniRx;

namespace HeavymetalMachines.ReportSystem
{
	public interface IReportSystemPresenter
	{
		IObservable<Unit> Initialize();

		IObservable<Unit> Dispose();

		void Show(PlayerData playerData);

		void Hide();

		IObservable<Unit> ObserveHide();

		bool Visible { get; }

		bool IsPlayerReported(long playerId);
	}
}
