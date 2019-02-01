using System;

namespace HeavyMetalMachines.Match
{
	public class MockMatchData : MatchData
	{
		public override bool LevelIsTutorial()
		{
			return this.IsLevelTutorial;
		}

		public bool IsLevelTutorial;
	}
}
