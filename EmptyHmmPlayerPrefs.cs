using System;

namespace HeavyMetalMachines
{
	public class EmptyHmmPlayerPrefs : IHMMPlayerPrefs
	{
		public void Load()
		{
			throw new NotImplementedException();
		}

		public void SkipSwordfishLoad()
		{
			throw new NotImplementedException();
		}

		public void ExecOnceOnPrefsLoaded(Action action)
		{
			action();
		}

		public bool IsLoaded()
		{
			return true;
		}

		public bool HasKey(string key)
		{
			return false;
		}

		public string GetString(string key, string defaultValue = null)
		{
			return defaultValue;
		}

		public void SetString(string key, string value)
		{
		}

		public void Save()
		{
		}

		public void SaveNow()
		{
		}

		public float GetFloat(string key, float defaultValue = 0f)
		{
			return defaultValue;
		}

		public void SetFloat(string key, float value)
		{
		}

		public int GetInt(string key, int defaultValue = 0)
		{
			return defaultValue;
		}

		public void SetInt(string key, int value)
		{
		}
	}
}
