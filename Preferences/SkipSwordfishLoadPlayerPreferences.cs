using System;

namespace HeavyMetalMachines.Preferences
{
	public class SkipSwordfishLoadPlayerPreferences : ILoadPlayerPreferences
	{
		public SkipSwordfishLoadPlayerPreferences(IHMMPlayerPrefs playerPrefs)
		{
			this._playerPrefs = playerPrefs;
		}

		public void Load()
		{
			this._playerPrefs.SkipSwordfishLoad();
		}

		private readonly IHMMPlayerPrefs _playerPrefs;
	}
}
