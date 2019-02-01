using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Match
{
	public class PlayersLoadingProgress
	{
		public PlayersLoadingProgress()
		{
			this._playerIdAndProgressDictionary = new Dictionary<long, float>(10);
		}

		public void UpdatePlayerProgress(long playerId, float progress)
		{
			this._playerIdAndProgressDictionary[playerId] = progress;
		}

		public float GetPlayerProgress(long playerId)
		{
			float result;
			if (this._playerIdAndProgressDictionary.TryGetValue(playerId, out result))
			{
				return result;
			}
			return 0f;
		}

		private readonly Dictionary<long, float> _playerIdAndProgressDictionary;
	}
}
