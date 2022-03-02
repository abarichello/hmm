using System;

namespace HeavyMetalMachines.ParentalControl
{
	public class DurangoGetParentalControlSettings : IGetParentalControlSettings
	{
		public ParentalControlSettings Get()
		{
			ParentalControlSettings result = default(ParentalControlSettings);
			result.MinimumAgeRequiredToPlay = 13;
			return result;
		}
	}
}
