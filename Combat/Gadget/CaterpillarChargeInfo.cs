using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class CaterpillarChargeInfo : BasicLinkInfo
	{
		public override Type GadgetType()
		{
			return typeof(CaterpillarCharge);
		}

		public FXInfo GrabbedEffect;

		public ModifierInfo[] GrabbedModifiers;

		public FXInfo GrabbedEndByLifetimeEffect;

		public ModifierInfo[] GrabbedEndByLifetimeModifiers;

		public FXInfo GrabbedEndByCollisionEffect;

		public ModifierInfo[] GrabbedEndByCollisionModifiers;
	}
}
