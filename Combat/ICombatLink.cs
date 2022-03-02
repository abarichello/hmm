using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public interface ICombatLink
	{
		bool IsBroken { get; }

		string Tag { get; }

		ILinkHook Point1 { get; }

		ILinkHook Point2 { get; }

		float Compression { get; }

		float Tension { get; }

		float TensionBreakForce { get; }

		bool ClampOut { get; }

		bool ClampIn { get; }

		bool IsFixedPivot { get; }

		float Range { get; }

		Vector3 PositionDiff { get; }

		bool IsEnabled { get; }

		void Break();

		void SetLengthOffset(float offset);

		void Update(IPhysicalObject updater);

		void FreezeVelocity(IPhysicalObject hookMovement);

		bool HasSameHooks(ICombatLink other);

		float GetOtherPointMass(ILinkHook point, Vector3 velocity);

		float GetPointMass(ILinkHook point, Vector3 velocity);

		void Disable();

		void Enable();

		event LinkUpdatedEventHandler OnLinkUpdated;

		event LinkUpdatedEventHandler OnLinkBroken;
	}
}
