using System;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Match;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Context
{
	public interface ICombatObject : IGadgetOwner
	{
		TeamKind Team { get; }

		bool IsCarryingBomb { get; }

		IPhysicalObject PhysicalObject { get; }

		ICombatController ModifierController { get; }

		bool IsPlayer { get; }

		bool IsBot { get; }

		bool NoHit { get; }

		Transform Transform { get; }

		CombatData Data { get; }

		CombatFeedback Feedback { get; }

		CombatAttributes Attributes { get; }

		CombatLayer Layer { get; }

		ICombatMovement CombatMovement { get; }

		ITurretMovement TurretMovement { get; }

		IPlayerData PlayerData { get; }

		CarInput CarInput { get; }

		IPlayerStats Stats { get; }

		void AddCollider(Collider collider);

		void RemoveCollider(Collider collider);

		IGadgetInput GetGadgetInput(GadgetSlot slot);
	}
}
