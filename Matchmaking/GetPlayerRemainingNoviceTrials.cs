using System;
using Hoplon.Logging;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.MatchMaking
{
	public class GetPlayerRemainingNoviceTrials : IGetPlayerRemainingNoviceTrials
	{
		public GetPlayerRemainingNoviceTrials(UserInfo userInfo, IConfigLoader config, ILogger<IGetPlayerRemainingNoviceTrials> logger)
		{
			this._userInfo = userInfo;
			this._config = config;
			this._logger = logger;
		}

		public int Get()
		{
			int intValue = this._config.GetIntValue(ConfigAccess.NoviceTrials);
			this._logger.DebugFormat("Getting Remaining NoviceTrials player skipping value with defaultConfig in {0}", new object[]
			{
				intValue
			});
			return Mathf.Max(this._userInfo.Bag.RemainingNoviceTrials - intValue, 0);
		}

		private readonly UserInfo _userInfo;

		private readonly IConfigLoader _config;

		private readonly ILogger<IGetPlayerRemainingNoviceTrials> _logger;
	}
}
