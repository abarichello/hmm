using System;
using HeavyMetalMachines.Match.Infra;
using Zenject;

namespace HeavyMetalMachines.Match
{
	public class GetMatchHistory : IGetMatchHistory
	{
		public bool HasStartedTraining()
		{
			return this._matchHistoryProvider.GetInventoryBag().HasStartedTraining;
		}

		public bool HasDoneTraining()
		{
			return this._matchHistoryProvider.GetInventoryBag().HasDoneTraining;
		}

		public bool HasStartedTutorial()
		{
			return this._matchHistoryProvider.GetInventoryBag().HasStartedTutorial;
		}

		public bool HasDoneTutorial()
		{
			return this._matchHistoryProvider.GetInventoryBag().HasDoneTutorial;
		}

		[Inject]
		private IMatchHistoryProvider _matchHistoryProvider;
	}
}
