using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public interface IGetLockedFromCompetitiveGroupMembers
	{
		IObservable<long[]> Get();
	}
}
