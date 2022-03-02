using System;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public interface IMatchmakingTrainingQueueJoin
	{
		IObservable<Unit> JoinTraining(string config);
	}
}
