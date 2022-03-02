using System;
using HeavyMetalMachines.Match.Infra;
using HeavyMetalMachines.Matches;

namespace HeavyMetalMachines.Match
{
	public class GetLocalPlayerNoviceTrialsTarget : IGetLocalPlayerNoviceTrialsTarget
	{
		public GetLocalPlayerNoviceTrialsTarget(IMatchHistoryProvider matchHistoryProvider)
		{
			this._matchHistoryProvider = matchHistoryProvider;
		}

		public int Get()
		{
			return this._matchHistoryProvider.GetInventoryBag().NoviceTrialsTarget;
		}

		private readonly IMatchHistoryProvider _matchHistoryProvider;
	}
}
