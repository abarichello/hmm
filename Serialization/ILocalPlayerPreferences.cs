using System;

namespace HeavyMetalMachines.Serialization
{
	public interface ILocalPlayerPreferences
	{
		bool GetBoolean(string key, bool fallbackValue);

		void SetBoolean(string key, bool value);
	}
}
