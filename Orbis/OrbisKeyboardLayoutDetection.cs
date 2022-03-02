using System;

namespace HeavyMetalMachines.Orbis
{
	internal class OrbisKeyboardLayoutDetection : IKeyboardLayoutDetection
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
