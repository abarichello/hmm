using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public interface IUpdatePlayerState
	{
		IObservable<Unit> Update(long playerId);

		IObservable<Unit> Update(long[] playersIds);
	}
}
