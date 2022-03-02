using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Loading
{
	public interface ILoadingVersusPlayerRankPresenter
	{
		IObservable<Unit> LoadRank(long playerId, ILoadingVersusPlayerRankView view);
	}
}
