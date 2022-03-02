using System;

namespace HeavyMetalMachines.AI.Steering
{
	public static class SteeringMath
	{
		public static float GetParabolicMaximumIndex(float fminusone, float fzero, float fplusone)
		{
			if (fminusone == fplusone)
			{
				return 0f;
			}
			float num = (fplusone - fminusone) * 0.5f;
			float num2 = fplusone - num - fzero;
			return -num / (2f * num2);
		}
	}
}
