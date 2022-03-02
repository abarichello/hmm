using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IPhysicalObject
	{
		Vector3 Position { get; }

		Vector3 LastPosition { get; }

		Quaternion Rotation { get; }

		Vector3 Velocity { get; }

		Vector3 AngularVelocity { get; }

		float Mass { get; }

		Vector3 Up { get; }

		List<ICombatLink> Links { get; }

		void AddLink(ICombatLink newLink, bool force);

		ICombatLink GetLinkWithTag(string tag);

		void ResetImpulseAndVelocity();

		void LookTowards(Vector3 forward);

		void ForcePosition(Vector3 newPosition, bool includeLinks = true);

		void Push(Vector3 direction, bool pct, float magnitude = 0f, bool ignorePushReceived = false);

		void PauseSimulation();

		void UnpauseSimulation();
	}
}
