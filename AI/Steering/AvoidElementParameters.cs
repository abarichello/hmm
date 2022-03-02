using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	internal class AvoidElementParameters : BaseBehaviourParameters, IAvoidElementParameters
	{
		public override SteeringBehaviourKind Kind
		{
			get
			{
				return SteeringBehaviourKind.AvoidElement;
			}
		}

		public IAIElementKind ElementKind
		{
			get
			{
				return this._elementKind;
			}
		}

		public AnimationCurve HPToActivationCurve
		{
			get
			{
				return this._hpToActivationCurve;
			}
		}

		public bool OnlyWhenCarryingBomb
		{
			get
			{
				return this._onlyWhenCarryingBomb;
			}
		}

		public float DangerForce
		{
			get
			{
				return this._dangerForce;
			}
		}

		public float InterestForce
		{
			get
			{
				return this._interestForce;
			}
		}

		public float Range
		{
			get
			{
				return this._range;
			}
		}

		[Tooltip("Element Kind this behaviour avoids")]
		[SerializeField]
		private UnityAIElementKind _elementKind;

		[Tooltip("Hp percent (X) to activate (Y, 0 or 1) this behaviour")]
		[SerializeField]
		private AnimationCurve _hpToActivationCurve;

		[Tooltip("Behaviour is only active when bot is carrying the bomb")]
		[SerializeField]
		private bool _onlyWhenCarryingBomb;

		[Tooltip("Strength of this behaviour avoidance")]
		[SerializeField]
		private float _dangerForce;

		[Tooltip("Strength of this behaviour interest away from danger")]
		[SerializeField]
		private float _interestForce;

		[Tooltip("Range to look for elements")]
		[SerializeField]
		private float _range;
	}
}
