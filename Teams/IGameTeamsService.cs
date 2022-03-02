using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Teams
{
	public interface IGameTeamsService
	{
		string GetPlayerTag(string universalId);

		Team GetPlayerTeam(string universalId);

		Team GetGroupTeam(TeamKind group);
	}
}
