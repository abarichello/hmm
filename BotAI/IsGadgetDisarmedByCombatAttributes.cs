using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.BotAI
{
	public class IsGadgetDisarmedByCombatAttributes : IIsGadgetDisarmed
	{
		public IsGadgetDisarmedByCombatAttributes(GadgetSlot slot, GadgetNatureKind nature)
		{
			this._nature = nature;
			this._slot = slot;
		}

		public bool Check(ICombatObject combat)
		{
			return combat.Attributes.IsGadgetDisarmed(this._slot, this._nature);
		}

		private GadgetSlot _slot;

		private GadgetNatureKind _nature;
	}
}
