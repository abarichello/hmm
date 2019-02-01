using System;
using System.Collections;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.UnityUI;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassView : MonoBehaviour, IBattlepassView, IBattlepassBuyUiActions
	{
		protected void Awake()
		{
			this._battlepassComponent = this._battlepassComponentAsset;
			this._battlepassInfoComponent = this._battlepassInfoComponentAsset;
			this._battlepassPremiumShopComponent = this._battlepassPremiumShopComponentAsset;
			this._battlepassRewardComponent = this._battlepassRewardComponentAsset;
			this._seasonScroller.gameObject.SetActive(true);
			this._missionsTabPresenter.gameObject.SetActive(false);
			this._translatedTitleText = Language.Get("BATTLEPASS_TITLE_NAME", TranslationSheets.Battlepass);
			this._translatedTabRewardText = Language.Get("BATTLEPASS_TAB_REWARDS", TranslationSheets.Battlepass);
			this._translatedTabMissionsText = Language.Get("BATTLEPASS_TAB_MISSIONS", TranslationSheets.Battlepass);
		}

		protected void Start()
		{
			this._mainWindowCanvasGroup.alpha = 0f;
			GUIUtils.ResetAnimation(this._mainWindowAnimation);
			this._mainWindowCanvasGroup.interactable = false;
			this._battlepassComponent.RegisterView(this);
			base.gameObject.SetActive(false);
		}

		protected void OnDestroy()
		{
			base.StopAllCoroutines();
		}

		public void Setup(BattlepassViewData battlepassViewData)
		{
			this._userHasPremium = battlepassViewData.DataSeason.UserHasPremium;
			this._rewardsToggleInfo.Setup(true, this._translatedTabRewardText, HmmUiText.TextStyles.Default, new UnityAction<bool>(this.RewardTabOnValueChanged));
			this._missionsToggleInfo.Setup(false, this._translatedTabMissionsText, HmmUiText.TextStyles.Default, new UnityAction<bool>(this.MissionsTabOnValueChanged));
			this._titleInfo.Setup(this._translatedTitleText, HmmUiText.TextStyles.UpperCase, this._translatedTabRewardText, HmmUiText.TextStyles.UpperCase);
			BattlepassViewData.BattlepassViewDataLevels dataLevels = battlepassViewData.DataLevels;
			this._currentLevel = dataLevels.CurrentLevel;
			TimeSpan remainingTime = battlepassViewData.DataTime.GetRemainingTime();
			string formatedText = string.Format("{0}{1}{2}", "<color=#{0}>", Language.Get("BATTLEPASS_ENDS_IN", TranslationSheets.Battlepass), "</color> {1}");
			this._timerInfo.Setup(remainingTime, new TimeSpan(1, 0, 0, 0), formatedText);
			this._seasonScroller.Setup(this, this._levelProgressView, 5, dataLevels.MaxLevels, dataLevels.CurrentLevel, battlepassViewData.DataSeason);
			this._missionsTabPresenter.Setup(this, battlepassViewData);
			this._levelProgressView.Setup(dataLevels);
		}

		public void RefreshData(BattlepassViewData viewData)
		{
			this._seasonScroller.RefreshData(viewData.DataLevels, viewData.DataSeason);
			this._missionsTabPresenter.Setup(this, viewData);
			this._levelProgressView.Setup(viewData.DataLevels);
			this._currentLevel = viewData.DataLevels.CurrentLevel;
		}

		public void EnableInteraction()
		{
			this._mainWindowCanvasGroup.interactable = true;
		}

		public void TryToOpenPremiumShop()
		{
			if (!this._userHasPremium)
			{
				this.OnUnlockPremiumButtonClick(this._missionsTabPresenter.gameObject.activeSelf);
			}
		}

		private void RewardTabOnValueChanged(bool isOn)
		{
			this._seasonScroller.SetVisibility(isOn, false);
			this._titleInfo.SetSubtitle((!isOn) ? this._translatedTabMissionsText : this._translatedTabRewardText, HmmUiText.TextStyles.UpperCase);
			if (isOn)
			{
				this._battlepassRewardComponent.TryToOpenRewardsToClaim(null);
			}
		}

		private void MissionsTabOnValueChanged(bool isOn)
		{
			this._missionsTabPresenter.SetVisibility(isOn, this._userHasPremium);
			if (isOn)
			{
				this._battlepassComponent.MarkMissionsAsSeen();
			}
		}

		public void SetVisibility(bool isVisible, bool hasRewardsToClaim, bool imediate)
		{
			if (isVisible)
			{
				base.gameObject.SetActive(true);
			}
			if (imediate)
			{
				this._isVisible = isVisible;
				if (isVisible)
				{
					this._seasonScroller.SetVisibility(true, true);
					this._mainWindowCanvas.enabled = true;
					this._seasonScroller.SelectSlotForLevel(this._currentLevel);
					this._mainWindowCanvasGroup.interactable = !hasRewardsToClaim;
				}
				else
				{
					this._mainWindowCanvasGroup.interactable = false;
					this._missionsTabPresenter.gameObject.SetActive(false);
					this._mainWindowCanvas.enabled = false;
					base.gameObject.SetActive(false);
				}
			}
			else
			{
				base.StopAllCoroutines();
				base.StartCoroutine(this.SetVisibilityCoroutine(isVisible, hasRewardsToClaim));
			}
		}

		public bool IsVisible()
		{
			return this._isVisible;
		}

		private IEnumerator SetVisibilityCoroutine(bool isVisible, bool hasRewardsToClaim)
		{
			this._mainWindowCanvasGroup.interactable = false;
			this._isVisible = isVisible;
			if (isVisible)
			{
				this._seasonScroller.SetVisibility(true, true);
				this._mainWindowCanvas.enabled = true;
				this._seasonScroller.SelectSlotForLevel(this._currentLevel);
			}
			GUIUtils.PlayAnimation(this._mainWindowAnimation, !isVisible, 1f, string.Empty);
			yield return new WaitForSeconds(this._mainWindowAnimation.clip.length);
			if (!isVisible)
			{
				this._missionsTabPresenter.gameObject.SetActive(false);
				this._mainWindowCanvas.enabled = false;
				base.gameObject.SetActive(false);
			}
			else
			{
				this._seasonScroller.JumpToPageOfSlotIndex(this._currentLevel, true);
				this._mainWindowCanvasGroup.interactable = !hasRewardsToClaim;
			}
			yield break;
		}

		[UnityUiComponentCall]
		public void OnBackButtonClick()
		{
			this._battlepassComponent.HideMetalpassWindow(false);
		}

		[UnityUiComponentCall]
		public void OnInfoButtonClick()
		{
			this._mainWindowCanvasGroup.interactable = false;
			this._battlepassInfoComponent.ShowInfoWindow(new Action(this.OnInfoWindowClose));
		}

		private void OnInfoWindowClose()
		{
			this._mainWindowCanvasGroup.interactable = true;
		}

		public void OnBuyLevelsButtonClick(int quantity, int targetLevel)
		{
			this._mainWindowCanvasGroup.interactable = false;
			this._levelBuyTarget = targetLevel;
			this._battlepassComponent.MetalpassBuyLevelRequest(quantity, new Action(this.OnBuyLevel), new Action(this.OnBuyLevelClose));
		}

		private void OnBuyLevel()
		{
			this._buyLevelConfirmed = true;
		}

		private void OnBuyLevelClose()
		{
			this._mainWindowCanvasGroup.interactable = true;
			if (this._buyLevelConfirmed)
			{
				this._buyLevelConfirmed = false;
				this._battlepassComponent.SetLevelFake(this._levelBuyTarget);
				this._seasonScroller.ShowUnlockLevelAnimation(this._levelBuyTarget, new Action(this.OnBuyAnimationFinished));
			}
		}

		public void OnUnlockPremiumButtonClick(bool fromMissionWindow)
		{
			if (this._userHasPremium)
			{
				UnityUiBattlepassView.Log.WarnFormat("OnUnlockPremiumButtonClick, but user already unlocked premium. [fromMissionWindow={0}]", new object[]
				{
					fromMissionWindow
				});
				return;
			}
			if (this._battlepassPremiumShopComponent.IsPremiumShopSceneLoad())
			{
				return;
			}
			this._openedUnlockPremiumFromMissionWindow = fromMissionWindow;
			this._mainWindowCanvasGroup.interactable = false;
			this._battlepassPremiumShopComponent.ShowPremiumShopWindow(new Action<int>(this.OnUnlockPremium), new Action(this.OnUnlockPremiumClose));
		}

		private void OnUnlockPremium(int levelTarget)
		{
			this._buyPremiumConfirmed = true;
			this._userHasPremium = true;
			this._levelBuyTarget = levelTarget;
		}

		private void OnUnlockPremiumClose()
		{
			this._mainWindowCanvasGroup.interactable = true;
			if (this._buyPremiumConfirmed)
			{
				this._buyPremiumConfirmed = false;
				Action onAnimationFinished;
				if (this._levelBuyTarget > 0)
				{
					onAnimationFinished = new Action(this.OnUnlockPremiumPlusLevels);
				}
				else
				{
					onAnimationFinished = new Action(this.OnBuyAnimationFinished);
				}
				this._seasonScroller.ShowUnlockPremiumAnimation(onAnimationFinished);
				this._openedUnlockPremiumFromMissionWindow = false;
			}
			else if (this._openedUnlockPremiumFromMissionWindow)
			{
				this._openedUnlockPremiumFromMissionWindow = false;
				this._missionsToggleInfo.SetToggleValue(true);
			}
		}

		private void OnUnlockPremiumPlusLevels()
		{
			this._seasonScroller.ShowUnlockLevelAnimation(this._levelBuyTarget, new Action(this.OnBuyAnimationFinished));
		}

		private void OnBuyAnimationFinished()
		{
			this._battlepassRewardComponent.TryToOpenRewardsToClaim(new Action(this.OnRewardWindowClosed));
		}

		public void RewardWindowClosed()
		{
			this.OnRewardWindowClosed();
		}

		private void OnRewardWindowClosed()
		{
			this.EnableInteraction();
			this._seasonScroller.ReloadArtPreviewScene();
			this._seasonScroller.SelectSlotForLevel(this._currentLevel);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(UnityUiBattlepassView));

		[Header("[Infra]")]
		[SerializeField]
		private BattlepassComponent _battlepassComponentAsset;

		[SerializeField]
		private BattlepassInfoComponent _battlepassInfoComponentAsset;

		[SerializeField]
		private BattlepassPremiumShopComponent _battlepassPremiumShopComponentAsset;

		[SerializeField]
		private BattlepassRewardComponent _battlepassRewardComponentAsset;

		private IBattlepassComponent _battlepassComponent;

		private IBattlepassInfoComponent _battlepassInfoComponent;

		private IBattlepassPremiumShopComponent _battlepassPremiumShopComponent;

		private IBattlepassRewardComponent _battlepassRewardComponent;

		[Header("[Main UI Components]")]
		[SerializeField]
		private Canvas _mainWindowCanvas;

		[SerializeField]
		private CanvasGroup _mainWindowCanvasGroup;

		[SerializeField]
		private Animation _mainWindowAnimation;

		[SerializeField]
		private Button _backButton;

		[SerializeField]
		private Button _detailsButton;

		[Header("[Sub Views]")]
		[SerializeField]
		private UnityUiToggleInfo _rewardsToggleInfo;

		[SerializeField]
		private UnityUiToggleInfo _missionsToggleInfo;

		[SerializeField]
		private UnityUiTitleInfo _titleInfo;

		[SerializeField]
		private UnityUiTimerInfo _timerInfo;

		[SerializeField]
		private UnityUiBattlepassSeasonScroller _seasonScroller;

		[SerializeField]
		private UnityUiBattlepassMissionsTabPresenter _missionsTabPresenter;

		[SerializeField]
		private UnityUiBattlepassLevelProgress _levelProgressView;

		[Header("[Test Only]")]
		[SerializeField]
		private BattlepassViewData _battlepassViewDataTest;

		private bool _isVisible;

		private int _currentLevel;

		private bool _userHasPremium;

		private string _translatedTitleText;

		private string _translatedTabRewardText;

		private string _translatedTabMissionsText;

		private bool _buyPremiumConfirmed;

		private bool _buyLevelConfirmed;

		private int _levelBuyTarget;

		private bool _openedUnlockPremiumFromMissionWindow;
	}
}
