using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public class SteeringSubject : ISteeringSubject
	{
		public Rigidbody SubjectRigidbody { get; set; }

		public Transform SubjectTransform { get; set; }

		public ICombatObject Combat { get; set; }
	}
}
