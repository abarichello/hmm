using System;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Social.Groups.Business;
using Pocketverse;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class CheckNoviceQueueCondition : ICheckNoviceQueueCondition
	{
		public CheckNoviceQueueCondition(IConfigLoader config, IGroupStorage groupStorage, ILocalPlayerStorage playerStorage, IGetPlayerRemainingNoviceTrials getPlayerRemainingNoviceTrials)
		{
			this._config = config;
			this._groupStorage = groupStorage;
			this._playerStorage = playerStorage;
			this._getPlayerRemainingNoviceTrials = getPlayerRemainingNoviceTrials;
		}

		public bool ShouldGoToNoviceQueue()
		{
			if (this._config.GetBoolValue(ConfigAccess.NoviceTrialsABTest) && this._playerStorage.Player.PlayerId % 2L == 0L)
			{
				return false;
			}
			int num = this._getPlayerRemainingNoviceTrials.Get();
			return this._groupStorage.Group == null && num > 0;
		}

		private readonly IConfigLoader _config;

		private readonly IGroupStorage _groupStorage;

		private readonly ILocalPlayerStorage _playerStorage;

		private readonly IGetPlayerRemainingNoviceTrials _getPlayerRemainingNoviceTrials;
	}
}
