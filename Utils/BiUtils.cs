using System;
using Gamesight;
using Pocketverse;

namespace HeavyMetalMachines.Utils
{
	public class BiUtils : IBiUtils
	{
		public void MarkConversion()
		{
			HMMHub hub = GameHubBehaviour.Hub;
			IConfigLoader config = hub.Config;
			string universalID = hub.User.UserSF.UniversalID;
			if (config.GetBoolValue(ConfigAccess.EnableRedShell))
			{
				GamesightTrack.SetApiKey("e2adebeff1cf8431da5b3b3d4a78e2a3");
				GamesightTrack.SetUserId(universalID);
				GamesightTrack.MarkConversion();
			}
			if (config.GetBoolValue(ConfigAccess.EnableHoplonTT))
			{
				string value = config.GetValue(ConfigAccess.HoplonTTUrl);
				HoplonTrackingTool.MarkConversion(value, universalID);
			}
			if (config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				return;
			}
		}
	}
}
