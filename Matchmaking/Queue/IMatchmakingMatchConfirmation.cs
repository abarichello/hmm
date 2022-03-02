using System;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public interface IMatchmakingMatchConfirmation
	{
		IObservable<bool> ConfirmMatch();
	}
}
