using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class FakeMatchTeams : IMatchTeams, IKeyStateParser
	{
		public StateType Type { get; private set; }

		public void Update(BitStream data)
		{
			throw new NotImplementedException();
		}

		public string GetPlayerTag(string universalId)
		{
			return "FSC";
		}

		public Team GetPlayerTeam(string universalId)
		{
			throw new NotImplementedException();
		}

		public Team GetGroupTeam(TeamKind group)
		{
			throw new NotImplementedException();
		}
	}
}
