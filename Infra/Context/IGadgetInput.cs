using System;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IGadgetInput
	{
		void ForcePressed();

		void ForceReleased();
	}
}
