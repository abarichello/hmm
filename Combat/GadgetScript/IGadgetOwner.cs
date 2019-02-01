using System;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public interface IGadgetOwner
	{
		IHMMGadgetContext GetGadgetContext(int id);

		bool HasGadgetContext(int id);

		CDummy Dummy { get; }
	}
}
