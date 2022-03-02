using System;
using UnityEngine;

namespace HeavyMetalMachines.Core.Extensions
{
	public static class TimeSpanExtensions
	{
		public static TimeSpan ToTimeSpan(this float valueInSeconds)
		{
			float num = Mathf.Floor(valueInSeconds % 60f);
			float num2 = Mathf.Floor(valueInSeconds / 60f);
			return new TimeSpan(0, (int)num2, (int)num);
		}
	}
}
