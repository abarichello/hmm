using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public interface IHMMGadgetContext : IGadgetContext, IParameterContext
	{
		int OwnerId { get; }

		IGadgetOwner Owner { get; }

		int Id { get; }

		void SetLastBodyId(IEventContext ev);

		ICombatObject Bomb { get; }

		bool IsClient { get; }

		bool IsServer { get; }

		bool IsLocalPlayer { get; }

		int CurrentTime { get; }

		Drawers HierarchyDrawers { get; }

		ICombatObject GetCombatObject(int id);

		ICombatObject GetCombatObject(Component component);

		IIdentifiable GetIdentifiable(int id);

		bool IsCarryingBomb(ICombatObject combatObject);

		IParameter<T> GetUIParameter<T>(string param);

		void SetLifebarVisibility(int combatObjectId, bool visible);

		void SetAttachedLifebarGroupVisibility(int lifebarOwnerId, int attachedId, bool visible);
	}
}
