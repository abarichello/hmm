using System;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public class ClearPlayersStateStorage : IClearPlayersStateStorage
	{
		public ClearPlayersStateStorage(IPlayersStateStorage playersStateStorage)
		{
			this._playersStateStorage = playersStateStorage;
		}

		public void Clear()
		{
			this._playersStateStorage.PlayersCompetitiveStateDictionary.Clear();
		}

		private readonly IPlayersStateStorage _playersStateStorage;
	}
}
