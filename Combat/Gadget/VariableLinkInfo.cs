using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class VariableLinkInfo : BasicLinkInfo
	{
		public override Type GadgetType()
		{
			return typeof(VariableLink);
		}

		public FXInfo OnRangeModificationStartEffect;

		public float MaxRangeModification;

		public float RangeModificationPerSecond;

		public float AditionalTensionBreakForce;

		public int LifeTimeMillisToRangeModStart;

		public bool DestroyLinkOnMaxRange;
	}
}
