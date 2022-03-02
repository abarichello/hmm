using System;

namespace HeavyMetalMachines
{
	public interface IKeyboardLayoutDetection
	{
		int GetKeyboardLayoutId();

		LanguageCode GetKeyboardLanguageCode(int layoutId);

		string GetKeyboardLayoutName();
	}
}
