using System;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public interface IGadgetOwner
	{
		IHMMGadgetContext GetGadgetContext(int id);

		bool HasGadgetContext(int id);

		CDummy Dummy { get; }

		bool IsLocalPlayer { get; }

		IIdentifiable Identifiable { get; }

		ICombatObject GadgetCombatObject { get; }
	}
}
