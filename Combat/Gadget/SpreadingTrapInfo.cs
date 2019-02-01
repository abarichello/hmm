using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class SpreadingTrapInfo : DropperGadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(SpreadingTrap);
		}

		public ModifierInfo[] PrimaryModifiers;

		public FXInfo PrimaryEffect;

		public float PrimaryRadius;

		public ModifierInfo[] SecondaryModifiers;

		public FXInfo SecondaryEffect;

		public float SecondaryRadius;

		public float SecondaryDropTime;

		public string SecondaryDropTimeUpgrade;

		public float SecondaryDropDelay;
	}
}
