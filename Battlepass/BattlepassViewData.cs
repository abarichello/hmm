using System;
using System.Collections.Generic;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Specializations;
using Commons.Swordfish.Battlepass;

namespace HeavyMetalMachines.Battlepass
{
	[Serializable]
	public class BattlepassViewData
	{
		public BattlepassViewData.BattlepassViewDataLevels DataLevels;

		public BattlepassViewData.BattlepassViewDataTime DataTime;

		public BattlepassViewData.BattlepassViewDataSeason DataSeason;

		public BattlepassConfig BattlepassConfig;

		public BattlepassProgress BattlepassProgress;

		[Serializable]
		public struct BattlepassViewDataLevels
		{
			public int CurrentLevel;

			public int MaxLevels;

			public int CurrentXp;

			public bool HasXpBooster;

			public int[] MaxXpPerLevel;
		}

		[Serializable]
		public struct BattlepassViewDataTime
		{
			public DateTime UtcNow
			{
				get
				{
					return DateTime.UtcNow + this.OffsetToSf;
				}
			}

			public TimeSpan GetRemainingTime()
			{
				return this.EndDateUtc - this.UtcNow;
			}

			public DateTime StartDateUtc;

			public DateTime EndDateUtc;

			public TimeSpan OffsetToSf;
		}

		[Serializable]
		public struct BattlepassViewDataSeason
		{
			public bool UserHasPremium;

			public int LevelPriceValue;

			public List<BattlepassViewData.BattlepassViewDataSlotItem> FreeSeasonItems;

			public List<BattlepassViewData.BattlepassViewDataSlotItem> PremiumSeasonItems;
		}

		[Serializable]
		public struct BattlepassViewDataSlotItem
		{
			public ProgressionInfo.RewardKind RewardKind;

			public int UnlockLevel;

			public int CurrencyAmount;

			public string IconAssetName;

			public string ArtAssetName;

			public ItemPreviewKind PreviewKind;

			public string TitleDraft;

			public string DescriptionDraft;

			public string LoreTitleDraft;

			public string LoreSubtitleDraft;

			public string LoreDescriptionDraft;

			public bool IsRepeated;

			public string ArtPreviewBackGroundAssetName;

			public SkinPrefabItemTypeComponent.SkinCustomizations SkinCustomizations;
		}
	}
}
