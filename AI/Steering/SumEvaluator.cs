using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public class SumEvaluator : ISteeringContextMapEvaluator
	{
		public SumEvaluator(ISteeringContextParameters parameters)
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
				this._resultMap[i] = result.InterestMap[i] - result.DangerMap[i];
				if (this._resultMap[i] > num)
				{
					num = this._resultMap[i];
					num2 = i;
				}
			}
			int num3 = (num2 + this._parameters.DirectionCount - 1) % this._parameters.DirectionCount;
			int num4 = (num2 + 1) % this._parameters.DirectionCount;
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
