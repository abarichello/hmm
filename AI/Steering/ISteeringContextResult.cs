using System;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteeringContextResult
	{
		float[] InterestMap { get; }

		float[] DangerMap { get; }

		void Clear();
	}
}
