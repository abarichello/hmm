using System;
using HeavyMetalMachines.MatchMakingQueue.Infra;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class GetNoviceTrialsAmount : IGetNoviceTrialsAmount
	{
		public GetNoviceTrialsAmount(INoviceTrialsAmountProvider noviceTrialsAmountProvider)
		{
			this._noviceTrialsAmountProvider = noviceTrialsAmountProvider;
		}

		public int Get()
		{
			return this._noviceTrialsAmountProvider.GetNoviceTrialsAmount();
		}

		private readonly INoviceTrialsAmountProvider _noviceTrialsAmountProvider;
	}
}
