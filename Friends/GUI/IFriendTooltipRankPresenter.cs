using System;
using UniRx;

namespace HeavyMetalMachines.Friends.GUI
{
	public interface IFriendTooltipRankPresenter
	{
		IObservable<Unit> LoadRank(long playerId);
	}
}
