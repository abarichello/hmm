using System;

namespace HeavyMetalMachines.Durango
{
	public class DurangoKeyboardLayoutDetection : IKeyboardLayoutDetection
	{
		public int GetKeyboardLayoutId()
		{
			return 0;
		}

		public LanguageCode GetKeyboardLanguageCode(int layoutId)
		{
			return LanguageCode.EN;
		}

		public string GetKeyboardLayoutName()
		{
			return "Unknown";
		}
	}
}
