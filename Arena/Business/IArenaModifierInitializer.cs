using System;

namespace HeavyMetalMachines.Arena.Business
{
	public interface IArenaModifierInitializer
	{
		void InitializeModifierApplier();

		void Dispose();
	}
}
