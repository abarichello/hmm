using System;
using UnityEngine;

namespace HeavyMetalMachines.Serialization
{
	public class PlayerPrefsLocalPlayerPreferences : ILocalPlayerPreferences
	{
		public bool GetBoolean(string key, bool fallbackValue)
		{
			if (!PlayerPrefs.HasKey(key))
			{
				return fallbackValue;
			}
			return PlayerPrefs.GetInt(key) > 0;
		}

		public void SetBoolean(string key, bool value)
		{
			PlayerPrefs.SetInt(key, (!value) ? 0 : 1);
			PlayerPrefs.Save();
		}
	}
}
