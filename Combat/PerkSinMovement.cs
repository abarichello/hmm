using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(Rigidbody))]
	public class PerkSinMovement : PerkStraightMovement
	{
		public override void PerkInitialized()
		{
			base.PerkInitialized();
			this._toRad = 0.0174532924f;
			this._maxAngle = 180 * this.BounceCount;
			PerkSinMovement.SinDirection bounceDirection = this.BounceDirection;
			if (bounceDirection != PerkSinMovement.SinDirection.Right)
			{
				if (bounceDirection == PerkSinMovement.SinDirection.Left)
				{
					this._finalDirection = -base._trans.right;
				}
			}
			else
			{
				this._finalDirection = base._trans.right;
			}
		}

		public override Vector3 UpdatePosition()
		{
			Vector3 a = base.UpdatePosition();
			float num = Mathf.Lerp(0f, (float)this._maxAngle, base._deltaTimeRatio);
			float num2 = Mathf.Sin(this._toRad * num);
			Vector3 b = this._finalDirection * (num2 * this.BounceRadius);
			return a + b;
		}

		public int BounceCount = 3;

		public float BounceRadius = 10f;

		public PerkSinMovement.SinDirection BounceDirection;

		private Vector3 _finalDirection = Vector3.zero;

		private int _maxAngle;

		private float _toRad;

		public enum SinDirection
		{
			Right,
			Left
		}
	}
}
