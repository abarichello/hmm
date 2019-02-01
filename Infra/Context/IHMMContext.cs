using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IHMMContext
	{
		ICombatObject Bomb { get; }

		ICombatObject[] RedTeam { get; }

		ICombatObject[] BlueTeam { get; }

		IArena Arena { get; }

		IScoreBoard ScoreBoard { get; }

		IStats Stats { get; }

		bool IsClient { get; }

		bool IsServer { get; }

		IGameTime Clock { get; }

		ICombatObject GetCombatObject(int id);

		ICombatObject GetCombatObject(Component component);

		IIdentifiable GetIdentifiable(int id);

		bool IsCarryingBomb(ICombatObject combatObject);
	}
}
