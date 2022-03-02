using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	internal class GoToDirectionParameters : BaseBehaviourParameters, IGoToDirectionParameters
	{
		public override SteeringBehaviourKind Kind
		{
			get
			{
				return SteeringBehaviourKind.GoToDirection;
			}
		}

		public float Force
		{
			get
			{
				return this._force;
			}
		}

		[SerializeField]
		private float _force = 1f;
	}
}
