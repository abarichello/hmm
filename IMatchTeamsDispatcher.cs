using System;

namespace HeavyMetalMachines
{
	public interface IMatchTeamsDispatcher
	{
		void SendTeams(byte to);

		void UpdateTeams();
	}
}
