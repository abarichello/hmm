using System;
using UnityEngine;

namespace HeavyMetalMachines.Pick
{
	public class MatchPickModeConfig : ScriptableObject
	{
		public float PickTime = 90f;

		public float BotPickTimeAllowedSeconds = 80f;

		public float CustomizationTime = 10f;

		public float MinBotCharSelectionTime = 1f;

		public float MaxBotCharSelectionTime = 2f;

		public float MinBotCharConfirmationTime = 1f;

		public float MaxBotCharConfirmationTime = 2f;

		public float MinBotGridSelectionTime = 1f;

		public float MaxBotGridSelectionTime = 2f;

		public float MinBotGridConfirmationTime = 1f;

		public float MaxBotGridConfirmationTime = 2f;
	}
}
