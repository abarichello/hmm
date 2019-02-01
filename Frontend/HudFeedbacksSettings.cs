using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudFeedbacksSettings : GameHubScriptableObject
	{
		[Header("[Bomb Drop]")]
		public float BombDropTimeInSec;

		public string BombDropDraftTranslation;

		[Header("[Bomb Spawn]")]
		public string BombFeedbackTimerLine1DraftTranslation;

		public string BombFeedbackTimerLine2DraftTranslation;

		public string BombFeedbackSpawnLine1DraftTranslation;

		public string BombFeedbackSpawnLine2DraftTranslation;

		[Header("[Bomb Pick]")]
		public string BombPickClientOwnerDraftTranslation;

		public string BombPickAlliedDraftTranslation;

		public string BombPickEnemyDraftTranslation;

		[Header("[Bomb Pick]")]
		public float BombPickDisputeStartedFeedbackDurationInSec;

		public float BombPickDisputeFinishedFeedbackDurationInSec;

		[Header("[Upgrade Available]")]
		public string UpgradeAvailableDraftTranslation;

		public float UpgradeAvailableMatchTimeActivationMillis;

		public float UpgradeAvailableFeedbackDurationInSec;

		[Header("[Upgrade Available Time]")]
		public float UpgradeAvailableTimeInSec;

		[Header("[Normal Kills Time]")]
		public float NormalKillsTimeInSec;

		[Header("[Kill Streak Time]")]
		public float KillStreakTimeInSec;

		[Header("[Near Death Feedback]")]
		[Range(0f, 1f)]
		public float NearDeathPercent;
	}
}
