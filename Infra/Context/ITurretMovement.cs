using System;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Context
{
	public interface ITurretMovement
	{
		float TurretAngle { get; set; }

		Vector3 TurretDirection { get; }

		Vector3 GlobalTurretDirection { get; }

		IIdentifiable Identifiable { get; }

		void ResetRotation();
	}
}
