using System;
using Hoplon.SensorSystem;

namespace HeavyMetalMachines.Infra.Counselor
{
	public class CounselorAdvice : Sensor
	{
		private int _uses;

		public byte TargetPlayerAddress;

		public int ConfigIndex;
	}
}
