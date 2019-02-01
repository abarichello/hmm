using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.UnityUI;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassSeasonScroller : MonoBehaviour, IBattlepassSeasonScroller, IEnhancedScrollerDelegate
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event UnityUiBattlepassSeasonScroller.SlotAnimationTriggerDelegate SlotAnimationTrigger;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event UnityUiBattlepassSeasonScroller.SlotTransformRequestDelegate SlotTransformRequestTrigger;

		protected void Start()
		{
			this._slotCellViewWidth = this._slotCellViewPrefab.GetComponent<RectTransform>().sizeDelta.x;
			this._currentSlotIndex = 0;
			this._scroller.Delegate = this;
			this._scroller.ScrollRect.horizontal = false;
			this._slotSelector.SetTarget(null, false, false);
		}

		protected void OnEnable()
		{
			this._buyButtons.AddBuyCallbacks(new UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate(this.OnUnlockPremiumButton), new UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate(this.OnBuyLevelsButton), new UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate(this.OnBuyAllLevelsButton));
			this.TryToRestorePendingAnimationData();
			this.SetupByPremiumState(this._userHasPremium);
		}

		private void TryToRestorePendingAnimationData()
		{
			if (this._isAnimatingUnlockLevels)
			{
				this._isAnimatingUnlockLevels = false;
				this._mainCanvasGroup.interactable = true;
				this._currentLevel = this._unlockAnimationTargetLevel;
				for (int i = 0; i <= this._unlockAnimationTargetLevel; i++)
				{
					this.SetAnimationSeasonScrollerData(i, this._unlockAnimationTargetLevel);
				}
				this._scroller.RefreshActiveCellViews();
				this.UpdateBuyLevelButton();
				this.JumpToPageOfSlotIndex(this.GetSelectedLevel(), true);
			}
		}

		protected void OnDisable()
		{
			this._buyButtons.RemoveBuyCallbacks(new UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate(this.OnUnlockPremiumButton), new UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate(this.OnBuyLevelsButton), new UnityUiBattlepassSeasonBuyButtons.SeasonBuyButtonClickDelegate(this.OnBuyAllLevelsButton));
			base.StopAllCoroutines();
			this._isJumping = false;
			this._slotSelector.ResetTarget();
		}

		public void SetVisibility(bool isVisible, bool imediate = false)
		{
			base.StopAllCoroutines();
			base.gameObject.SetActive(true);
			if (imediate)
			{
				this._mainCanvasGroup.alpha = ((!isVisible) ? 0f : 1f);
			}
			else if (isVisible)
			{
				this.ReselectCurrentSlotArtPreview();
				this._mainAnimation.Play("battlepass_transition_in");
			}
			else
			{
				this._mainAnimation.Play("battlepass_transition_out");
				base.StartCoroutine(this.WaitForCloseAnimation(this._mainAnimation.GetClip("battlepass_transition_out").length));
			}
		}

		private void ReselectCurrentSlotArtPreview()
		{
			UnityUiBattlepassArtPreview.ArtPreviewData artPreviewData = (this._selectedPremiumSlotLevel == -1) ? this._seasonScrollerArtPreviewDatas[this._selectedFreeSlotLevel].FreeDataArtPreview : this._seasonScrollerArtPreviewDatas[this._selectedPremiumSlotLevel].PremiumDataArtPreview;
			this._artPreview.ShowReward(artPreviewData, true);
		}

		private IEnumerator WaitForCloseAnimation(float timeInSec)
		{
			yield return new WaitForSeconds(timeInSec + Time.deltaTime);
			base.gameObject.SetActive(false);
			yield break;
		}

		protected void Update()
		{
			this.ArrowInputCheckUpdate();
		}

		private void ArrowInputCheckUpdate()
		{
			if (Input.GetKeyUp(KeyCode.LeftArrow))
			{
				int i = this.GetSelectedLevel();
				bool isPremium = this._selectedPremiumSlotLevel != -1;
				while (i > 0)
				{
					i--;
					if (this.OnSlotSelected(isPremium, i))
					{
						this.TryToJumpToPageOfSlotIndex(i, false);
						break;
					}
				}
			}
			else if (Input.GetKeyUp(KeyCode.RightArrow))
			{
				int j = this.GetSelectedLevel();
				bool isPremium2 = this._selectedPremiumSlotLevel != -1;
				while (j < this._maxSlots - 1)
				{
					j++;
					if (this.OnSlotSelected(isPremium2, j))
					{
						this.TryToJumpToPageOfSlotIndex(j, false);
						break;
					}
				}
			}
			else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
			{
				int selectedLevel = this.GetSelectedLevel();
				bool flag = this._selectedPremiumSlotLevel != -1;
				this.OnSlotSelected(!flag, selectedLevel);
			}
		}

		public void Setup(IBattlepassBuyUiActions buyUiActions, IBattlepassLevelProgress levelProgress, int pageSize, int maxSlots, int currentLevel, BattlepassViewData.BattlepassViewDataSeason dataSeason)
		{
			this._mainCanvasGroup.interactable = true;
			this._isAnimatingUnlockLevels = false;
			this._userHasPremium = dataSeason.UserHasPremium;
			this._buyUiActions = buyUiActions;
			this._levelProgress = levelProgress;
			this._pageSize = pageSize;
			this._maxSlots = maxSlots;
			this._numPages = this._maxSlots / this._pageSize;
			this._currentLevel = currentLevel;
			this._buyLevelPriceValue = dataSeason.LevelPriceValue;
			this._selectedFreeSlotLevel = -1;
			this._selectedPremiumSlotLevel = -1;
			this.UpdatePageInfo();
			this._seasonCellViewDatas = new UnityUiBattlepassSeasonCellView.SeasonCellViewData[maxSlots];
			this._seasonScrollerArtPreviewDatas = new UnityUiBattlepassSeasonScroller.SeasonScrollerArtPreviewData[maxSlots];
			for (int i = 0; i < maxSlots; i++)
			{
				UnityUiBattlepassSeasonCellView.SeasonCellViewData seasonCellViewData = new UnityUiBattlepassSeasonCellView.SeasonCellViewData
				{
					Level = i,
					IsCurrentLevel = (i == this._currentLevel),
					LevelType = ((i != this._currentLevel) ? ((i >= this._currentLevel) ? UnityUiBattlepassSeasonCellView.LevelType.Locked : UnityUiBattlepassSeasonCellView.LevelType.Unlocked) : UnityUiBattlepassSeasonCellView.LevelType.Current)
				};
				seasonCellViewData.FreeSlotData = new UnityUiBattlepassSeasonCellView.SeasonCellViewSlotData
				{
					SlotHasReward = false,
					IsLocked = (i > this._currentLevel)
				};
				seasonCellViewData.PremiumSlotData = new UnityUiBattlepassSeasonCellView.SeasonCellViewSlotData
				{
					SlotHasReward = false,
					IsLocked = (i > this._currentLevel)
				};
				this._seasonCellViewDatas[i] = seasonCellViewData;
			}
			List<BattlepassViewData.BattlepassViewDataSlotItem> freeSeasonItems = dataSeason.FreeSeasonItems;
			for (int j = 0; j < freeSeasonItems.Count; j++)
			{
				int unlockLevel = freeSeasonItems[j].UnlockLevel;
				this._seasonCellViewDatas[unlockLevel].FreeSlotData = this.GetSetupDataSlot(freeSeasonItems[j], this._selectedFreeSlotLevel);
				this._seasonScrollerArtPreviewDatas[unlockLevel].FreeDataArtPreview = this.GetSetupDataArtPreview(freeSeasonItems[j]);
			}
			List<BattlepassViewData.BattlepassViewDataSlotItem> premiumSeasonItems = dataSeason.PremiumSeasonItems;
			for (int k = 0; k < premiumSeasonItems.Count; k++)
			{
				int unlockLevel2 = premiumSeasonItems[k].UnlockLevel;
				this._seasonCellViewDatas[unlockLevel2].PremiumSlotData = this.GetSetupDataSlot(premiumSeasonItems[k], this._selectedPremiumSlotLevel);
				this._seasonScrollerArtPreviewDatas[unlockLevel2].PremiumDataArtPreview = this.GetSetupDataArtPreview(premiumSeasonItems[k]);
			}
			this.SetupByPremiumState(dataSeason.UserHasPremium);
			this._buyButtons.SetPremiumButtonInteractable(!dataSeason.UserHasPremium);
			this._buyButtons.SetupBuyLevels(this._buyLevelPriceValue.ToString("0"));
			this.UpdateBuyAllLevelsText(this._maxSlots, this._currentLevel, dataSeason.LevelPriceValue);
			this._scroller.ReloadData(0f);
		}

		private UnityUiBattlepassSeasonCellView.SeasonCellViewSlotData GetSetupDataSlot(BattlepassViewData.BattlepassViewDataSlotItem dataSlotItem, int selectedSlotLevel)
		{
			return new UnityUiBattlepassSeasonCellView.SeasonCellViewSlotData
			{
				IconAssetName = dataSlotItem.IconAssetName,
				SlotHasReward = true,
				CurrencyAmount = dataSlotItem.CurrencyAmount,
				IsHardCurrency = (dataSlotItem.RewardKind == ProgressionInfo.RewardKind.HardCurrency),
				IsLocked = (dataSlotItem.UnlockLevel > this._currentLevel),
				IsSelected = (selectedSlotLevel == dataSlotItem.UnlockLevel),
				IsRepeated = dataSlotItem.IsRepeated
			};
		}

		private UnityUiBattlepassArtPreview.ArtPreviewData GetSetupDataArtPreview(BattlepassViewData.BattlepassViewDataSlotItem dataSlotItem)
		{
			UnityUiBattlepassArtPreview.ArtPreviewData result = default(UnityUiBattlepassArtPreview.ArtPreviewData);
			result.RewardAssetName = dataSlotItem.ArtAssetName;
			result.RewardAssetKind = dataSlotItem.PreviewKind;
			result.TitleText = dataSlotItem.TitleDraft;
			result.DescriptionText = dataSlotItem.DescriptionDraft;
			result.ShowCurrencyIcon = (dataSlotItem.CurrencyAmount > 0);
			result.IsHardCurrency = (dataSlotItem.RewardKind == ProgressionInfo.RewardKind.HardCurrency);
			result.CurrencyReward = dataSlotItem.CurrencyAmount;
			result.LoreData.TitleText = dataSlotItem.LoreTitleDraft;
			result.LoreData.SubtitleText = dataSlotItem.LoreSubtitleDraft;
			result.LoreData.DescriptionText = dataSlotItem.LoreDescriptionDraft;
			result.LoreData.IsLocked = (dataSlotItem.UnlockLevel > this._currentLevel);
			result.ArtPreviewBackGroundAssetName = dataSlotItem.ArtPreviewBackGroundAssetName;
			result.SkinCustomizations = dataSlotItem.SkinCustomizations;
			return result;
		}

		public void RefreshData(BattlepassViewData.BattlepassViewDataLevels dataLevels, BattlepassViewData.BattlepassViewDataSeason dataSeason)
		{
			this.UpdateBuyAllLevelsText(dataLevels.MaxLevels, dataLevels.CurrentLevel, dataSeason.LevelPriceValue);
		}

		private void UpdateBuyAllLevelsText(int maxSlots, int currentLevel, int levelPrice)
		{
			string allLevelsButtonHoverValue = ((maxSlots - (currentLevel + 1)) * levelPrice).ToString("0");
			this._buyButtons.SetAllLevelsButtonHoverValue(allLevelsButtonHoverValue);
		}

		private void SetupByPremiumState(bool userHasPremium)
		{
			for (int i = 0; i < this._maxSlots; i++)
			{
				bool isLocked = !userHasPremium || i > this._currentLevel;
				this._seasonCellViewDatas[i].PremiumSlotData.IsLocked = isLocked;
				this._seasonScrollerArtPreviewDatas[i].PremiumDataArtPreview.LoreData.IsLocked = isLocked;
			}
			this._buyButtons.SetupByPremiumState(userHasPremium, this._currentLevel, this._maxSlots);
			this._premiumLockCanvasGroup.alpha = ((!userHasPremium) ? 1f : 0f);
			this._premiumLockCanvasGroup.interactable = !userHasPremium;
			this._premiumLockCanvasGroup.blocksRaycasts = !userHasPremium;
		}

		public UnityUiBattlepassSeasonCellView.SeasonCellViewData GetSeasonCellViewData(int slotLevel)
		{
			return this._seasonCellViewDatas[slotLevel];
		}

		public bool IsSlotSelectionInCorner(int slotIndex)
		{
			if (slotIndex != -1)
			{
				int num = this._currentSlotIndex + this._pageSize;
				return slotIndex == this._currentSlotIndex || slotIndex == num - 1;
			}
			return false;
		}

		public bool IsSlotSelectionOutCorner(int slotIndex)
		{
			if (slotIndex != -1)
			{
				int num = this._currentSlotIndex + this._pageSize;
				return slotIndex == this._currentSlotIndex - 1 || slotIndex == num;
			}
			return false;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._maxSlots;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return this._slotCellViewWidth;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UnityUiBattlepassSeasonCellView unityUiBattlepassSeasonCellView = scroller.GetCellView(this._slotCellViewPrefab) as UnityUiBattlepassSeasonCellView;
			unityUiBattlepassSeasonCellView.Setup(this, this._seasonCellViewDatas[dataIndex], new UnityUiBattlepassSeasonCellView.OnSlotSelected(this.OnSlotSelected), new UnityUiBattlepassSeasonCellView.OnSlotUpdateSelector(this.OnSlotUpdateSelector), ref this.SlotAnimationTrigger, ref this.SlotTransformRequestTrigger);
			return unityUiBattlepassSeasonCellView;
		}

		private bool OnSlotSelected(bool isPremium, int slotLevel)
		{
			if (!((!isPremium) ? this._seasonCellViewDatas[slotLevel].FreeSlotData.SlotHasReward : this._seasonCellViewDatas[slotLevel].PremiumSlotData.SlotHasReward))
			{
				return false;
			}
			if ((isPremium && this._selectedPremiumSlotLevel == slotLevel) || (!isPremium && this._selectedFreeSlotLevel == slotLevel))
			{
				return false;
			}
			this._artPreview.ShowReward((!isPremium) ? this._seasonScrollerArtPreviewDatas[slotLevel].FreeDataArtPreview : this._seasonScrollerArtPreviewDatas[slotLevel].PremiumDataArtPreview, true);
			this.TryToClearSlotSelection();
			this._selectedPremiumSlotLevel = ((!isPremium) ? -1 : slotLevel);
			this._selectedFreeSlotLevel = ((!isPremium) ? slotLevel : -1);
			if (isPremium)
			{
				this._seasonCellViewDatas[slotLevel].PremiumSlotData.IsSelected = true;
			}
			else
			{
				this._seasonCellViewDatas[slotLevel].FreeSlotData.IsSelected = true;
			}
			this._scroller.RefreshActiveCellViews();
			this.UpdateBuyLevelButton();
			return true;
		}

		private void OnSlotUpdateSelector(int slotLevel, Transform slotTransform)
		{
			this._slotSelector.SetTarget(slotTransform, slotLevel != this._currentLevel, false);
		}

		private void UpdateBuyLevelButton()
		{
			this._buyButtons.UpdateBuyLevelButton(this.GetSelectedLevel(), this._currentLevel, this._buyLevelPriceValue);
		}

		private void TryToClearSlotSelection()
		{
			if (this._selectedFreeSlotLevel != -1)
			{
				this._seasonCellViewDatas[this._selectedFreeSlotLevel].FreeSlotData.IsSelected = false;
			}
			if (this._selectedPremiumSlotLevel != -1)
			{
				this._seasonCellViewDatas[this._selectedPremiumSlotLevel].PremiumSlotData.IsSelected = false;
			}
		}

		[UnityUiComponentCall]
		public void JumpRight()
		{
			int num = this._currentSlotIndex + this._pageSize;
			if (num < this._maxSlots)
			{
				this._currentSlotIndex = num;
				this.TryToJump(num);
			}
		}

		[UnityUiComponentCall]
		public void JumpLeft()
		{
			int num = this._currentSlotIndex - this._pageSize;
			if (num >= 0)
			{
				this._currentSlotIndex = num;
				this.TryToJump(num);
			}
		}

		private void TryToJumpToPageOfSlotIndex(int slotIndex, bool imediate = true)
		{
			int num = slotIndex / this._pageSize;
			int num2 = num * this._pageSize;
			if (num2 != this._currentSlotIndex)
			{
				this.JumpToPageOfSlotIndex(slotIndex, imediate);
			}
		}

		public void JumpToPageOfSlotIndex(int slotIndex, bool imediate = true)
		{
			int num = slotIndex / this._pageSize;
			this._currentSlotIndex = num * this._pageSize;
			if (imediate)
			{
				this._scroller.JumpToDataIndex(this._currentSlotIndex, 0f, 0f, true, EnhancedScroller.TweenType.immediate, 0f, null, EnhancedScroller.LoopJumpDirectionEnum.Closest);
				this.UpdateNavigationButtons();
				this.UpdatePageInfo();
				if (this.IsSlotSelectionInCorner(this.GetSelectedLevel()) || this.IsSlotSelectionInCorner(this._currentLevel))
				{
					this._scroller.RefreshActiveCellViews();
				}
			}
			else
			{
				this.TryToJump(this._currentSlotIndex);
			}
		}

		public void JumpReset()
		{
			this._currentSlotIndex = 0;
			this._scroller.JumpToDataIndex(0, 0f, 0f, true, EnhancedScroller.TweenType.immediate, 0f, null, EnhancedScroller.LoopJumpDirectionEnum.Closest);
			this._leftButton.interactable = false;
			this._rightButton.interactable = (this._numPages > 1);
		}

		private void TryToJump(int desiredSlotIndex)
		{
			if (!this._isJumping)
			{
				this.JumpToIndex(desiredSlotIndex);
			}
		}

		private void JumpToIndex(int slotIndex)
		{
			this._isJumping = true;
			this._jumpTargetIndex = slotIndex;
			if (this.IsSlotSelectionInCorner(this.GetSelectedLevel()) || this.IsSlotSelectionInCorner(this._currentLevel))
			{
				this._scroller.RefreshActiveCellViews();
			}
			this._scroller.JumpToDataIndex(slotIndex, 0f, 0f, true, this._scrollerJumpTweenType, this._scrollerJumpTweenTimeInSec, new Action(this.OnJumpCompleted), EnhancedScroller.LoopJumpDirectionEnum.Down);
		}

		private void OnJumpCompleted()
		{
			this._isJumping = false;
			this.UpdateNavigationButtons();
			this.UpdatePageInfo();
			if (this._currentSlotIndex != this._jumpTargetIndex)
			{
				this.JumpToIndex(this._currentSlotIndex);
			}
			else if (this.IsSlotSelectionOutCorner(this.GetSelectedLevel()) || this.IsSlotSelectionOutCorner(this._currentLevel))
			{
				this._scroller.RefreshActiveCellViews();
			}
		}

		private void UpdateNavigationButtons()
		{
			this._leftButton.interactable = (this._currentSlotIndex > 0);
			int num = this._currentSlotIndex / this._pageSize;
			this._rightButton.interactable = (num < this._numPages - 1);
		}

		private void UpdatePageInfo()
		{
			int num = this._currentSlotIndex / this._pageSize;
			this._pageText.text = string.Format("<color=#{0}>{1} {2}</color> / {3}", new object[]
			{
				HudUtils.RGBToHex(this._pageTextColor),
				Language.Get("BATTLEPASS_PAGE", TranslationSheets.Battlepass),
				num + 1,
				this._numPages
			});
		}

		public void SelectSlotForLevel(int level)
		{
			bool slotHasReward = this._seasonCellViewDatas[level].FreeSlotData.SlotHasReward;
			bool isLocked = this._seasonCellViewDatas[level].PremiumSlotData.IsLocked;
			bool isPremium = !slotHasReward || !isLocked;
			this.OnSlotSelected(isPremium, level);
		}

		public void ShowUnlockPremiumAnimation(Action onAnimationFinished)
		{
			this._artPreview.PreloadModelViewer();
			this._userHasPremium = true;
			base.StartCoroutine(this.ShowUnlockPremiumAnimationCoroutine(onAnimationFinished));
		}

		private IEnumerator ShowUnlockPremiumAnimationCoroutine(Action onAnimationFinished)
		{
			this._mainCanvasGroup.interactable = false;
			this._unlockPremiumAnimation.Play();
			yield return new WaitForSeconds(this._delayAfterUnlockPremiumAnimationInSec);
			for (int i = 0; i < this._maxSlots; i++)
			{
				this._seasonCellViewDatas[i].PremiumSlotData.IsLocked = (i > this._currentLevel);
			}
			this._scroller.RefreshActiveCellViews();
			bool levelMaxedOut = this._currentLevel == this._maxSlots - 1;
			if (levelMaxedOut)
			{
				this._buyButtons.DisableButtonsCanvas();
			}
			yield return new WaitForSeconds(this._buyButtons.PlayUnlockAnimation());
			while (this._unlockPremiumAnimation.isPlaying)
			{
				yield return null;
			}
			this.SetupByPremiumState(true);
			this._mainCanvasGroup.interactable = true;
			onAnimationFinished();
			yield break;
		}

		public int GetSelectedLevel()
		{
			if (this._selectedFreeSlotLevel != -1)
			{
				return this._selectedFreeSlotLevel;
			}
			return this._selectedPremiumSlotLevel;
		}

		public void ShowUnlockLevelAnimation(int levelUnlockTarget, Action onAnimationFinished)
		{
			this._artPreview.PreloadModelViewer();
			base.StartCoroutine(this.ShowUnlockLevelsAnimationCoroutine(levelUnlockTarget, onAnimationFinished));
		}

		private IEnumerator ShowUnlockLevelsAnimationCoroutine(int targetLevel, Action onAnimationFinished)
		{
			this._isAnimatingUnlockLevels = true;
			this._unlockAnimationTargetLevel = targetLevel;
			this._mainCanvasGroup.interactable = false;
			this._buyButtons.DisableLevelButtons();
			this.JumpToPageOfSlotIndex(this._currentLevel + 1, false);
			while (this._scroller.IsTweening)
			{
				yield return null;
			}
			this._seasonCellViewDatas[this._currentLevel].IsCurrentLevel = false;
			this._scroller.RefreshActiveCellViews();
			int slotLevel = this._currentLevel + 1;
			int selectorLevel = (slotLevel != this._currentSlotIndex) ? this._currentLevel : slotLevel;
			do
			{
				this._slotSelector.ResetTarget();
				this.SlotTransformRequestTrigger(selectorLevel);
				int targetSelectorLevel = Mathf.Min(targetLevel, this._currentSlotIndex + this._pageSize - 1);
				this.SlotTransformRequestTrigger(targetSelectorLevel);
				while (slotLevel < this._currentSlotIndex + this._pageSize && slotLevel <= targetLevel)
				{
					this.SetAnimationSeasonScrollerData(slotLevel, targetLevel);
					this._seasonCellViewDatas[slotLevel].IsCurrentLevel = false;
					this.SlotAnimationTrigger(slotLevel, this._seasonCellViewDatas[slotLevel]);
					this._levelProgress.AnimateLevelUp(slotLevel, this._unlockLevelsAnimationConfig.TimeBetweenSlotsInSec);
					yield return new WaitForSeconds(this._unlockLevelsAnimationConfig.TimeBetweenSlotsInSec);
					slotLevel++;
				}
				selectorLevel = slotLevel;
				if (slotLevel <= targetLevel)
				{
					yield return new WaitForSeconds(this._unlockLevelsAnimationConfig.DelayBeforeJumpInSec);
					this._slotSelector.ResetTarget();
					this.JumpRight();
					while (this._scroller.IsTweening)
					{
						yield return null;
					}
				}
			}
			while (slotLevel <= targetLevel);
			yield return new WaitForSeconds(this._unlockLevelsAnimationConfig.DelayBeforeEndButtonUpdateInSec);
			this._currentLevel = targetLevel;
			if (this._currentLevel < this._maxSlots - 1)
			{
				this._buyButtons.EnableLevelButtons();
			}
			this._seasonCellViewDatas[this._currentLevel].IsCurrentLevel = true;
			this._scroller.RefreshActiveCellViews();
			this.UpdateBuyLevelButton();
			this.SetupByPremiumState(true);
			this._slotSelector.ResetTarget();
			this._isAnimatingUnlockLevels = false;
			onAnimationFinished();
			this._mainCanvasGroup.interactable = true;
			yield break;
		}

		private void SetAnimationSeasonScrollerData(int slotLevel, int targetLevel)
		{
			this._seasonCellViewDatas[slotLevel].FreeSlotData.IsLocked = false;
			this._seasonCellViewDatas[slotLevel].PremiumSlotData.IsLocked = false;
			this._seasonCellViewDatas[slotLevel].IsCurrentLevel = (slotLevel == targetLevel);
			this._seasonCellViewDatas[slotLevel].LevelType = ((slotLevel != targetLevel) ? UnityUiBattlepassSeasonCellView.LevelType.Unlocked : UnityUiBattlepassSeasonCellView.LevelType.Current);
			this._seasonScrollerArtPreviewDatas[slotLevel].FreeDataArtPreview.LoreData.IsLocked = false;
			this._seasonScrollerArtPreviewDatas[slotLevel].PremiumDataArtPreview.LoreData.IsLocked = false;
		}

		private void OnUnlockPremiumButton()
		{
			this._buyUiActions.OnUnlockPremiumButtonClick(false);
		}

		private void OnBuyLevelsButton()
		{
			this.OnBuyLevels(this.GetSelectedLevel());
		}

		private void OnBuyAllLevelsButton()
		{
			this.OnBuyLevels(this._maxSlots - 1);
		}

		private void OnBuyLevels(int targetLevel)
		{
			int num = 1;
			if (targetLevel > this._currentLevel)
			{
				num = targetLevel - this._currentLevel;
			}
			this._buyUiActions.OnBuyLevelsButtonClick(num, this._currentLevel + num);
		}

		public void ReloadArtPreviewScene()
		{
			this._artPreview.ReloadModelViewerScene();
		}

		[Header("[Components]")]
		[SerializeField]
		private CanvasGroup _mainCanvasGroup;

		[SerializeField]
		private Animation _mainAnimation;

		[SerializeField]
		private EnhancedScroller _scroller;

		[SerializeField]
		private Button _leftButton;

		[SerializeField]
		private Button _rightButton;

		[SerializeField]
		private Text _pageText;

		[SerializeField]
		private UnityUiBattlepassSeasonSelector _slotSelector;

		[SerializeField]
		private CanvasGroup _premiumLockCanvasGroup;

		[SerializeField]
		private Animation _unlockPremiumAnimation;

		[Header("[Config]")]
		[SerializeField]
		private float _pageTextAlphaOnScroll = 0.2f;

		[SerializeField]
		private Color _pageTextColor;

		[SerializeField]
		private float _delayAfterUnlockPremiumAnimationInSec = 0.4f;

		[SerializeField]
		private EnhancedScroller.TweenType _scrollerJumpTweenType = EnhancedScroller.TweenType.easeInOutQuad;

		[SerializeField]
		private float _scrollerJumpTweenTimeInSec = 0.5f;

		[Header("[Cell]")]
		[SerializeField]
		private UnityUiBattlepassSeasonCellView _slotCellViewPrefab;

		[Header("[Views]")]
		[SerializeField]
		private UnityUiBattlepassArtPreview _artPreview;

		[SerializeField]
		private UnityUiBattlepassSeasonBuyButtons _buyButtons;

		[Header("[Animation Configs]")]
		[SerializeField]
		private UnityUiBattlepassSeasonScroller.UnlockLevelsAnimationConfig _unlockLevelsAnimationConfig;

		private UnityUiBattlepassSeasonCellView.SeasonCellViewData[] _seasonCellViewDatas;

		private UnityUiBattlepassSeasonScroller.SeasonScrollerArtPreviewData[] _seasonScrollerArtPreviewDatas;

		private bool _userHasPremium;

		private float _slotCellViewWidth;

		private int _currentSlotIndex;

		private int _pageSize;

		private int _maxSlots;

		private int _numPages;

		private bool _isJumping;

		private int _jumpTargetIndex;

		private int _currentLevel;

		private int _buyLevelPriceValue;

		private int _selectedFreeSlotLevel;

		private int _selectedPremiumSlotLevel;

		private IBattlepassBuyUiActions _buyUiActions;

		private IBattlepassLevelProgress _levelProgress;

		private bool _isAnimatingUnlockLevels;

		private int _unlockAnimationTargetLevel;

		[Serializable]
		private struct UnlockLevelsAnimationConfig
		{
			public float TimeBetweenSlotsInSec;

			public float DelayBeforeJumpInSec;

			public float DelayBeforeEndButtonUpdateInSec;
		}

		public struct SeasonScrollerArtPreviewData
		{
			public UnityUiBattlepassArtPreview.ArtPreviewData FreeDataArtPreview;

			public UnityUiBattlepassArtPreview.ArtPreviewData PremiumDataArtPreview;
		}

		public delegate void SlotAnimationTriggerDelegate(int slotLevel, UnityUiBattlepassSeasonCellView.SeasonCellViewData cellViewData);

		public delegate void SlotTransformRequestDelegate(int slotLevel);
	}
}
