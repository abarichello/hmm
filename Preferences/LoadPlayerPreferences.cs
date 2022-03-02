using System;

namespace HeavyMetalMachines.Preferences
{
	public class LoadPlayerPreferences : ILoadPlayerPreferences
	{
		public LoadPlayerPreferences(IHMMPlayerPrefs playerPrefs)
		{
			this._playerPrefs = playerPrefs;
		}

		public void Load()
		{
			this._playerPrefs.Load();
		}

		private readonly IHMMPlayerPrefs _playerPrefs;
	}
}
