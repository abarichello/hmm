using System;
using UniRx;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassRewardView
	{
		void SetVisibility(bool isVisible);

		bool IsVisible();

		IObservable<Unit> ObserveHide();
	}
}
