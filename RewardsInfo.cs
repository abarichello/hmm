using System;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using Pocketverse.Util;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class RewardsInfo : GameHubScriptableObject
	{
		public static int GetPerformanceMedal(float performance)
		{
			if (performance >= GameHubScriptableObject.Hub.Server.Rewards.PerformanceRewards[0].MinPerformance)
			{
				return 3;
			}
			if (performance >= GameHubScriptableObject.Hub.Server.Rewards.PerformanceRewards[1].MinPerformance)
			{
				return 2;
			}
			if (performance >= GameHubScriptableObject.Hub.Server.Rewards.PerformanceRewards[2].MinPerformance)
			{
				return 1;
			}
			return 0;
		}

		public int GetRewardsByMatchTime(float matchTime)
		{
			matchTime = Mathf.Floor(matchTime);
			float num = (float)LinearFit.ClampFit01((double)matchTime, (double)this.MatchTimeMinSeconds, (double)this.MatchTimeMaxSeconds);
			return Mathf.FloorToInt((float)LinearFit.InversedClampFit01((double)num, (double)this.MatchTimeMinXp, (double)this.MatchTimeMaxXp));
		}

		public RewardsInfo.BotXpMultiplier GetBotXpMultiplier(int playerLevel)
		{
			for (int i = 0; i < this.VsBotMultipliers.Length; i++)
			{
				RewardsInfo.BotXpMultiplier botXpMultiplier = this.VsBotMultipliers[i];
				if (playerLevel <= botXpMultiplier.MaxLevel)
				{
					return botXpMultiplier;
				}
			}
			return this.VsBotMultipliers[this.VsBotMultipliers.Length - 1];
		}

		public float GetPerformance(float playerPerformance, float matchTime, RewardsInfo.PerformanceIndicatorKind kind)
		{
			float num = 1f;
			switch (kind)
			{
			case RewardsInfo.PerformanceIndicatorKind.DamageDone:
				num = this.DamageDone;
				break;
			case RewardsInfo.PerformanceIndicatorKind.RepairDone:
				num = this.RepairDone;
				break;
			case RewardsInfo.PerformanceIndicatorKind.BombTime:
				num = this.BombTime;
				break;
			case RewardsInfo.PerformanceIndicatorKind.DebuffTime:
				num = this.DebuffTime;
				break;
			}
			float num2 = num * matchTime;
			return (num2 != 0f) ? (Mathf.Floor(playerPerformance) / num2) : 0f;
		}

		public void SetPerformance(RewardsBag rewards, PlayerStats stats, IMatchStats matchStats)
		{
			RewardsInfo rewards2 = GameHubScriptableObject.Hub.Server.Rewards;
			float matchTimeSeconds = matchStats.GetMatchTimeSeconds();
			int num = 0;
			rewards.DamageDone = rewards2.GetPerformance(stats.DamageDealtToPlayers, matchTimeSeconds, RewardsInfo.PerformanceIndicatorKind.DamageDone);
			rewards.RepairDone = rewards2.GetPerformance(stats.HealingProvided, matchTimeSeconds, RewardsInfo.PerformanceIndicatorKind.RepairDone);
			rewards.BombTime = rewards2.GetPerformance(stats.BombPossessionTime, matchTimeSeconds, RewardsInfo.PerformanceIndicatorKind.BombTime);
			rewards.DebuffTime = rewards2.GetPerformance(stats.DebuffTime, matchTimeSeconds, RewardsInfo.PerformanceIndicatorKind.DebuffTime);
			rewards.BombDeliveredCount = stats.BombsDelivered;
			rewards.KillsCount = stats.KillsAndAssists;
			rewards.DeathsCount = stats.Deaths;
			rewards.TotalDamage = (int)stats.DamageDealtToPlayers;
			rewards.TotalRepair = (int)stats.HealingProvided;
			rewards.TravelledDistanceKilometers = (int)stats.TravelledDistance / 1000;
			rewards.TravelledDistanceMeters = (int)stats.TravelledDistance;
			rewards.SpeedBoostCount = stats.GetGadgetUses(GadgetSlot.BoostGadget);
			rewards.BombLostCount = stats.BombCarrierKills;
			rewards.BombStolenCount = stats.BombTakenCount;
			rewards.TotalBombPossession = (int)stats.BombPossessionTime;
			rewards.TotalDebuffTime = (int)stats.DebuffTime;
			rewards.TotalTimePlayed = GameHubScriptableObject.Hub.GameTime.MatchTimer.GetTime() / 1000;
			if (RewardsInfo.GetPerformanceMedal(rewards.DamageDone) > 0)
			{
				num++;
			}
			if (RewardsInfo.GetPerformanceMedal(rewards.RepairDone) > 0)
			{
				num++;
			}
			if (RewardsInfo.GetPerformanceMedal(rewards.BombTime) > 0)
			{
				num++;
			}
			if (RewardsInfo.GetPerformanceMedal(rewards.DebuffTime) > 0)
			{
				num++;
			}
			stats.NumberOfMedals = num;
		}

		[Header("Match Time")]
		public float MatchTimeMinSeconds;

		public float MatchTimeMinXp;

		public float MatchTimeMaxSeconds;

		public float MatchTimeMaxXp;

		[Header("Match time xp bonus multipliers")]
		public RewardsInfo.BotXpMultiplier[] VsBotMultipliers;

		[Header("Result")]
		public float WinXpBonus;

		[Header("Scrap")]
		public int PVPWinScrapAmount;

		public int PVPLoseScrapAmount;

		public int PVEWinScrapAmount;

		public int PVELoseScrapAmount;

		public int RankedWinScrapAmount;

		public int RankedLoseScrapAmount;

		public int TournamentWinScrapAmount;

		public int TournamentLoseScrapAmount;

		[Header("Performance indicators")]
		public float DamageDone;

		public float RepairDone;

		public float BombTime;

		public float DebuffTime;

		public RewardsInfo.PerformanceXpReward[] PerformanceRewards;

		public enum PerformanceIndicatorKind
		{
			DamageDone,
			RepairDone,
			BombTime,
			DebuffTime
		}

		public enum Medals
		{
			None,
			Bronze,
			Silver,
			Gold
		}

		[Serializable]
		public class BotXpMultiplier
		{
			public float Multiplier;

			public int MaxLevel;
		}

		[Serializable]
		public class PerformanceXpReward
		{
			public float MinPerformance;

			public int XpBonusAmount;
		}
	}
}
