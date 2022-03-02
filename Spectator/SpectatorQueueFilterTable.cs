using System;
using UnityEngine;

namespace HeavyMetalMachines.Spectator
{
	[CreateAssetMenu(menuName = "Scriptable Object/Spectator/Queue Filter Table")]
	public class SpectatorQueueFilterTable : ScriptableObject
	{
		public CrossplayPlatformQueueSettingsSerializableDictionary Registry = new CrossplayPlatformQueueSettingsSerializableDictionary
		{
			{
				new CrossplayPlatformQueueKey(0, true),
				new CrossplayPlatformQueueSettings(true, true, false, false, false, false)
			},
			{
				new CrossplayPlatformQueueKey(1, true),
				new CrossplayPlatformQueueSettings(true, true, false, false, false, false)
			},
			{
				new CrossplayPlatformQueueKey(1, false),
				new CrossplayPlatformQueueSettings(false, false, true, true, false, false)
			},
			{
				new CrossplayPlatformQueueKey(2, true),
				new CrossplayPlatformQueueSettings(true, true, false, false, false, false)
			},
			{
				new CrossplayPlatformQueueKey(2, false),
				new CrossplayPlatformQueueSettings(false, false, false, false, true, true)
			}
		};

		public string[] Queues = new string[]
		{
			"Normal",
			"Ranked",
			"NormalPSN",
			"RankedPSN",
			"NormalXboxLive",
			"RankedXboxLive"
		};
	}
}
