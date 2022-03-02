using System;
using System.Collections;
using HeavyMetalMachines.Battlepass.Business;
using HeavyMetalMachines.Battlepass.Rewards.Presenter;
using HeavyMetalMachines.Boosters.Business;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.UnityUI;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassView : MonoBehaviour, ILegacyBattlepassView, IBattlepassBuyUiActions
	{
		protected void Awake()
		{
			this._battlepassComponent = this._battlepassComponentAsset;
			this._battlepassInfoComponent = this._battlepassInfoComponentAsset;
			this._battlepassPremiumShopComponent = this._battlepassPremiumShopComponentAsset;
			this._battlepassRewardComponent = this._battlepassRewardComponentAsset;
			this._seasonScroller.gameObject.SetActive(true);
			this._missionsTabPresenter.gameObject.SetActive(false);
			this._translatedTitleText = Language.Get("BATTLEPASS_TITLE_NAME", TranslationContext.Battlepass);
			this._translatedTabRewardText = Language.Get("BATTLEPASS_TAB_REWARDS", TranslationContext.Battlepass);
			this._translatedTabMissionsText = Language.Get("BATTLEPASS_TAB_MISSIONS", TranslationContext.Battlepass);
			ObservableExtensions.Subscribe<Unit>(this._titleInfo.ObserveInfoButtonClick(), delegate(Unit _)
			{
				this.OnDetailsButtonClick();
			});
		}

		protected void Start()
		{
			this._mainWindowCanvasGroup.alpha = 0f;
			GUIUtils.ResetAnimation(this._mainWindowAnimation);
			this._mainWindowCanvasGroup.interactable = false;
			IStoreBusinessFactory storeBusinessFactory = this._diContainer.Resolve<IStoreBusinessFactory>();
			IGetLocalPlayerXpBooster igetLocalPlayerXpBooster = this._diContainer.Resolve<IGetLocalPlayerXpBooster>();
			IGetBattlepassSeason igetBattlepassSeason = this._diContainer.Resolve<IGetBattlepassSeason>();
			this._battlepassComponent.SetStoreBusinessFactory(storeBusinessFactory);
			this._battlepassComponent.SetIGetLocalPlayerXpBooster(igetLocalPlayerXpBooster);
			this._battlepassComponent.SetIGetBattlepassSeason(igetBattlepassSeason);
			this._battlepassComponent.RegisterView(this);
			base.gameObject.SetActive(false);
			BattlepassProgressScriptableObject.OnBattlepassProgressSet += this.OnBattlepassProgressSet;
		}

		protected void OnDestroy()
		{
			this._battlepassComponent.UnregisterView();
			BattlepassProgressScriptableObject.OnBattlepassProgressSet -= this.OnBattlepassProgressSet;
			base.StopAllCoroutines();
		}

		public void Setup(BattlepassViewData battlepassViewData)
		{
			this._userHasPremium = battlepassViewData.DataSeason.UserHasPremium;
			this._rewardsToggleInfo.Setup(true, this._translatedTabRewardText, HmmUiText.TextStyles.Default, new UnityAction<bool>(this.RewardTabOnValueChanged));
			this._missionsToggleInfo.Setup(false, this._translatedTabMissionsText, HmmUiText.TextStyles.Default, new UnityAction<bool>(this.MissionsTabOnValueChanged));
			this._titleInfo.Setup(this._translatedTitleText, HmmUiText.TextStyles.UpperCase, this._translatedTabRewardText, HmmUiText.TextStyles.UpperCase, string.Empty, HmmUiText.TextStyles.UpperCase, true);
			BattlepassViewData.BattlepassViewDataLevels dataLevels = battlepassViewData.DataLevels;
			this._currentLevel = dataLevels.CurrentLevel;
			TimeSpan remainingTime = battlepassViewData.DataTime.GetRemainingTime();
			string formatedText = string.Format("{0}{1}{2}", "<color=#{0}>", Language.Get("BATTLEPASS_ENDS_IN", TranslationContext.Battlepass), "</color> {1}");
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
			this._titleInfo.Subtitle = ((!isOn) ? this._translatedTabMissionsText : this._translatedTabRewardText);
			this._titleInfo.SubtitleTextStyle = HmmUiText.TextStyles.UpperCase;
			if (isOn)
			{
				this.TryToOpenRewardsToClaim();
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

		[Obsolete("Use MainMenuTree.BattlepassNode")]
		public void SetVisibility(bool isVisible, bool hasRewardsToClaim, bool imediate)
		{
			UnityUiBattlepassView.Log.WarnFormat("obsolete SetVisibility. Use MainMenuTree.BattlepassNode", new object[0]);
		}

		public bool IsVisible()
		{
			return this._isVisible;
		}

		[Obsolete]
		private IEnumerator SetVisibilityCoroutine(bool isVisible, bool hasRewardsToClaim)
		{
			UnityUiBattlepassView.Log.Warn("using obsolete SetVisibilityCoroutine");
			yield break;
		}

		[UnityUiComponentCall]
		[Obsolete("USer mainmenutree.mainmenuNode")]
		public void OnBackButtonClick()
		{
			UnityUiBattlepassView.Log.WarnFormat("Obsolete OnBackButtonClick", new object[0]);
		}

		private void OnDetailsButtonClick()
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
			if (this._buyLevelConfirmed)
			{
				this._buyLevelConfirmed = false;
				this._battlepassComponent.SetLevelFake(this._levelBuyTarget);
				this._seasonScroller.ShowUnlockLevelAnimation(this._levelBuyTarget, new Action(this.OnBuyAnimationFinished));
			}
			else
			{
				this.EnableInteraction();
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
			if (this._buyPremiumConfirmed)
			{
				this._buyPremiumConfirmed = false;
				this._battlepassComponent.GivePremiumFake();
				Action onAnimationFinished;
				if (this._levelBuyTarget > 0)
				{
					onAnimationFinished = new Action(this.OnUnlockPremiumPlusLevels);
					this._battlepassComponent.SetLevelFake(this._levelBuyTarget);
				}
				else
				{
					onAnimationFinished = new Action(this.OnBuyAnimationFinished);
				}
				this._seasonScroller.ShowUnlockPremiumAnimation(onAnimationFinished);
				this._openedUnlockPremiumFromMissionWindow = false;
			}
			else
			{
				if (this._openedUnlockPremiumFromMissionWindow)
				{
					this._openedUnlockPremiumFromMissionWindow = false;
					this._missionsToggleInfo.SetToggleValue(true);
				}
				this.EnableInteraction();
			}
		}

		private void OnUnlockPremiumPlusLevels()
		{
			this._seasonScroller.ShowUnlockLevelAnimation(this._levelBuyTarget, new Action(this.OnBuyAnimationFinished));
		}

		private void OnBuyAnimationFinished()
		{
			ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(this._battlepassRewardsPresenter.TryToOpenRewardsToClaim(), delegate(bool opened)
			{
				if (!opened)
				{
					this.EnableInteraction();
				}
				else
				{
					this.OnRewardWindowClosed();
				}
			}));
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

		private void OnBattlepassProgressSet(BattlepassProgress battlepassProgress)
		{
			if (!this._mainWindowCanvas.isActiveAndEnabled)
			{
				return;
			}
			base.StartCoroutine(this.WaitToCheckRewardToClaim());
		}

		private IEnumerator WaitToCheckRewardToClaim()
		{
			yield return new WaitForSeconds(1f);
			if (this._seasonScroller.IsAnimatingUnlockLevels() || !this._retryOpenRewardWindow)
			{
				UnityUiBattlepassView.Log.Debug("_seasonScroller.IsAnimatingUnlockLevels() || !_retryOpenRewardWindow");
				yield break;
			}
			ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(this._battlepassRewardsPresenter.TryToOpenRewardsToClaim(), delegate(bool opened)
			{
				if (!opened)
				{
					this.EnableInteraction();
				}
				else
				{
					this._retryOpenRewardWindow = false;
					this.OnRewardWindowClosed();
				}
			}));
			yield break;
		}

		public IObservable<Unit> AnimateShow()
		{
			UnityAnimation unityAnimation = new UnityAnimation(this._mainWindowAnimation, "battlepass_in");
			return Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.BeforeAnimationShow();
			}), unityAnimation.Play()), delegate(Unit _)
			{
				this.AfterAnimationShow();
			});
		}

		private void BeforeAnimationShow()
		{
			this.Setup(this._battlepassComponent.GetBattlepassViewData());
			base.gameObject.SetActive(true);
			this._seasonScroller.SetVisibility(true, true);
			this._mainWindowCanvas.enabled = true;
			this._mainWindowCanvasGroup.interactable = false;
			this._isVisible = false;
			this._seasonScroller.SelectSlotForLevel(this._currentLevel);
		}

		private void AfterAnimationShow()
		{
			this._mainWindowCanvasGroup.interactable = true;
			this._retryOpenRewardWindow = true;
			this._seasonScroller.JumpToPageOfSlotIndex(this._currentLevel, true);
			this._mainWindowCanvasGroup.interactable = true;
			this._isVisible = true;
			this.TryToOpenRewardsToClaim();
		}

		public IObservable<Unit> AnimateHide()
		{
			UnityAnimation unityAnimation = new UnityAnimation(this._mainWindowAnimation, "battlepass_out");
			return Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.BeforeAnimationHide();
			}), unityAnimation.Play()), delegate(Unit _)
			{
				this.AfterAnimationHide();
			});
		}

		private void BeforeAnimationHide()
		{
			this._battlepassPremiumShopComponent.HidePremiumShopWindow();
			this._battlepassRewardComponent.HideRewardWindow();
			this._mainWindowCanvasGroup.interactable = false;
		}

		private void AfterAnimationHide()
		{
			this._mainWindowCanvas.enabled = false;
			this._missionsTabPresenter.gameObject.SetActive(false);
			base.gameObject.SetActive(false);
		}

		private void TryToOpenRewardsToClaim()
		{
			ObservableExtensions.Subscribe<bool>(this._battlepassRewardsPresenter.TryToOpenRewardsToClaim());
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

		[InjectOnClient]
		private DiContainer _diContainer;

		[InjectOnClient]
		private IBattlepassRewardsPresenter _battlepassRewardsPresenter;

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

		private bool _retryOpenRewardWindow;
	}
}
