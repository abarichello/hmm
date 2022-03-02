using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public interface ILinkHook
	{
		Vector3 Velocity { get; }

		Vector3 Position { get; }

		IPhysicalObject Movement { get; }

		float Mass { get; }

		float MassStruggling { get; }

		Vector3 Normal { get; }

		float Distance { get; }

		Vector3 Pivot { get; }

		ICombatLink Link { get; }

		float PerpendicularSpeed { get; }

		Vector3 CombatPositionDiff { get; }

		void UpdateVelocity();

		void UpdatePivot(Vector3 pivot);

		void Clamp(float distanceFromPivot);

		void FreezeVelocity();
	}
}
