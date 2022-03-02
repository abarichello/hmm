using System;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.DataTransferObjects.Player;

namespace HeavyMetalMachines.Achievements
{
	public static class MatchPlayerStatisticsExtensions
	{
		public static void Convert(this MatchPlayerStatistics matchPlayerStatistics, PlayerStats playerStats, bool isMatchWon)
		{
			matchPlayerStatistics.BombDeliveredCount = playerStats.BombsDelivered;
			matchPlayerStatistics.DamageDealtToPlayers = (int)playerStats.DamageDealtToPlayers;
			matchPlayerStatistics.HealingProvided = (int)playerStats.HealingProvided;
			matchPlayerStatistics.Deaths = playerStats.Deaths;
			matchPlayerStatistics.KillsAndAssists = playerStats.KillsAndAssists;
			matchPlayerStatistics.TravelledMeters = (int)playerStats.TravelledDistance;
			matchPlayerStatistics.IsMatchWon = isMatchWon;
		}

		public static void Convert(this MatchPlayerStatistics matchPlayerStatistics, RewardsBag rewardsBag)
		{
			matchPlayerStatistics.BombDeliveredCount = rewardsBag.BombDeliveredCount;
			matchPlayerStatistics.DamageDealtToPlayers = rewardsBag.TotalDamage;
			matchPlayerStatistics.HealingProvided = rewardsBag.TotalRepair;
			matchPlayerStatistics.Deaths = rewardsBag.DeathsCount;
			matchPlayerStatistics.KillsAndAssists = rewardsBag.KillsCount;
			matchPlayerStatistics.TravelledMeters = rewardsBag.TravelledDistanceMeters;
			matchPlayerStatistics.IsMatchWon = rewardsBag.GetMatchWon();
		}
	}
}
