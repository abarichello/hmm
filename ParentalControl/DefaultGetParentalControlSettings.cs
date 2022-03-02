using System;

namespace HeavyMetalMachines.ParentalControl
{
	public class DefaultGetParentalControlSettings : IGetParentalControlSettings
	{
		public ParentalControlSettings Get()
		{
			return default(ParentalControlSettings);
		}
	}
}
