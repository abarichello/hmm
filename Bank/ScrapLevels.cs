using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Bank
{
	public class ScrapLevels : GameHubScriptableObject
	{
		public bool FreeUpgrades;

		public int StartingScrap = 9999;

		public int TimedScrapInterval;

		public int TimedScrapValue;

		public int AssistDelayMillis = 5000;

		public int MaxLevel;

		public int KillsAndAssistsToLevelUp = 1;

		public ModifierInfo[] LevelUpModifiers;

		public int DominationKills = 3;

		public int RevengeKills = 3;

		public int ScrapLifeTime;

		public int ScrapToLevelUp;

		public ScrapInfo[] ScrapPickupValue;

		public ScrapInfo ScrapPerPlayerKill;

		public int ScrapPerPlayerKillLevelBonus;

		public int ScrapPerPlayerKillLevelDiff;

		public int ScrapPerPlayerKillDeathStreakMaxAmount;

		public float ScrapPerPlayerKillDeathStreakMaxReduction;

		public bool ScrapPerPlayerKillRemovalOnDeath;

		public string ScrapPerPlayerKillAsset;

		public ScrapInfo ScrapPerPlayerAssist;

		public int ScrapPerPlayerAssistBonusPerLevel;

		public ScrapInfo ScrapPerFirstBlood;

		public ScrapInfo ScrapPerKillStreakEndBase;

		public int[] ScrapPerKillStreakEndLevels;

		public ScrapInfo ScrapPerTurret;

		public ScrapInfo ScrapPerGenerator;

		public ScrapInfo[] ScrapPerWard;

		public ScrapInfo[] ScrapPerNeutral;

		public ScrapInfo[] ScrapPerBombDelivery;

		public bool ScrapDrop;

		public bool ScrapDropWhenNoEnemiesAround;

		public bool CreepLastHitting;

		public int KillingSpreeTimeMillis = 10000;

		public float StrifeSplitRangeSqr = 3600f;

		public int ScrapSplitTeamSize = 5;

		public int MaximumScrapEffecs = 10;
	}
}
