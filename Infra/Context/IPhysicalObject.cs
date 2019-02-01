using System;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IPhysicalObject
	{
		Vector3 Position { get; }

		Quaternion Rotation { get; }

		Vector3 Velocity { get; }

		Vector3 AngularVelocity { get; }

		void ResetImpulseAndVelocity();
	}
}
