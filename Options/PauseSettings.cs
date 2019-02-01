using System;
using HeavyMetalMachines.Match;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	public class PauseSettings : ScriptableObject
	{
		public PauseSettings.PauseData GetPauseData(MatchData.MatchKind matchKind)
		{
			PauseSettings.PauseData pauseData = null;
			if (this.pauseSettingsData != null)
			{
				for (int i = 0; i < this.pauseSettingsData.Length; i++)
				{
					if (this.pauseSettingsData[i].matchKind == matchKind)
					{
						pauseData = this.pauseSettingsData[i];
						Debug.Log("[PauseSettings] Loaded Pause Settings of MatchKind: " + matchKind);
						break;
					}
				}
			}
			else
			{
				Debug.LogError("[PauseSettings] pauseSettingsData is not assigned on inspector.", this);
			}
			if (pauseData == null)
			{
				Debug.LogError("[PauseSettings] Could not find PauseData for MatchKind: " + matchKind + ". Creating a default one.", this);
				pauseData = new PauseSettings.PauseData();
			}
			return pauseData;
		}

		[SerializeField]
		private PauseSettings.PauseData[] pauseSettingsData;

		[Serializable]
		public class PauseData
		{
			public MatchData.MatchKind matchKind;

			public float EnemyDelayToUnpause = 30f;

			public float AlliedDelayToUnpause;

			public float CooldownToPauseAgain = 120f;

			public float PauseCountDownTime = 3f;

			public float UnpauseCountDownTime = 3f;
		}
	}
}
