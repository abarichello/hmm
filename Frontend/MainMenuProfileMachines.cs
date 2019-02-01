using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuProfileMachines : MainMenuProfileWindow
	{
		public override void OnLoading()
		{
			UIGrid charactersGrid = this.CharactersGrid;
			charactersGrid.onCustomSort = (Comparison<Transform>)Delegate.Combine(charactersGrid.onCustomSort, new Comparison<Transform>(this.SortCharactersList));
			this._characterIdOpenAfterPool = -1;
		}

		public override void OnUnloading()
		{
			this.CharactersGrid.onCustomSort = null;
		}

		private void CreatePoolFromUpdateData()
		{
			if (this._creatingPool)
			{
				return;
			}
			this._creatingPool = true;
			GameHubBehaviour.Hub.StartCoroutine(GUIUtils.CreateGridPoolAsync(GameHubBehaviour.Hub, this.CharactersGrid, this.CharactersGridQuantity, delegate()
			{
				GameHubBehaviour.Hub.StartCoroutine(GUIUtils.CreateGridPoolAsync(GameHubBehaviour.Hub, this.DetailsGui.RewardsGrid, this.DetailsGui.RewardsGridQuantity, delegate()
				{
					GameHubBehaviour.Hub.StartCoroutine(GUIUtils.CreateGridPoolAsync(GameHubBehaviour.Hub, this.DetailsGui.StatisticsGrid, this.DetailsGui.StatisticsInfoList.Length, delegate()
					{
						this._creatingPool = false;
						this._poolCreated = true;
						this.UpdateData();
						if (this._characterIdOpenAfterPool != -1)
						{
							this.OnCharacterClick(this._characterIdOpenAfterPool);
							this._characterIdOpenAfterPool = -1;
						}
					}));
				}));
			}));
		}

		public override void UpdateData()
		{
			if (this.FilterComponents.Length > 0)
			{
				this.FilterSetTitleLabel(this.FilterComponents[0]);
			}
			if (!this._poolCreated)
			{
				this.CreatePoolFromUpdateData();
				return;
			}
			this.CharactersGrid.hideInactive = false;
			List<Transform> childList = this.CharactersGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				childList[i].gameObject.SetActive(false);
			}
			List<ItemTypeScriptableObject> allCharacters = GameHubBehaviour.Hub.InventoryColletion.GetAllCharacters();
			int num = 0;
			int num2 = 0;
			while (num < allCharacters.Count && num2 < childList.Count)
			{
				CharacterItemTypeComponent component = allCharacters[num].GetComponent<CharacterItemTypeComponent>();
				HeavyMetalMachines.Character.CharacterInfo mainAttributes = component.MainAttributes;
				if (allCharacters[num].IsActive)
				{
					MainMenuProfileCharacterCard component2 = childList[num2].GetComponent<MainMenuProfileCharacterCard>();
					component2.SetCharacterName(mainAttributes.LocalizedName);
					component2.SetButtonEventListener(mainAttributes.CharacterId);
					component2.SetCharacterSprite(HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, allCharacters[num].Id, HudUtils.PlayerIconSize.Size128));
					component2.SetLockGroupVisibility(true);
					component2.SetCharacterLevelProgressBarVisibility(true);
					CharacterBag characterBag;
					if (HudUtils.TryToGetCharacterBag(GameHubBehaviour.Hub, mainAttributes.CharacterItemTypeGuid, out characterBag))
					{
						int levelForXP = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetLevelForXP(characterBag.Xp);
						component2.SetInfo((float)levelForXP, HudUtils.GetNormalizedLevelInfo(GameHubBehaviour.Hub.SharedConfigs.CharacterProgression, levelForXP, characterBag.Xp), mainAttributes.Role);
					}
					else
					{
						UnityEngine.Debug.Log(string.Format("(UpdateData) - CharacterBag not found for char typeId:[{0}]", mainAttributes.CharacterItemTypeGuid));
						component2.SetInfo(0f, 0f, mainAttributes.Role);
					}
					component2.SetCornerVisibility(false);
					component2.gameObject.SetActive(true);
				}
				num++;
				num2++;
			}
			this.CharactersGrid.hideInactive = true;
			this.CharactersGrid.Reposition();
			if (this.FilterComponents.Length > 0)
			{
				this.FilterSetTitleLabel(this.FilterComponents[this._filterCurrentIndex]);
			}
		}

		public override void SetWindowVisibility(bool visible)
		{
			base.StopCoroutineSafe(this.disableCoroutine);
			if (visible)
			{
				base.gameObject.SetActive(true);
				this.MachinesGameObject.SetActive(true);
				this.CharactersGrid.Reposition();
				this.ScreenAlphaAnimation.Play("profileMachinesIn");
			}
			else if (base.gameObject.activeInHierarchy)
			{
				this.ScreenAlphaAnimation.Play("profileMachinesOut");
				this.disableCoroutine = base.StartCoroutine(GUIUtils.WaitAndDisable(this.ScreenAlphaAnimation.clip.length, base.gameObject));
			}
			this.DetailsGameObject.SetActive(false);
			this.MainBackButtonGameObject.SetActive(true);
			this._returnToProfileSummary = false;
		}

		public override void OnBackToMainMenu()
		{
			this.SetWindowVisibility(false);
		}

		public void OnClickBackToMachinesButton()
		{
			this.MainBackButtonGameObject.SetActive(true);
			if (this._returnToProfileSummary)
			{
				this.MainMenuProfileController.ShowWindow(MainMenuProfileController.ProfileWindowType.Summary).LeftButton.Set(true, true);
				return;
			}
			this.MachinesGameObject.SetActive(true);
			this.DetailsGameObject.SetActive(false);
		}

		public void OnCharacterClick(int id)
		{
			this.DetailsGui.CarSprite.sprite2D = null;
			if (!this._poolCreated)
			{
				this._characterIdOpenAfterPool = id;
				return;
			}
			this.UpdateDetails(id);
			this.MachinesGameObject.SetActive(false);
			this.DetailsGameObject.SetActive(true);
			this.MainBackButtonGameObject.SetActive(false);
		}

		public void OnCharacterClickFromProfileSummary(int id)
		{
			this.OnCharacterClick(id);
			this._returnToProfileSummary = true;
		}

		private void UpdateDetails(int id)
		{
			HeavyMetalMachines.Character.CharacterInfo characterInfoByCharacterId = GameHubBehaviour.Hub.InventoryColletion.GetCharacterInfoByCharacterId(id);
			this.DetailsGui.CarNameLabel.text = characterInfoByCharacterId.LocalizedName;
			this.DetailsGui.CarSprite.SpriteName = characterInfoByCharacterId.Asset + "_skin_00";
			this.DetailsGui.RewardsGrid.hideInactive = false;
			List<Transform> childList = this.DetailsGui.RewardsGrid.GetChildList();
			List<MainMenuProfileMachineRewardSlot.MachineRewardSlotInfo> list = new List<MainMenuProfileMachineRewardSlot.MachineRewardSlotInfo>(childList.Count);
			CharacterBag characterBag;
			int num;
			if (HudUtils.TryToGetCharacterBag(GameHubBehaviour.Hub, characterInfoByCharacterId.CharacterItemTypeGuid, out characterBag))
			{
				this.DetailsGui.UpdateCharacterInfo(characterBag);
				num = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetLevelForXP(characterBag.Xp);
			}
			else
			{
				UnityEngine.Debug.Log(string.Format("(UpdateDetails) - CharacterBag not found for char typeId:[{0}]", characterInfoByCharacterId.CharacterItemTypeGuid));
				num = 0;
				this.DetailsGui.UpdateCharacterInfo(null);
			}
			string empty = string.Empty;
			ProgressionInfo.Level[] levels = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.Levels;
			for (int i = 0; i < levels.Length; i++)
			{
				if (levels[i].Kind != ProgressionInfo.RewardKind.None)
				{
					HudUtils.TryToGetPlayerUnlockRewardIconSpriteName(GameHubBehaviour.Hub.SharedConfigs.CharacterProgression, i, out empty);
					string rewardName;
					HudUtils.TryToGetUnlockRewardName(GameHubBehaviour.Hub.SharedConfigs.CharacterProgression, i, out rewardName);
					bool isLocked = i > num;
					list.Add(new MainMenuProfileMachineRewardSlot.MachineRewardSlotInfo
					{
						RewardName = rewardName,
						SpriteName = empty,
						IsLocked = isLocked,
						LevelText = (i + 1).ToString("0")
					});
				}
			}
			HudUtils.TryToGetPlayerUnlockRewardIconSpriteName(GameHubBehaviour.Hub.SharedConfigs.CharacterProgression, num + 1, out empty);
			bool flag = string.IsNullOrEmpty(empty);
			this.DetailsGui.NextRewardIconSprite.SpriteName = empty;
			this.DetailsGui.NextRewardIconSprite.gameObject.SetActive(!flag);
			this.DetailsGui.NextRewardNoRewardLabel.SetActive(flag);
			this.DetailsGui.UpdateRewards(list);
			if (characterBag != null)
			{
				this.DetailsGui.VictoriesLabel.text = characterBag.WinsCount.ToString("0");
				this.DetailsGui.DefeatsLabel.text = characterBag.DefeatsCount.ToString("0");
			}
			else
			{
				this.DetailsGui.VictoriesLabel.text = "0";
				this.DetailsGui.DefeatsLabel.text = "0";
			}
			this.DetailsGui.UpdateStatisticsGrid(characterBag);
		}

		public void OnFilterLeftClick()
		{
			this._filterCurrentIndex--;
			if (this._filterCurrentIndex < 0)
			{
				this._filterCurrentIndex = this.FilterComponents.Length - 1;
			}
			this.FilterSetTitleLabel(this.FilterComponents[this._filterCurrentIndex]);
			this.CharactersGrid.Reposition();
		}

		public void OnFilterRightClick()
		{
			this._filterCurrentIndex++;
			if (this._filterCurrentIndex >= this.FilterComponents.Length)
			{
				this._filterCurrentIndex = 0;
			}
			this.FilterSetTitleLabel(this.FilterComponents[this._filterCurrentIndex]);
			this.CharactersGrid.Reposition();
		}

		private void FilterSetTitleLabel(MainMenuProfileMachines.FilterComponent filterComponent)
		{
			this.FilterTitleLabel.text = Language.Get(filterComponent.TranslationDraft, filterComponent.TranslationSheet);
		}

		private int SortCharactersList(Transform t1, Transform t2)
		{
			MainMenuProfileMachines.FilterType filterType = MainMenuProfileMachines.FilterType.AtoZ;
			if (this.FilterComponents.Length > 0)
			{
				filterType = this.FilterComponents[this._filterCurrentIndex].Type;
			}
			return t1.GetComponent<MainMenuProfileCharacterCard>().Compare(filterType, t2.GetComponent<MainMenuProfileCharacterCard>());
		}

		public GameObject MainBackButtonGameObject;

		public GameObject MachinesGameObject;

		public GameObject DetailsGameObject;

		public int CharactersGridQuantity = 18;

		public UIGrid CharactersGrid;

		public UILabel FilterTitleLabel;

		[SerializeField]
		protected MainMenuProfileMachines.FilterComponent[] FilterComponents;

		private int _filterCurrentIndex;

		private bool _creatingPool;

		private bool _poolCreated;

		private int _characterIdOpenAfterPool;

		private bool _returnToProfileSummary;

		public MainMenuProfileMachines.DetailsGuiComponents DetailsGui;

		private Coroutine disableCoroutine;

		public enum FilterType
		{
			AtoZ,
			ZtoA,
			Role,
			Level
		}

		[Serializable]
		protected struct FilterComponent
		{
			public MainMenuProfileMachines.FilterType Type;

			public TranslationSheets TranslationSheet;

			public string TranslationDraft;
		}

		[Serializable]
		public struct DetailsGuiComponents
		{
			public void UpdateCharacterInfo(CharacterBag characterBag)
			{
				if (characterBag == null)
				{
					this.CharacterXpLabel.text = string.Format("0/{0}", GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetXPForLevel(1));
					this.LevelLabel.text = "1";
					this.LevelProgressBar.value = 0f;
					return;
				}
				int levelForXP = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetLevelForXP(characterBag.Xp);
				int num;
				int num2;
				GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetXpForSegment(characterBag.Xp, levelForXP, out num, out num2);
				this.CharacterXpLabel.text = string.Format("{0}/{1}", num, num2);
				this.LevelLabel.text = (levelForXP + 1).ToString("0");
				this.LevelProgressBar.value = HudUtils.GetNormalizedLevelInfo(GameHubBehaviour.Hub.SharedConfigs.CharacterProgression, levelForXP, characterBag.Xp);
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

			private MainMenuProfileStatisticsSlot.StatisticsSlotInfo GetStatisticsSlotInfo(CharacterBag characterBag, MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfo statisticsInfo)
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
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.BombStolen:
					result.Total = characterBag.BombStolenCount;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.BombLost:
					result.Total = characterBag.BombLostCount;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.BombDelivered:
					result.Total = characterBag.BombDeliveredCount;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.Wins:
					result.Total = characterBag.WinsCount;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.Defeats:
					result.Total = characterBag.DefeatsCount;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.Kills:
					result.Total = characterBag.KillsCount;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.Deaths:
					result.Total = characterBag.DeathsCount;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.Matches:
					result.Total = characterBag.MatchesCount;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.TotalDamage:
					result.Total = characterBag.TotalDamage;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.TotalRepair:
					result.Total = characterBag.TotalRepair;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.TravelledDistance:
					result.Total = characterBag.TravelledDistance;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.SpeedBoost:
					result.Total = characterBag.SpeedBoostCount;
					break;
				case MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType.ScrapCollected:
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
			internal MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfo[] StatisticsInfoList;

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

				public MainMenuProfileMachines.DetailsGuiComponents.StatisticsInfoType Type;

				public TranslationSheets TranslationSheet;

				public string TranslationDraft;
			}
		}
	}
}
