using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines
{
	public interface IMatchTeams : IKeyStateParser
	{
		string GetPlayerTag(string universalId);

		Team GetPlayerTeam(string universalId);

		Team GetGroupTeam(TeamKind group);
	}
}
