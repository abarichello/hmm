using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public class DiscardDangerEvaluator : ISteeringContextMapEvaluator
	{
		public DiscardDangerEvaluator(ISteeringContextParameters parameters)
		{
			this._parameters = parameters;
			this._resultMap = new float[this._parameters.DirectionCount];
		}

		public float[] LastResult
		{
			get
			{
				return this._resultMap;
			}
		}

		public bool EvaluateDirection(ISteeringContextResult result, out Vector3 finalDirection)
		{
			float num = float.MinValue;
			int num2 = -1;
			for (int i = 0; i < this._parameters.DirectionCount; i++)
			{
				this._resultMap[i] = ((result.DangerMap[i] <= 0f) ? result.InterestMap[i] : 0f);
				if (this._resultMap[i] > num)
				{
					num = this._resultMap[i];
					num2 = i;
				}
			}
			int num3 = (num2 + this._parameters.DirectionCount - 1) % this._parameters.DirectionCount;
			int num4 = (num2 + 1) % this._parameters.DirectionCount;
			if (Mathf.Approximately(this._resultMap[num2] * 2f - this._resultMap[num3] - this._resultMap[num4], 0f))
			{
				finalDirection = Vector3.zero;
				return false;
			}
			float num5 = SteeringMath.GetParabolicMaximumIndex(this._resultMap[num3], this._resultMap[num2], this._resultMap[num4]);
			num5 += (float)num2;
			float num6 = num5 * this._parameters.DegreesPerDirection;
			finalDirection = Quaternion.AngleAxis(num6, Vector3.up) * Vector3.forward;
			return true;
		}

		private readonly ISteeringContextParameters _parameters;

		private readonly float[] _resultMap;
	}
}
