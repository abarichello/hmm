using System;
using System.Collections.Generic;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public struct MainMenuProfileMachinesDetailsGuiComponents
	{
		public void UpdateCharacterInfo(HMMHub hub, CharacterBag characterBag)
		{
			if (characterBag == null)
			{
				this.CharacterXpLabel.text = string.Format("0/{0}", hub.SharedConfigs.CharacterProgression.GetXPForLevel(1));
				this.LevelLabel.text = "1";
				this.LevelProgressBar.value = 0f;
				return;
			}
			int levelForXP = hub.SharedConfigs.CharacterProgression.GetLevelForXP(characterBag.Xp);
			int num;
			int num2;
			hub.SharedConfigs.CharacterProgression.GetXpForSegment(characterBag.Xp, levelForXP, ref num, ref num2);
			this.CharacterXpLabel.text = string.Format("{0}/{1}", num, num2);
			this.LevelLabel.text = (levelForXP + 1).ToString("0");
			this.LevelProgressBar.value = HudUtils.GetNormalizedLevelInfo(hub.SharedConfigs.CharacterProgression, levelForXP, characterBag.Xp);
		}

		public void UpdateRewards(List<MainMenuProfileMachineRewardSlot.MachineRewardSlotInfo> slotInfos)
		{
			this.RewardsGrid.hideInactive = false;
			this.DisableGrid(this.RewardsGrid);
			for (int i = 0; i < slotInfos.Count; i++)
			{
				this.AddRewardInfo(slotInfos[i]);
			}
			this.RewardsGrid.hideInactive = true;
			this.RewardsGrid.Reposition();
		}

		private void AddRewardInfo(MainMenuProfileMachineRewardSlot.MachineRewardSlotInfo rewardSlotInfo)
		{
			List<Transform> childList = this.RewardsGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				MainMenuProfileMachineRewardSlot component = childList[i].GetComponent<MainMenuProfileMachineRewardSlot>();
				if (!component.gameObject.activeSelf)
				{
					component.Setup(rewardSlotInfo);
					component.gameObject.SetActive(true);
					break;
				}
			}
		}

		public void UpdateStatisticsGrid(CharacterBag characterBag)
		{
			this.StatisticsGrid.hideInactive = false;
			this.DisableGrid(this.StatisticsGrid);
			for (int i = 0; i < this.StatisticsInfoList.Length; i++)
			{
				MainMenuProfileStatisticsSlot.StatisticsSlotInfo statisticsSlotInfo = this.GetStatisticsSlotInfo(characterBag, this.StatisticsInfoList[i]);
				if (statisticsSlotInfo.Show)
				{
					this.AddStatisticsGridInfo(statisticsSlotInfo);
				}
			}
			this.StatisticsGrid.hideInactive = true;
			this.StatisticsGrid.Reposition();
		}

		private MainMenuProfileStatisticsSlot.StatisticsSlotInfo GetStatisticsSlotInfo(CharacterBag characterBag, MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfo statisticsInfo)
		{
			MainMenuProfileStatisticsSlot.StatisticsSlotInfo result = default(MainMenuProfileStatisticsSlot.StatisticsSlotInfo);
			result.Show = statisticsInfo.Show;
			result.IconSprite = statisticsInfo.IconSprite;
			result.Title = Language.Get(statisticsInfo.TranslationDraft, statisticsInfo.TranslationSheet);
			if (characterBag == null)
			{
				result.Total = 0;
				result.Average = 0f;
				return result;
			}
			switch (statisticsInfo.Type)
			{
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.BombStolen:
				result.Total = characterBag.BombStolenCount;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.BombLost:
				result.Total = characterBag.BombLostCount;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.BombDelivered:
				result.Total = characterBag.BombDeliveredCount;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.Wins:
				result.Total = characterBag.WinsCount;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.Defeats:
				result.Total = characterBag.DefeatsCount;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.Kills:
				result.Total = characterBag.KillsCount;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.Deaths:
				result.Total = characterBag.DeathsCount;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.Matches:
				result.Total = characterBag.MatchesCount;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.TotalDamage:
				result.Total = characterBag.TotalDamage;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.TotalRepair:
				result.Total = characterBag.TotalRepair;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.TravelledDistance:
				result.Total = characterBag.TravelledDistance;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.SpeedBoost:
				result.Total = characterBag.SpeedBoostCount;
				break;
			case MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType.ScrapCollected:
				result.Total = characterBag.ScrapCollectedCount;
				break;
			default:
				return result;
			}
			float num = Convert.ToSingle(result.Total);
			result.Average = ((result.Total != 0 && characterBag.MatchesCount != 0) ? (num / (float)characterBag.MatchesCount) : 0f);
			return result;
		}

		private void AddStatisticsGridInfo(MainMenuProfileStatisticsSlot.StatisticsSlotInfo statisticsSlotInfo)
		{
			List<Transform> childList = this.StatisticsGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				MainMenuProfileStatisticsSlot component = childList[i].GetComponent<MainMenuProfileStatisticsSlot>();
				if (!component.gameObject.activeSelf)
				{
					component.SetInfo(statisticsSlotInfo);
					component.gameObject.SetActive(true);
					break;
				}
			}
		}

		private void DisableGrid(UIGrid grid)
		{
			List<Transform> childList = grid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				childList[i].gameObject.SetActive(false);
			}
		}

		public UILabel CarNameLabel;

		public HMMUI2DDynamicSprite CarSprite;

		public UILabel LevelLabel;

		public UIProgressBar LevelProgressBar;

		public int RewardsGridQuantity;

		public UIGrid RewardsGrid;

		public UILabel VictoriesLabel;

		public UILabel DefeatsLabel;

		public UIScrollView StatisticsScrollView;

		public UIScrollBar StatisticsScrollBar;

		public UIGrid StatisticsGrid;

		public UILabel CharacterXpLabel;

		[Header("[Next Reward]")]
		public GameObject NextRewardGroupGameObject;

		public HMMUI2DDynamicSprite NextRewardIconSprite;

		public GameObject NextRewardNoRewardLabel;

		[SerializeField]
		internal MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfo[] StatisticsInfoList;

		internal enum StatisticsInfoType
		{
			BombStolen,
			BombLost,
			BombDelivered,
			Wins,
			Defeats,
			Kills,
			Deaths,
			Matches,
			TotalDamage,
			TotalRepair,
			TravelledDistance,
			SpeedBoost,
			ScrapCollected
		}

		[Serializable]
		internal struct StatisticsInfo
		{
			public bool Show;

			public Sprite IconSprite;

			public MainMenuProfileMachinesDetailsGuiComponents.StatisticsInfoType Type;

			public TranslationSheets TranslationSheet;

			public string TranslationDraft;
		}
	}
}
