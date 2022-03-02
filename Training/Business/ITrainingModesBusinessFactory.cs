using System;
using HeavyMetalMachines.Matchmaking.Queue;

namespace HeavyMetalMachines.Training.Business
{
	public interface ITrainingModesBusinessFactory
	{
		IMatchmakingTrainingQueueJoin CreateJoinCustomTraining();
	}
}
