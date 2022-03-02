using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteeringSubject
	{
		Rigidbody SubjectRigidbody { get; }

		Transform SubjectTransform { get; }

		ICombatObject Combat { get; }
	}
}
