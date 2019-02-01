using System;
using Gamesight;

namespace HeavyMetalMachines.Utils
{
	public class BiUtils
	{
		public static void MarkConversion(HMMHub hub)
		{
			if (hub.Config.GetBoolValue(ConfigAccess.EnableRedShell))
			{
				GamesightTrack.SetApiKey("e2adebeff1cf8431da5b3b3d4a78e2a3");
				GamesightTrack.SetUserId(hub.User.UserSF.UniversalID);
				GamesightTrack.MarkConversion();
			}
			if (hub.Config.GetBoolValue(ConfigAccess.EnableHoplonTT))
			{
				string value = hub.Config.GetValue(ConfigAccess.HoplonTTUrl);
				HoplonTrackingTool.MarkConversion(value, hub.User.UserSF.UniversalID);
			}
		}
	}
}
