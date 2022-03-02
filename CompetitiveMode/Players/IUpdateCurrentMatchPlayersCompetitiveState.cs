using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public interface IUpdateCurrentMatchPlayersCompetitiveState
	{
		IObservable<Unit> Update();
	}
}
