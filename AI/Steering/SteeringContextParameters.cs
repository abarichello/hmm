using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	[Serializable]
	public class SteeringContextParameters : ISteeringContextParameters
	{
		public int DirectionCount
		{
			get
			{
				return this._directionCount;
			}
		}

		public float DegreesPerDirection
		{
			get
			{
				return 360f / (float)this._directionCount;
			}
		}

		public float DirectionsPerDegree
		{
			get
			{
				return (float)this._directionCount / 360f;
			}
		}

		public int GetOppositeDirectionIndex(int index)
		{
			return (index + this._directionCount / 2) % this._directionCount;
		}

		public Vector3 GetDirection(int index)
		{
			float num = this.DegreesPerDirection * (float)index;
			return Quaternion.AngleAxis(num, Vector3.up) * Vector3.forward;
		}

		public float RoundToIndex(Vector3 direction)
		{
			float num = Vector3.Angle(Vector3.forward, direction);
			if (direction.x < 0f)
			{
				num = 360f - num;
			}
			float num2 = Mathf.Round(num * this.DirectionsPerDegree * 2f) / 2f;
			return num2 % (float)this.DirectionCount;
		}

		public int GetMirroredDirection(int index, float axisIndex)
		{
			float num = axisIndex - (float)index;
			float num2 = axisIndex + num;
			return Mathf.RoundToInt(num2 + (float)this.DirectionCount) % this.DirectionCount;
		}

		[SerializeField]
		private int _directionCount;
	}
}
