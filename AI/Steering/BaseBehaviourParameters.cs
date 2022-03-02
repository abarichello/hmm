using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public abstract class BaseBehaviourParameters : ScriptableObject, ISteeringBehaviourParameters
	{
		public abstract SteeringBehaviourKind Kind { get; }
	}
}
