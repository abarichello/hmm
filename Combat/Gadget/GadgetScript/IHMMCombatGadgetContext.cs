using System;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;

namespace HeavyMetalMachines.Combat.Gadget.GadgetScript
{
	public interface IHMMCombatGadgetContext : IHMMGadgetContext, IGadgetContext, IGadgetInput
	{
		GadgetSlot Slot { get; }

		IGadgetHudElement GadgetHudElement { get; }

		IHudEmotePresenter HudEmoteView { get; }

		IGadgetHudElement GetGadgetHudElement(GadgetSlot slot);

		IHudIconBar GetHudIconBar(ICombatObject combat);
	}
}
