using System;
using HeavyMetalMachines.Combat.Gadget;

namespace HeavyMetalMachines.Frontend
{
	public interface IHudEmotePresenter
	{
		void Initialize(int playerCarId);

		void Stop();

		void PlayEmote(GadgetSlot slot, int playerCarId);

		void Dispose();
	}
}
