using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class UpgradeInfo
	{
		public string Name;

		public string Tag;

		public string[] LevelNames;

		public int[] LevelPrices;

		public ExternalUpgrade[] ExternalUpgrades;
	}
}
