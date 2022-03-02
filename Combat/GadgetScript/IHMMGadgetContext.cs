using System;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public interface IHMMGadgetContext : IGadgetContext, IGadgetInput
	{
		IGadgetOwner Owner { get; }

		int Id { get; }

		void SetLastBodyId(int objectId);

		ICombatObject Bomb { get; }

		IGameCamera GameCamera { get; }

		bool IsClient { get; }

		bool IsServer { get; }

		bool IsTest { get; }

		int CurrentTime { get; }

		Drawers HierarchyDrawers { get; }

		IScoreBoard ScoreBoard { get; }

		IStateMachine StateMachine { get; }

		void SetBodyDestructionTime(int bodyId, int time);

		bool TryGetBodyDestructionTime(int bodyId, out int time);

		ICombatObject GetCombatObject(int id);

		ICombatObject GetCombatObject(Component component);

		IIdentifiable GetIdentifiable(int id);

		bool IsCarryingBomb(ICombatObject combatObject);

		void CleanUp();

		IParameter<T> GetUIParameter<T>(string param);

		void SetLifebarVisibility(int combatObjectId, bool visible);

		void SetAttachedLifebarGroupVisibility(int lifebarOwnerId, int attachedId, bool visible);
	}
}
