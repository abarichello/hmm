using System;
using HeavyMetalMachines.Matches.DataTransferObjects;
using UniRx;

namespace HeavyMetalMachines.Match.Infra
{
	public interface IMarkMatchService
	{
		IObservable<Unit> MarkPlayerHasStarted(long playerId, MatchKind matchKind);

		IObservable<Unit> MarkPlayerHasDone(long playerId, MatchKind matchKind);
	}
}
