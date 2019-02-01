using System;
using System.Text;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;

namespace HeavyMetalMachines
{
	public struct ModifierEvent
	{
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Mod:[Id=");
			stringBuilder.Append(this.ObjId);
			stringBuilder.Append(" Other=");
			stringBuilder.Append(this.OtherId);
			stringBuilder.Append(" Effect=");
			stringBuilder.Append(this.Effect);
			stringBuilder.Append(" Amount=");
			stringBuilder.Append(this.Amount);
			stringBuilder.Append(" Slot=");
			stringBuilder.Append(this.Slot);
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}

		public int ObjId;

		public int OtherId;

		public GadgetSlot Slot;

		public EffectKind Effect;

		public float Amount;
	}
}
