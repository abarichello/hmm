using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public abstract class DropperGadgetInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(DropperGadget);
		}

		public float DropTime;

		public string DropTimeUpgrade;

		public float DropDistance;

		public int ImmunityMillis;

		public CDummy.DummyKind Dummy;
	}
}
