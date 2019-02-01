using System;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public class Control
	{
		public ControlAction Action;

		public string PrimaryKey;

		public string PrimaryModifier;

		public string SecondaryKey;

		public string SecondaryModifier;
	}
}
