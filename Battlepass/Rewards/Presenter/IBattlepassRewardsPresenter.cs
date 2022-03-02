using System;
using UniRx;

namespace HeavyMetalMachines.Battlepass.Rewards.Presenter
{
	public interface IBattlepassRewardsPresenter
	{
		IObservable<bool> TryToOpenRewardsToClaim();
	}
}
