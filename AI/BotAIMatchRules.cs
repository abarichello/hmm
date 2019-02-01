using System;
using HeavyMetalMachines.BotAI;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.AI
{
	public class BotAIMatchRules : GameHubScriptableObject
	{
		public BotAIGoal GetMatchAIGoal()
		{
			int round = GameHubScriptableObject.Hub.BombManager.ScoreBoard.Round;
			if (round == 0)
			{
				return this.firstMatch.Match1;
			}
			if (round != 1)
			{
				return this.firstMatch.Match3;
			}
			return this.firstMatch.Match2;
		}

		public BotAIGoal BotOnlyTeamCap;

		[Tooltip("Min weapon range")]
		public float MinWeaponRange = 5f;

		[Tooltip("No try to ambush if enemy is close to you")]
		public float AmbushTargetDistance = 30f;

		[Tooltip("Max range bot will try to lead the deliver ( for trap )")]
		public float MaxLeadRange = 80f;

		[Tooltip("Interval that bot will wait before re evaluate a hit ( carrier or self )")]
		public float AggroDelay = 0.5f;

		[Tooltip("Interval that bot will wait before trying to get the bomb again")]
		public float BombPassDelay = 0.5f;

		[Tooltip("Distance limit to pass the bomb")]
		public float PassBombRange = 10f;

		[Tooltip("Worst range of action for a not focused bot (Goals.Focus) (X)")]
		public float MaxRangeOfAction = 150f;

		[Tooltip("Time a bot will still pursuit another car outside of the max range of action")]
		public float PursuitTime = 2f;

		[Tooltip("Number of bots that can be focused on the carrier at any given time")]
		public int MaxCarrierAggroCount = 2;

		public float CloseToDeliveryDistance = 200f;

		public BotPickConfig BotPickConfig;

		public BotAIMatchRules.FirstMatch firstMatch;

		[Serializable]
		public class FirstMatch
		{
			public BotAIGoal Match1;

			public BotAIGoal Match2;

			public BotAIGoal Match3;
		}
	}
}
