using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IPlayerData
	{
		User User { get; }

		CharacterInfo Character { get; }

		int PlayerCarId { get; }

		TeamKind Team { get; }

		bool IsBot { get; }
	}
}
