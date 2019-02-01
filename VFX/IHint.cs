using System;

namespace HeavyMetalMachines.VFX
{
	public interface IHint
	{
		void ActivateHint();

		void UpdateIndex(long index);
	}
}
