using System;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteeringContextFormater
	{
		void FormatInterests(float[] interestMap);

		void FormatDangers(float[] dangerMap);

		void FormatResults(float[] resultMap);

		void FormatBehaviour(ISteeringBehaviour behaviour, float[] interestMap, float[] dangerMap);
	}
}
