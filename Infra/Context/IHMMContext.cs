using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.GameCamera;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IHMMContext
	{
		IGameCamera GameCamera { get; }

		ICombatObject Bomb { get; }

		ICombatObject[] RedTeam { get; }

		ICombatObject[] BlueTeam { get; }

		IArena Arena { get; }

		IScoreBoard ScoreBoard { get; }

		IStats Stats { get; }

		IGadgetHud GadgetHud { get; }

		bool IsClient { get; }

		bool IsServer { get; }

		bool IsTest { get; }

		IGameTime Clock { get; }

		ICombatObject GetCombatObject(int id);

		ICombatObject GetCombatObject(Component component);

		IIdentifiable GetIdentifiable(int id);

		bool IsCarryingBomb(ICombatObject combatObject);

		IHudIconBar GetHudIconBar(ICombatObject combatObject);

		IStateMachine StateMachine { get; }

		IHudEmotePresenter GetHudEmote(ICombatObject combatObject);
	}
}
