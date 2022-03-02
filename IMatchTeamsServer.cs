using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines
{
	public interface IMatchTeamsServer
	{
		void AddTeam(string universalId, Team team);

		void SetGroupTeam(TeamKind group, Team team);
	}
}
