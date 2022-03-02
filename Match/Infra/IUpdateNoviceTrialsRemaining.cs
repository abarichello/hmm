using System;
using UniRx;

namespace HeavyMetalMachines.Match.Infra
{
	public interface IUpdateNoviceTrialsRemaining
	{
		IObservable<Unit> Update(long playerId);
	}
}
