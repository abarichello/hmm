using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Matchmaking.Queue;
using Hoplon.Logging;

namespace HeavyMetalMachines.Match
{
	public class AutoMatch : IAutoMatch
	{
		public AutoMatch(ICheckNoviceQueueCondition noviceChecker, IAutoMatchStorage storage, ILogger<AutoMatch> logger, ICheckConsolesQueueCondition checkConsolesQueueCondition)
		{
			this._noviceChecker = noviceChecker;
			this._storage = storage;
			this.Log = logger;
			this._checkConsolesQueueCondition = checkConsolesQueueCondition;
		}

		public GameModeTabs GetGameModeTab()
		{
			this.Log.DebugFormat("AutoMatch _lastMatchKind: {0}", new object[]
			{
				this._storage.LastMatchKind
			});
			switch (this._storage.LastMatchKind)
			{
			case 1:
				return GameModeTabs.CoopVsBots;
			case 3:
				return this.RankedQueue();
			}
			return this.NormalQueue();
		}

		private GameModeTabs NormalQueue()
		{
			if (this._noviceChecker.ShouldGoToNoviceQueue())
			{
				return GameModeTabs.Novice;
			}
			if (this._checkConsolesQueueCondition.Check())
			{
				return Platform.Current.GetExclusiveCasualQueueName();
			}
			return GameModeTabs.Normal;
		}

		private GameModeTabs RankedQueue()
		{
			if (this._checkConsolesQueueCondition.Check())
			{
				return Platform.Current.GetExclusiveRankedQueueName();
			}
			return GameModeTabs.Ranked;
		}

		private readonly ICheckNoviceQueueCondition _noviceChecker;

		private readonly IAutoMatchStorage _storage;

		private readonly ILogger<AutoMatch> Log;

		private readonly ICheckConsolesQueueCondition _checkConsolesQueueCondition;
	}
}
