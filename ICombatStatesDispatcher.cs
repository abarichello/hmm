using System;

namespace HeavyMetalMachines
{
	public interface ICombatStatesDispatcher
	{
		void SendData();

		void SendFullData(byte to);
	}
}
