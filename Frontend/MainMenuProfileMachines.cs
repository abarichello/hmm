using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.Utils;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuProfileMachines : MainMenuProfileWindow
	{
		private IUiNavigationSubGroupHolder UiNavigationSubGroupHolder
		{
			get
			{
				return this._uiNavigationSubGroupHolder;
			}
		}

		private IUiNavigationRebuilder UiNavigationAxisSelectorRebuilder
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

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
			if (this._charactersCreated)
			{
				return;
			}
			this.CharactersGrid.hideInactive = false;
			UIGrid.Sorting sorting = this.CharactersGrid.sorting;
			this.CharactersGrid.sorting = UIGrid.Sorting.None;
			List<Transform> childList = this.CharactersGrid.GetChildList();
			this.CharactersGrid.sorting = sorting;
			for (int i = 0; i < childList.Count; i++)
			{
				childList[i].gameObject.SetActive(false);
			}
			List<IItemType> allCharacters = GameHubBehaviour.Hub.InventoryColletion.GetAllCharacters();
			allCharacters.Sort(new Comparison<IItemType>(this.SortCharacterItemTypes));
			int num = 0;
			int num2 = 0;
			while (num < allCharacters.Count && num2 < childList.Count)
			{
				IItemType itemType = allCharacters[num];
				CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
				if (itemType.IsActive)
				{
					MainMenuProfileCharacterCard component2 = childList[num2].GetComponent<MainMenuProfileCharacterCard>();
					component2.SetCharacterName(component.GetCharacterLocalizedName());
					component2.SetButtonEventListener(component.CharacterId);
					component2.SetCharacterSprite(HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, itemType.Id, HudUtils.PlayerIconSize.Size128));
					component2.SetLockGroupVisibility(true);
					component2.SetCharacterLevelProgressBarVisibility(true);
					CharacterBag characterBag;
					if (HudUtils.TryToGetCharacterBag(GameHubBehaviour.Hub, itemType.Id, out characterBag))
					{
						int levelForXP = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetLevelForXP(characterBag.Xp);
						component2.SetInfo((float)levelForXP, HudUtils.GetNormalizedLevelInfo(GameHubBehaviour.Hub.SharedConfigs.CharacterProgression, levelForXP, characterBag.Xp), component.Role);
					}
					else
					{
						Debug.Log(string.Format("(UpdateData) - CharacterBag not found for char typeId:[{0}]", itemType.Id));
						component2.SetInfo(0f, 0f, component.Role);
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
			this._charactersCreated = true;
			if (this._visible)
			{
				this.UiNavigationSubGroupHolder.SubGroupFocusGet();
			}
			this.UiNavigationAxisSelectorRebuilder.RebuildAndSelect();
		}

		private int SortCharacterItemTypes(IItemType x, IItemType y)
		{
			CharacterItemTypeComponent component = x.GetComponent<CharacterItemTypeComponent>();
			CharacterItemTypeComponent component2 = y.GetComponent<CharacterItemTypeComponent>();
			return string.Compare(component.GetCharacterLocalizedName(), component2.GetCharacterLocalizedName(), StringComparison.OrdinalIgnoreCase);
		}

		public override void SetWindowVisibility(bool visible)
		{
			this._visible = visible;
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
			this._returnToProfileSummary = false;
			this.SetUiNavigationFocus(visible);
		}

		private void SetUiNavigationFocus(bool focused)
		{
			if (focused)
			{
				if (this._charactersCreated)
				{
					this.UiNavigationSubGroupHolder.SubGroupFocusGet();
				}
			}
			else
			{
				this.UiNavigationSubGroupHolder.SubGroupFocusRelease();
			}
		}

		public override void OnPreBackToMainMenu()
		{
		}

		public override void OnBackToMainMenu()
		{
			this.SetWindowVisibility(false);
		}

		public void OnClickBackToMachinesButton()
		{
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
		}

		public void OnCharacterClickFromProfileSummary(int id)
		{
			this.OnCharacterClick(id);
			this._returnToProfileSummary = true;
		}

		private void UpdateDetails(int id)
		{
			Debug.Log("CharacterId: " + id);
			IItemType itemType;
			GameHubBehaviour.Hub.InventoryColletion.AllCharactersByCharacterId.TryGetValue(id, out itemType);
			CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
			this.DetailsGui.CarNameLabel.text = component.GetCharacterLocalizedName();
			this.DetailsGui.CarSprite.SpriteName = component.AssetPrefix + "_skin_00";
			this.DetailsGui.RewardsGrid.hideInactive = false;
			List<Transform> childList = this.DetailsGui.RewardsGrid.GetChildList();
			List<MainMenuProfileMachineRewardSlot.MachineRewardSlotInfo> list = new List<MainMenuProfileMachineRewardSlot.MachineRewardSlotInfo>(childList.Count);
			CharacterBag characterBag;
			int num;
			if (HudUtils.TryToGetCharacterBag(GameHubBehaviour.Hub, itemType.Id, out characterBag))
			{
				this.DetailsGui.UpdateCharacterInfo(GameHubBehaviour.Hub, characterBag);
				num = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetLevelForXP(characterBag.Xp);
			}
			else
			{
				Debug.Log(string.Format("(UpdateDetails) - CharacterBag not found for char typeId:[{0}]", itemType.Id));
				num = 0;
				this.DetailsGui.UpdateCharacterInfo(GameHubBehaviour.Hub, null);
			}
			string empty = string.Empty;
			ProgressionInfo.Level[] levels = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.Levels;
			for (int i = 0; i < levels.Length; i++)
			{
				if (levels[i].Kind != null)
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

		public GameObject MachinesGameObject;

		public GameObject DetailsGameObject;

		public int CharactersGridQuantity = 18;

		public UIGrid CharactersGrid;

		public UILabel FilterTitleLabel;

		[SerializeField]
		protected MainMenuProfileMachines.FilterComponent[] FilterComponents;

		public MainMenuProfileMachinesDetailsGuiComponents DetailsGui;

		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		private int _filterCurrentIndex;

		private bool _creatingPool;

		private bool _poolCreated;

		private int _characterIdOpenAfterPool;

		private bool _charactersCreated;

		private bool _returnToProfileSummary;

		private Coroutine disableCoroutine;

		private bool _visible;

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
	}
}
