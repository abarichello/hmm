using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Context
{
	public interface ICombatObject
	{
		TeamKind Team { get; }

		bool IsCarryingBomb { get; }

		IPhysicalObject PhysicalObject { get; }

		IIdentifiable Identifiable { get; }

		ICombatController ModifierController { get; }

		Transform Transform { get; }

		CombatData Data { get; }

		CombatFeedback Feedback { get; }

		CombatAttributes Attributes { get; }

		ICombatMovement CombatMovement { get; }

		void BreakBombLink();
	}
}
