using System;

namespace HeavyMetalMachines.Combat
{
	public interface IScoreboardDispatcher
	{
		void Send();

		void SendFull(byte to);
	}
}
