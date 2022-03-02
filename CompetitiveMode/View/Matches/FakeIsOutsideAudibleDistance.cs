using System;
using HeavyMetalMachines.Audio;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class FakeIsOutsideAudibleDistance : IIsOutsideAudibleDistance
	{
		public bool CheckSqrDistance(float sqrDistance, float targetXPos, float targetYPos, float targetZPos)
		{
			return false;
		}
	}
}
