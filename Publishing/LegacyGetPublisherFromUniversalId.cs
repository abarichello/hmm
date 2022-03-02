using System;
using ClientAPI.Utils;

namespace HeavyMetalMachines.Publishing
{
	public class LegacyGetPublisherFromUniversalId : IGetPublisherFromUniversalId
	{
		public Publisher Get(string universalId)
		{
			if (string.IsNullOrEmpty(universalId))
			{
				return Publishers.Default;
			}
			if (UniversalIdUtil.IsPsnId(universalId))
			{
				return Publishers.Psn;
			}
			if (UniversalIdUtil.IsSteamId(universalId))
			{
				return Publishers.Steam;
			}
			if (UniversalIdUtil.IsMyGamesId(universalId))
			{
				return Publishers.MyGames;
			}
			if (UniversalIdUtil.IsXboxLiveId(universalId))
			{
				return Publishers.XboxLive;
			}
			if (UniversalIdUtil.IsSwordfishUniversalId(universalId))
			{
				return Publishers.Swordfish;
			}
			return Publishers.Default;
		}
	}
}
