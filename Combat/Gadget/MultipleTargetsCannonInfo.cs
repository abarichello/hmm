using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class MultipleTargetsCannonInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(MultipleTargetsCannon);
		}

		public MultipleTargetsCannonInfo.TargetsKind Targets;

		public string TargetsUpgrade;

		public enum TargetsKind
		{
			None,
			All,
			Allieds,
			Enemies
		}
	}
}
