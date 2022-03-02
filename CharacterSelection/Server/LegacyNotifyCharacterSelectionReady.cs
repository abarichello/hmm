using System;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.CharacterSelection.Server
{
	public class LegacyNotifyCharacterSelectionReady : INotifyCharacterSelectionReady
	{
		public LegacyNotifyCharacterSelectionReady(MatchData matchData, ServerInfo serverInfo)
		{
			this._matchData = matchData;
			this._serverInfo = serverInfo;
		}

		public void Notify()
		{
			this._matchData.State = MatchData.MatchState.CharacterPick;
			this._serverInfo.SpreadInfo();
		}

		private readonly MatchData _matchData;

		private readonly ServerInfo _serverInfo;
	}
}
