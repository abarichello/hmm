using System;

namespace HeavyMetalMachines.Swordfish
{
	public interface IInitializeOnSwordfishConnected
	{
		void Initialize(SwordfishConnection connection);

		void Dispose();
	}
}
