using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	public class TimelineConfiguration : GameHubScriptableObject
	{
		public double SmoothingFactor
		{
			get
			{
				return Math.Pow(this._smoothingFactor, this.power);
			}
		}

		public SmoothClock SmoothClockInstance
		{
			get
			{
				SmoothClock result;
				if ((result = this._instance) == null)
				{
					result = (this._instance = new SmoothClock(this));
				}
				return result;
			}
		}

		[Range(0f, 0.2f)]
		public double ClockDelay = 0.06;

		[Range(0f, 1f)]
		[SerializeField]
		[Tooltip("The higher the value, the smoother and slower the movement will be")]
		private double _smoothingFactor = 0.5;

		private double power = 13.0;

		private SmoothClock _instance;
	}
}
