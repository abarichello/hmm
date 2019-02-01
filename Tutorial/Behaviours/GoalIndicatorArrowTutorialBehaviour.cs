using System;
using HeavyMetalMachines.Car;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class GoalIndicatorArrowTutorialBehaviour : ActionTutorialBehaviourBase
	{
		protected override void ExecuteAction()
		{
			this._indicator = base.playerController.GetComponent<CarIndicatorArrow>();
			if (this._indicator != null)
			{
				if (this.BezierSpline != null)
				{
					this._active = true;
					this._bezierParam = 0f;
					this.BezierSpline.Evaluate(this._bezierParam, out this._targetPosition, out this._targetTangent, true);
				}
				else
				{
					this._indicator.UpdateBombObjective();
				}
			}
		}

		protected override void UpdateOnClient()
		{
			base.UpdateOnClient();
			if (this._active)
			{
				Vector3 position = base.playerController.Combat.Transform.position;
				this._bezierParam = Mathf.Clamp01(this._bezierParam + (this.TargetRadius - (this._targetPosition - position).magnitude) / this.BezierSpline.ArcLength);
				this.BezierSpline.Evaluate(this._bezierParam, out this._targetPosition, out this._targetTangent, true);
				this._indicator.UpdateObjective(this._targetPosition + 100f * (this._targetPosition - position), false);
			}
		}

		[Tooltip("If not null, the indicator arrow will point to the bezier curve, otherwise it will turn itself off or point to the bomb.")]
		public BezierSpline2D BezierSpline;

		[Tooltip("Distance to maintain the arrow target from the player.")]
		public float TargetRadius;

		private bool _active;

		private float _bezierParam;

		private float _bezierParamSpeed;

		private Vector3 _targetPosition;

		private Vector3 _targetTangent;

		private CarIndicatorArrow _indicator;
	}
}
