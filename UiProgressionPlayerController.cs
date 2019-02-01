using System;
using System.Collections.Generic;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class UiProgressionPlayerController
	{
		public UiProgressionPlayerController(SharedConfigs sharedConfigs, IMatchStats matchStats)
		{
			this._sharedConfigs = sharedConfigs;
			this._matchStats = matchStats;
			this.SetupAccountXp(0);
			this.SetupCharacterXp(0);
			this.SetupMatchRewards();
		}

		public int OldTotalAccountXp { get; private set; }

		public int OldAccountLevel { get; private set; }

		public int CurrentAccountXpInThisLevel { get; private set; }

		public int XpForNextAccountLevelUp { get; private set; }

		public bool PlayerAtMaxLevel { get; private set; }

		public int OldTotalCharacterXp { get; private set; }

		public int OldCharacterLevel { get; private set; }

		public int CurrentCharacterXpInThisLevel { get; private set; }

		public int XpForNextCharacterLevelUp { get; private set; }

		public bool CharacterAtMaxLevel { get; private set; }

		public int PenaltyAmount { get; private set; }

		public int XpBoosters { get; private set; }

		public int XpForEvents { get; private set; }

		public int XpByVictory { get; private set; }

		public int XpByMatchTime { get; private set; }

		public int XpBoosterDailly { get; private set; }

		public int XpFoundersBoost { get; private set; }

		public int XpByPerformance { get; private set; }

		public int TotalFame { get; private set; }

		private void SetupAccountXp(int currentXp = 0)
		{
			int oldTotalAccountXp = (currentXp != 0) ? currentXp : this._matchStats.CurrentPlayerReward.OldBattlepassXP;
			this.OldTotalAccountXp = oldTotalAccountXp;
			this.OldAccountLevel = this._sharedConfigs.PlayerProgression.GetLevelForXP(this.OldTotalAccountXp);
			this.CurrentAccountXpInThisLevel = this.OldTotalAccountXp - this._sharedConfigs.PlayerProgression.GetOverallXPForLevel(this.OldAccountLevel);
			this.XpForNextAccountLevelUp = this._sharedConfigs.PlayerProgression.GetXPForLevel(this.OldAccountLevel + 1);
			this.PlayerAtMaxLevel = (this.OldAccountLevel + 1 >= this._sharedConfigs.PlayerProgression.TotalLevels);
		}

		private void SetupCharacterXp(int currentXp = 0)
		{
			int oldTotalCharacterXp = (currentXp != 0) ? currentXp : this._matchStats.CurrentPlayerReward.OldCharacterXP;
			this.OldTotalCharacterXp = oldTotalCharacterXp;
			this.OldCharacterLevel = this._sharedConfigs.CharacterProgression.GetLevelForXP(this.OldTotalCharacterXp);
			this.CurrentCharacterXpInThisLevel = this.OldTotalCharacterXp - this._sharedConfigs.CharacterProgression.GetOverallXPForLevel(this.OldCharacterLevel);
			this.XpForNextCharacterLevelUp = this._sharedConfigs.CharacterProgression.GetXPForLevel(this.OldCharacterLevel + 1);
			this.CharacterAtMaxLevel = (this.OldCharacterLevel + 1 >= this._sharedConfigs.CharacterProgression.TotalLevels);
		}

		private void SetupMatchRewards()
		{
			this.XpBoosters = this._matchStats.CurrentPlayerReward.XpByBoost - this._matchStats.CurrentPlayerReward.XpByBoost * this.PenaltyAmount + this._testvalue;
			this.XpForEvents = this._matchStats.CurrentPlayerReward.XpByEvent - this._matchStats.CurrentPlayerReward.XpByEvent * this.PenaltyAmount + this._testvalue;
			this.XpByVictory = this._matchStats.CurrentPlayerReward.XpByWin - this._matchStats.CurrentPlayerReward.XpByWin * this.PenaltyAmount + this._testvalue;
			this.XpByMatchTime = this._matchStats.CurrentPlayerReward.XpByMatchtime - this._matchStats.CurrentPlayerReward.XpByMatchtime * this.PenaltyAmount + this._testvalue;
			this.XpBoosterDailly = this._matchStats.CurrentPlayerReward.XpByBoostDaily - this._matchStats.CurrentPlayerReward.XpByBoostDaily * this.PenaltyAmount + this._testvalue;
			this.XpFoundersBoost = this._matchStats.CurrentPlayerReward.XpByFounderBonus - this._matchStats.CurrentPlayerReward.XpByFounderBonus * this.PenaltyAmount + this._testvalue;
			this.XpByPerformance = this._matchStats.CurrentPlayerReward.XpByPerformance - this._matchStats.CurrentPlayerReward.XpByPerformance * this.PenaltyAmount + this._testvalue;
			this.UpdateXpReceivedList(this.XpByVictory, UiProgressionPlayerController.RewardReceivedByKind.Victory);
			this.UpdateXpReceivedList(this.XpByPerformance, UiProgressionPlayerController.RewardReceivedByKind.Performance);
			this.UpdateXpReceivedList(this.XpByMatchTime, UiProgressionPlayerController.RewardReceivedByKind.MatchTime);
			this.UpdateXpReceivedList(this.XpForEvents, UiProgressionPlayerController.RewardReceivedByKind.Events);
			this.UpdateXpReceivedList(this.XpFoundersBoost, UiProgressionPlayerController.RewardReceivedByKind.FoundersBooster);
			this.UpdateXpReceivedList(this.XpBoosterDailly, UiProgressionPlayerController.RewardReceivedByKind.BoosterDaily);
			this.UpdateXpReceivedList(this.XpBoosters, UiProgressionPlayerController.RewardReceivedByKind.Boosters);
		}

		public void UpdateXpReceivedList(int valueRecived, UiProgressionPlayerController.RewardReceivedByKind kind)
		{
			if (valueRecived <= 0)
			{
				return;
			}
			UiProgressionPlayerController.Reward item = default(UiProgressionPlayerController.Reward);
			item.Kind = kind;
			item.Value = valueRecived;
			this.Rewards.Add(item);
		}

		public void OnPlayerLevelUp()
		{
			int overallXPForLevel = this._sharedConfigs.PlayerProgression.GetOverallXPForLevel(this.OldAccountLevel + 1);
			this.SetupAccountXp(overallXPForLevel);
		}

		public void OnCharacterLevelUp()
		{
			int overallXPForLevel = this._sharedConfigs.CharacterProgression.GetOverallXPForLevel(this.OldCharacterLevel + 1);
			this.SetupCharacterXp(overallXPForLevel);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(UiProgressionPlayerController));

		private readonly SharedConfigs _sharedConfigs;

		private readonly IMatchStats _matchStats;

		public List<UiProgressionPlayerController.Reward> Rewards = new List<UiProgressionPlayerController.Reward>(9);

		private readonly int _testvalue;

		public struct Reward
		{
			public int Value;

			public UiProgressionPlayerController.RewardReceivedByKind Kind;
		}

		public enum RewardReceivedByKind
		{
			None,
			Events,
			Victory,
			Boosters,
			MatchTime,
			Performance,
			BoosterDaily,
			FoundersBooster,
			Fame
		}
	}
}
