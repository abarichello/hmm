using System;

namespace HeavyMetalMachines
{
	public interface IMatchPlayersDispatcher
	{
		void UpdatePlayers();

		void UpdatePlayer(int objId);

		void SendPlayers(byte to);
	}
}
