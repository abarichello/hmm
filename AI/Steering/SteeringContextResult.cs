using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public class SteeringContextResult : ISteeringContextResult
	{
		public SteeringContextResult(ISteeringContextParameters parameters)
		{
			this._parameters = parameters;
			this._interestMap = new float[this._parameters.DirectionCount];
			this._dangerMap = new float[this._parameters.DirectionCount];
		}

		public ISteeringContextParameters Parameters
		{
			get
			{
				return this._parameters;
			}
		}

		public float[] InterestMap
		{
			get
			{
				return this._interestMap;
			}
		}

		public float[] DangerMap
		{
			get
			{
				return this._dangerMap;
			}
		}

		public void Clear()
		{
			for (int i = 0; i < this._parameters.DirectionCount; i++)
			{
				this._interestMap[i] = 0f;
				this._dangerMap[i] = 0f;
			}
		}

		public void MoveTowards(ISteeringContextResult target, float delta)
		{
			for (int i = 0; i < this._parameters.DirectionCount; i++)
			{
				this._interestMap[i] = Mathf.MoveTowards(this._interestMap[i], target.InterestMap[i], delta);
				this._dangerMap[i] = Mathf.MoveTowards(this._dangerMap[i], target.DangerMap[i], delta);
			}
		}

		private readonly ISteeringContextParameters _parameters;

		private readonly float[] _interestMap;

		private readonly float[] _dangerMap;
	}
}
