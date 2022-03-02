using System;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	public class CompetitiveRankingPlayerScrollerData
	{
		public string Position { get; set; }

		public string PlayerName { get; set; }

		public long PlayerId { get; set; }

		public long PlayerTag { get; set; }

		public int DivisionIndex { get; set; }

		public int SubdivisionIndex { get; set; }

		public string Score { get; set; }

		public bool IsLocalPlayer { get; set; }

		public string SubdivisionImageName { get; set; }

		public bool IsPsnUser { get; set; }

		public string UniversalId { get; set; }
	}
}
