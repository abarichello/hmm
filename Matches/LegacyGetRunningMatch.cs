using System;
using Hoplon;
using Pocketverse;

namespace HeavyMetalMachines.Matches
{
	public class LegacyGetRunningMatch : IGetRunningMatch
	{
		public LegacyGetRunningMatch(UserInfo userInfo, IConfigLoader config)
		{
			this._userInfo = userInfo;
			this._config = config;
		}

		public Maybe<string> GetMatchId()
		{
			if (!this.HasRunningMatch())
			{
				return Maybe<string>.None;
			}
			return this._userInfo.Bag.CurrentMatchId;
		}

		public bool HasRunningMatch()
		{
			return !string.IsNullOrEmpty(this._userInfo.Bag.CurrentMatchId);
		}

		private readonly UserInfo _userInfo;

		private readonly IConfigLoader _config;
	}
}
