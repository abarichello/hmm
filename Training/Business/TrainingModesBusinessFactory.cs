using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Matchmaking.Queue;
using Hoplon.Logging;
using Zenject;

namespace HeavyMetalMachines.Training.Business
{
	public class TrainingModesBusinessFactory : ITrainingModesBusinessFactory
	{
		public IMatchmakingTrainingQueueJoin CreateJoinCustomTraining()
		{
			MatchmakingCustomTrainingQueueJoin matchmakingCustomTrainingQueueJoin = new MatchmakingCustomTrainingQueueJoin(this._logger);
			this._container.Inject(matchmakingCustomTrainingQueueJoin);
			return matchmakingCustomTrainingQueueJoin;
		}

		[InjectOnClient]
		private DiContainer _container;

		[Inject]
		private ILogger<MatchmakingCustomTrainingQueueJoin> _logger;
	}
}
