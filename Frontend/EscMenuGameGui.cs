using System;
using System.Collections.Generic;
using HeavyMetalMachines.Crossplay.Presenter;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Publishing;
using Hoplon.Input.UiNavigation.AxisSelector;
using Hoplon.Logging;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class EscMenuGameGui : EscMenuScreenGui
	{
		private IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public override void Show()
		{
			base.Show();
			this.AdaptLanguageToConsolePlatform();
			this._scrollView.ResetPosition();
			this.CheckForEnableCrossplayOption();
			this.CheckForEnableGadgetCursorOption();
		}

		private void Start()
		{
			this._disposables = new CompositeDisposable();
			this.InstallCrossplayToggle();
			this.InstallGadgetsCursorToggle();
		}

		private void OnDestroy()
		{
			if (this._disposables != null)
			{
				this._disposables.Dispose();
				this._disposables = null;
			}
		}

		private void InstallCrossplayToggle()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(Observable.Where<bool>(Observable.SelectMany<Unit, bool>(Observable.Merge<Unit>(this.EnableCrossplayToggle.OnToggleOn(), new IObservable<Unit>[]
			{
				this.EnableCrossplayToggle.OnToggleOff()
			}), (Unit _) => this._crossplayFeedbackWindowPresenter.Show(this.EnableCrossplayToggle.IsOn)), (bool result) => !result), delegate(bool _)
			{
				this.EnableCrossplayToggle.IsOn = GameHubBehaviour.Hub.Options.Game.CrossplayEnable;
			}));
			this._disposables.Add(disposable);
		}

		private void InstallGadgetsCursorToggle()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Merge<Unit>(this._showGadgetsCursorToggle.OnToggleOn(), new IObservable<Unit>[]
			{
				this._showGadgetsCursorToggle.OnToggleOff()
			}), delegate(Unit _)
			{
				this.ApplyGadgetCursorToggleChange();
			}));
			this._disposables.Add(disposable);
		}

		private void ApplyGadgetCursorToggleChange()
		{
			GameHubBehaviour.Hub.Options.Game.ShowGadgetsCursor = this._showGadgetsCursorToggle.IsOn;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		private void CheckForEnableGadgetCursorOption()
		{
			if (this._getCurrentPublisher.Get() == Publishers.Psn)
			{
				this._showGadgetsCursorToggle.IsInteractable = false;
			}
		}

		private void CheckForEnableCrossplayOption()
		{
			if (this.ShouldHideCrossplayOption())
			{
				ActivatableExtensions.Deactivate(this._crossplayGroupActivatable);
				return;
			}
			ActivatableExtensions.Activate(this._crossplayGroupActivatable);
			this.SetCrossplayInteractability();
		}

		private bool ShouldHideCrossplayOption()
		{
			Publisher publisher = this._getCurrentPublisher.Get();
			return publisher != Publishers.Psn && publisher != Publishers.XboxLive;
		}

		private void SetCrossplayInteractability()
		{
			Publisher publisher = this._getCurrentPublisher.Get();
			if (publisher == Publishers.XboxLive && this._getCrossPlayRestrictionIsEnabled.Get())
			{
				this.CrossplayDisabledLabel.Text = Language.Get("HINT_CROSSPLAY_DISABLED_XBOX_FEEDBACK", TranslationContext.WelcomeScreen);
				this.EnableCrossplayToggle.IsInteractable = false;
				this._crossplayFeedbackDisableActivatable.SetActive(true);
				return;
			}
			IGetThenObserveMatchmakingQueueState getThenObserveMatchmakingQueueState = this._diContainer.Resolve<IGetThenObserveMatchmakingQueueState>();
			bool flag = this._stateMachine.CurrentStateKind == GameState.GameStateKind.MainMenu;
			MatchmakingQueueStep step = getThenObserveMatchmakingQueueState.Get().Step;
			bool flag2 = step == null || step == 2;
			bool flag3 = flag && flag2;
			this.CrossplayDisabledLabel.Text = Language.Get("OPTIONS_CR_HINT_LABEL", TranslationContext.Options);
			this.EnableCrossplayToggle.IsInteractable = flag3;
			this._crossplayFeedbackDisableActivatable.SetActive(!flag3);
		}

		private void AdaptLanguageToConsolePlatform()
		{
			bool flag = Platform.Current.IsConsole();
			this.GameLanguageEditableGroup.SetActive(!flag);
			this.GameLanguageReadOnlyGroup.SetActive(flag);
		}

		public override void Hide()
		{
			base.Hide();
			this.UiNavigationAxisSelector.ClearSelection();
		}

		public override void ReloadCurrent()
		{
			this.GameLanguagePopup.items = new List<string>(GameHubBehaviour.Hub.Options.Game.LanguageNames);
			this.GameLanguagePopup.value = this.GameLanguagePopup.items[GameHubBehaviour.Hub.Options.Game.LanguageIndex];
			this.GameLanguageReadOnlyLabel.text = this.GameLanguagePopup.items[GameHubBehaviour.Hub.Options.Game.LanguageIndex];
			this.EnableCrossplayToggle.IsOn = GameHubBehaviour.Hub.Options.Game.CrossplayEnable;
			this.ShowGadgetsLifebarToggle.Set(GameHubBehaviour.Hub.Options.Game.ShowGadgetsLifebar, false);
			this._showGadgetsCursorToggle.IsOn = GameHubBehaviour.Hub.Options.Game.ShowGadgetsCursor;
			this.ShowPingToggle.Set(GameHubBehaviour.Hub.Options.Game.ShowPing, false);
			this.CounselorActivationToggle.Set(GameHubBehaviour.Hub.Options.Game.CounselorActive, false);
			this.CounselorHudHintToggle.Set(GameHubBehaviour.Hub.Options.Game.CounselorHudHint, false);
			this.ShowLifebarTextToggle.Set(GameHubBehaviour.Hub.Options.Game.ShowLifebarText, false);
			this.ShowPlayerIndicator.Set(GameHubBehaviour.Hub.Options.Game.ShowPlayerIndicator, false);
			this.PlayerIndicatorAlphaSlider.Set(GameHubBehaviour.Hub.Options.Game.PlayerIndicatorAlpha, false);
			this.ShowObjectiveIndicatorToggle.Set(GameHubBehaviour.Hub.Options.Game.ShowObjectiveIndicator, false);
			this.ObjectiveIndicatorAlphaSlider.Set(GameHubBehaviour.Hub.Options.Game.ObjectiveIndicatorAlpha, false);
			this.ObjectiveIndicatorSizeSlider.Set(GameHubBehaviour.Hub.Options.Game.ObjectiveIndicatorSize, false);
			this.ObjectiveIndicatorQuantitySlider.Set(GameHubBehaviour.Hub.Options.Game.ObjectiveIndicatorQuantity, false);
			this.UpdateLabelPlayerIndicatorAlpha();
			this.UpdateLabelObjectiveIndicatorAlpha();
			this.UpdateLabelObjectiveIndicatorSize();
			this.UpdateLabelObjectiveIndicatorQuantity();
		}

		public override void ResetDefault()
		{
			GameHubBehaviour.Hub.Options.Game.ResetDefault();
			GameHubBehaviour.Hub.Options.Game.Apply();
			this.ReloadCurrent();
		}

		public void OnGameLanguagePopupChanged()
		{
			int num = this.GameLanguagePopup.items.FindIndex((string i) => i == this.GameLanguagePopup.value);
			if (GameHubBehaviour.Hub.Options.Game.LanguageIndex == num)
			{
				return;
			}
			GameHubBehaviour.Hub.Options.Game.LanguageIndex = num;
			if (GameHubBehaviour.Hub.Options.Game.HasPendingChanges)
			{
				Guid confirmWindowGuid = Guid.NewGuid();
				LanguageCode languageCode = LanguageLocalizationOptions.LanguageIndexToCode(num);
				ConfirmWindowProperties properties = new ConfirmWindowProperties
				{
					Guid = confirmWindowGuid,
					QuestionText = Language.GetFromLanguage("GAME_LANGUAGE_WARNING_REBOOTREQUIRED", TranslationContext.Options, languageCode),
					OkButtonText = Language.Get("GAME_LANGUAGE_WARNING_OK", TranslationContext.Options),
					OnOk = delegate()
					{
						GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					}
				};
				GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
			}
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnShowGadgetsLifebarChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ShowGadgetsLifebar = this.ShowGadgetsLifebarToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnShowPingChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ShowPing = this.ShowPingToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnGameCounselorActivationChanged()
		{
			GameHubBehaviour.Hub.Options.Game.CounselorActive = this.CounselorActivationToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
			this.ReloadCurrent();
		}

		public void OnCounselorHudHintChanged()
		{
			GameHubBehaviour.Hub.Options.Game.CounselorHudHint = this.CounselorHudHintToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnShowLifebarTextChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ShowLifebarText = this.ShowLifebarTextToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnShowPlayerIndicatorChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ShowPlayerIndicator = this.ShowPlayerIndicator.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnPlayerIndicatorAlphaSliderChanged()
		{
			GameHubBehaviour.Hub.Options.Game.PlayerIndicatorAlpha = this.PlayerIndicatorAlphaSlider.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
			this.UpdateLabelPlayerIndicatorAlpha();
		}

		private void UpdateLabelPlayerIndicatorAlpha()
		{
			this.UpdateLabelAlpha(this._optionsScriptableObject.PlayerIndicatorVisualMaxAlpha, this._optionsScriptableObject.PlayerIndicatorVisualMinAlpha, GameHubBehaviour.Hub.Options.Game.PlayerIndicatorAlpha, this.PlayerIndicatorAlphaLabel);
		}

		public void OnShowObjectiveIndicatorChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ShowObjectiveIndicator = this.ShowObjectiveIndicatorToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void ObjectiveIndicatorAlphaSliderChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ObjectiveIndicatorAlpha = this.ObjectiveIndicatorAlphaSlider.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
			this.UpdateLabelObjectiveIndicatorAlpha();
		}

		private void UpdateLabelObjectiveIndicatorAlpha()
		{
			this.UpdateLabelAlpha(this._optionsScriptableObject.ObjectiveIndicatorVisualMaxAlpha, this._optionsScriptableObject.ObjectiveIndicatorVisualMinAlpha, GameHubBehaviour.Hub.Options.Game.ObjectiveIndicatorAlpha, this.ObjectiveIndicatorAlphaLabel);
		}

		public void ObjectiveIndicatorSizeSliderChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ObjectiveIndicatorSize = this.ObjectiveIndicatorSizeSlider.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
			this.UpdateLabelObjectiveIndicatorSize();
		}

		private void UpdateLabelObjectiveIndicatorSize()
		{
			this.UpdateLabelAlpha(this._optionsScriptableObject.ObjectiveIndicatorVisualMaxSize, this._optionsScriptableObject.ObjectiveIndicatorVisualMinSize, GameHubBehaviour.Hub.Options.Game.ObjectiveIndicatorSize, this.ObjectiveIndicatorSizeLabel);
		}

		public void ObjectiveIndicatorQuantitySliderChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ObjectiveIndicatorQuantity = this.ObjectiveIndicatorQuantitySlider.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
			this.UpdateLabelObjectiveIndicatorQuantity();
		}

		private void UpdateLabelObjectiveIndicatorQuantity()
		{
			this.UpdateLabelAlpha(this._optionsScriptableObject.ObjectiveIndicatorVisualMaxArrowQuantity, this._optionsScriptableObject.ObjectiveIndicatorVisualMinArrowQuantity, GameHubBehaviour.Hub.Options.Game.ObjectiveIndicatorQuantity, this.ObjectiveIndicatorQuantityLabel);
		}

		private void UpdateLabelAlpha(int max, int min, float alpha, UILabel label)
		{
			int num = max - min;
			label.text = (alpha * (float)num + (float)min).ToString("0");
		}

		[Header("[Language]")]
		public UIPopupList GameLanguagePopup;

		public UILabel GameLanguageReadOnlyLabel;

		public GameObject GameLanguageEditableGroup;

		public GameObject GameLanguageReadOnlyGroup;

		[Header("[Counselor]")]
		[SerializeField]
		private UIToggle CounselorActivationToggle;

		[SerializeField]
		private UIToggle CounselorHudHintToggle;

		[Header("[Gadget Feedbacks]")]
		[SerializeField]
		private UIToggle ShowGadgetsLifebarToggle;

		[SerializeField]
		private NguiToggle _showGadgetsCursorToggle;

		[Header("[Ping]")]
		[SerializeField]
		private UIToggle ShowPingToggle;

		[Header("[Lifebar]")]
		[SerializeField]
		private UIToggle ShowLifebarTextToggle;

		[Header("[Player Indicator]")]
		[SerializeField]
		private UIToggle ShowPlayerIndicator;

		[SerializeField]
		private UISlider PlayerIndicatorAlphaSlider;

		[SerializeField]
		private UILabel PlayerIndicatorAlphaLabel;

		[Header("[Objective Indicator]")]
		[SerializeField]
		private UIToggle ShowObjectiveIndicatorToggle;

		[SerializeField]
		private UISlider ObjectiveIndicatorAlphaSlider;

		[SerializeField]
		private UILabel ObjectiveIndicatorAlphaLabel;

		[SerializeField]
		private UISlider ObjectiveIndicatorSizeSlider;

		[SerializeField]
		private UILabel ObjectiveIndicatorSizeLabel;

		[SerializeField]
		private UISlider ObjectiveIndicatorQuantitySlider;

		[SerializeField]
		private UILabel ObjectiveIndicatorQuantityLabel;

		[Header("[Crossplay]")]
		[SerializeField]
		private NguiToggle EnableCrossplayToggle;

		[SerializeField]
		private UILabel CrossplayDisabledLabel;

		[SerializeField]
		private GameObjectActivatable _crossplayGroupActivatable;

		[SerializeField]
		private GameObjectActivatable _crossplayFeedbackDisableActivatable;

		[Header("[Configs]")]
		[SerializeField]
		private OptionsScriptableObject _optionsScriptableObject;

		[Header("[Scroll]")]
		[SerializeField]
		private UIScrollView _scrollView;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[Inject]
		private ICrossplayFeedbackWindowPresenter _crossplayFeedbackWindowPresenter;

		[Inject]
		private DiContainer _diContainer;

		[Inject]
		private IStateMachine _stateMachine;

		[Inject]
		private IGetCurrentPublisher _getCurrentPublisher;

		[Inject]
		private IGetCrossPlayRestrictionIsEnabled _getCrossPlayRestrictionIsEnabled;

		[Inject]
		private ILogger<EscMenuGameGui> _logger;

		private CompositeDisposable _disposables;
	}
}
