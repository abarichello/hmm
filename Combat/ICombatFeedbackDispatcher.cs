using System;

namespace HeavyMetalMachines.Combat
{
	public interface ICombatFeedbackDispatcher
	{
		void SendData();

		void SendFullData(byte to);
	}
}
