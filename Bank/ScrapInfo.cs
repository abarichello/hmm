using System;

namespace HeavyMetalMachines.Bank
{
	[Serializable]
	public class ScrapInfo
	{
		public string Name;

		public int Value;

		public ScrapTeamDivisionKind TeamDivision;

		public bool Reliable;

		public bool UseBonus;
	}
}
