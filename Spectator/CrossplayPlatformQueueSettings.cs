using System;

namespace HeavyMetalMachines.Spectator
{
	[Serializable]
	public class CrossplayPlatformQueueSettings
	{
		public CrossplayPlatformQueueSettings(bool shouldNormaQueueBeVisible, bool shouldRankedQueueBeVisible, bool shouldPsnNormalQueueBeVisible, bool shouldPsnRankedQueueBeVisible, bool shouldXboxNormalQueueBeVisible, bool shouldXboxRankedQueueBeVisible)
		{
			this.Visibility["Normal"] = shouldNormaQueueBeVisible;
			this.Visibility["Ranked"] = shouldRankedQueueBeVisible;
			this.Visibility["NormalPSN"] = shouldPsnNormalQueueBeVisible;
			this.Visibility["RankedPSN"] = shouldPsnRankedQueueBeVisible;
			this.Visibility["NormalXboxLive"] = shouldXboxNormalQueueBeVisible;
			this.Visibility["RankedXboxLive"] = shouldXboxRankedQueueBeVisible;
		}

		public CrossplayPlatformQueueSettings.VisibilitySerializableDictionary Visibility = new CrossplayPlatformQueueSettings.VisibilitySerializableDictionary
		{
			{
				"Normal",
				false
			},
			{
				"Ranked",
				false
			},
			{
				"NormalPSN",
				false
			},
			{
				"RankedPSN",
				false
			},
			{
				"NormalXboxLive",
				false
			},
			{
				"RankedXboxLive",
				false
			}
		};

		[Serializable]
		public class VisibilitySerializableDictionary : SerializableDictionary<string, bool>
		{
		}
	}
}
