using System;
using System.Collections;
using System.Collections.Generic;
using FMod;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Tutorial.Behaviours;
using HeavyMetalMachines.Tutorial.UnityUI;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Holoville.HOTween;
using Hoplon.Input;
using Hoplon.Input.Business;
using Hoplon.Input.UiNavigation;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial
{
	public class TutorialUIController : GameHubBehaviour
	{
		public static TutorialUIController Instance
		{
			get
			{
				return TutorialUIController._instance;
			}
		}

		public static BoxCollider screenCollider { get; private set; }

		public bool IsEnabled
		{
			get
			{
				return !GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipTutorial) && !GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish);
			}
		}

		public bool IsInTutorialTransition
		{
			get
			{
				return this._isInTutorialTransition;
			}
			private set
			{
				TutorialUIController.Log.WarnFormat("Setting IsInTutorialTransition {0} {1}", new object[]
				{
					value,
					Time.time
				});
				this._isInTutorialTransition = value;
				if (value)
				{
					TutorialUIController.screenCollider.enabled = true;
				}
			}
		}

		public bool CanStartMatch { get; set; }

		public bool HasDialogActive
		{
			get
			{
				return this._currentTutorialData != null && (this._currentTutorialData.dialogType == TutorialData.DialogTypes.TutorialGuy || this._currentTutorialData.dialogType == TutorialData.DialogTypes.Informative);
			}
		}

		public void Awake()
		{
			if (GameHubBehaviour.Hub == null)
			{
				return;
			}
			TutorialUIController._instance = this;
		}

		private void Start()
		{
			if (!this._isInitialized)
			{
				this.Init();
			}
		}

		private void Init()
		{
			TutorialUIController._instance = this;
			this._isInitialized = true;
			if (!Application.isPlaying)
			{
				return;
			}
			this.SetupOverlay();
			Vector3 size;
			size..ctor((float)Screen.width, (float)Screen.height, 1f);
			TutorialUIController.screenCollider = base.gameObject.AddComponent<BoxCollider>();
			TutorialUIController.screenCollider.size = size;
			this.tutorialGuyPanel.SetWindowVisibility(false);
			this.ObjectivePanel.SetWindowVisibility(false);
			this.BottomPanelComponent.Load();
			this.InformativePanel.SetWindowVisibility(false);
			TweenAlpha.Begin(this.ObjectivePanelButtonAFeedback.Glow.gameObject, 0f, 0f);
			TweenAlpha.Begin(this.ObjectivePanelButtonWFeedback.Glow.gameObject, 0f, 0f);
			TweenAlpha.Begin(this.ObjectivePanelButtonDFeedback.Glow.gameObject, 0f, 0f);
			TweenAlpha.Begin(this.ObjectivePanelButtonDriftFeedback.Glow.gameObject, 0f, 0f);
			this.ObjectivePanelButtonAFeedback.gameObject.SetActive(false);
			this.ObjectivePanelButtonAFeedback.ProgressBar.value = 0f;
			this.ObjectivePanelButtonWFeedback.gameObject.SetActive(false);
			this.ObjectivePanelButtonAFeedback.ProgressBar.value = 0f;
			this.ObjectivePanelButtonDFeedback.gameObject.SetActive(false);
			this.ObjectivePanelButtonDFeedback.ProgressBar.value = 0f;
			this.ObjectivePanelButtonDriftFeedback.gameObject.SetActive(false);
			this.ObjectivePanelButtonDriftFeedback.ProgressBar.value = 0f;
			TweenAlpha.Begin(base.gameObject, 0f, 0f);
			this._uiTablesToRefresh = base.GetComponentsInChildren<UITable>(true);
			this.TryToInstallControlListeners();
		}

		public void StartTutorial(TutorialTrigger tutorialTrigger, string tutorialName, EventDelegate onButtonEvent, GameObject targetGO = null)
		{
			if (!this._isInitialized)
			{
				this.Init();
			}
			this.CurrentTutorialTrigger = tutorialTrigger;
			base.StartCoroutine(this.StartTutorialNextFrame(tutorialName, onButtonEvent, targetGO));
		}

		public void ShowDialog(TutorialData tutorialData, EventDelegate onButtonEvent)
		{
			if (!this._isInitialized)
			{
				this.Init();
			}
			this.CurrentTutorialTrigger = null;
			this.TheLegendOfTheDisabledGameObject.SetActive(true);
			this._currentTutorialData = tutorialData;
			this.SetupTutorialDialog(onButtonEvent, null);
		}

		public IEnumerator StartTutorialNextFrame(string tutorialName, EventDelegate onButtonEvent, GameObject targetGO = null)
		{
			this.TheLegendOfTheDisabledGameObject.SetActive(true);
			if (this._currentTutorialData != null && this._currentTutorialData.Name == tutorialName)
			{
				yield break;
			}
			yield return null;
			this.IsInTutorialTransition = false;
			Dictionary<string, TutorialData> tutorialDatasDictionary = GameHubBehaviour.Hub.TutorialHub.TutorialControllerInstance.tutorialDatasDictionary;
			if (!tutorialDatasDictionary.TryGetValue(tutorialName, out this._currentTutorialData))
			{
				yield break;
			}
			TutorialUIController.Log.DebugFormat("Tutorial started={0}", new object[]
			{
				this._currentTutorialData.Name
			});
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(39, string.Format("Tutorial={0}", this._currentTutorialData.Name), false);
			this.SetupTutorialDialog(onButtonEvent, targetGO);
			yield break;
		}

		private void SetupTutorialDialog(EventDelegate onButtonEvent, GameObject targetGO = null)
		{
			this._showDelay = this._currentTutorialData.ShowDelay;
			if (this._showDelay > 0f)
			{
				this._delayedOnButtonEvent = onButtonEvent;
				this._delayedTargetGO = targetGO;
			}
			else
			{
				this.InnerSetupTutorialDialog(onButtonEvent, targetGO);
			}
		}

		private void InnerSetupTutorialDialog(EventDelegate onClosed, GameObject targetGO = null)
		{
			if (!this.UnderTutorial)
			{
				this.ShowPanel();
			}
			if (targetGO == null)
			{
				if (this.currentRootTarget == null && this._currentTutorialData.targetRootName != null)
				{
					this.currentRootTarget = GameObject.Find(this._currentTutorialData.targetRootName);
					if (this.currentRootTarget == null)
					{
						this.currentRootTarget = GameObject.Find(this._currentTutorialData.targetRootName + "(Clone)");
					}
				}
				if (this.currentChildTarget == null && this.currentRootTarget != null && this._currentTutorialData.targetPath != null)
				{
					this.currentChildTarget = this.currentRootTarget.transform.Find(this._currentTutorialData.targetPath);
				}
			}
			else
			{
				this.currentChildTarget = targetGO.transform;
			}
			Camera currentCamera = UICamera.currentCamera;
			this.tutorialGuyPanel.gameObject.SetActive(this._currentTutorialData.dialogType == TutorialData.DialogTypes.TutorialGuy);
			this.InformativePanel.gameObject.SetActive(this._currentTutorialData.dialogType == TutorialData.DialogTypes.Informative);
			TutorialData.DialogTypes dialogType = this._currentTutorialData.dialogType;
			if (dialogType != TutorialData.DialogTypes.TutorialGuy)
			{
				if (dialogType != TutorialData.DialogTypes.Informative)
				{
					if (dialogType != TutorialData.DialogTypes.Objective)
					{
					}
				}
				else
				{
					this._currentWindow = this.InformativePanel;
					this.SetupInformativeTutorialDialog();
				}
			}
			else
			{
				this._currentWindow = this.tutorialGuyPanel;
				this.tutorialGuyLabel.text = Language.Get(this._currentTutorialData.Descriptions[0].Draft, TranslationContext.Tutorial);
				this.tutorialGuyOkButton.gameObject.SetActive(this._currentTutorialData.tutorialGuyLifetime <= 0f);
				this.tutorialGuyOkButton.isEnabled = true;
			}
			for (int i = 0; i < this._uiTablesToRefresh.Length; i++)
			{
				UITable uitable = this._uiTablesToRefresh[i];
				uitable.Reposition();
			}
			if (this._currentWindow != null)
			{
				this._currentWindow.SetWindowVisibility(true);
				if (this._currentTutorialData.dialogType == TutorialData.DialogTypes.TutorialGuy || this._currentTutorialData.dialogType == TutorialData.DialogTypes.Informative)
				{
					this._uiNavigationGroupHolder.AddGroup();
				}
				if (!this._currentTutorialData.IsObjective())
				{
					this.ShowOvelay(this.CurrentAnimationDuration(true));
					if (this.TutorialSnapshot != null && this._tutorialSnapshotToken == null)
					{
						this._tutorialSnapshotToken = FMODAudioManager.PlayAt(this.TutorialSnapshot, null);
					}
				}
			}
			this._currentOnDialogClosedDelegate = onClosed;
			if (onClosed != null)
			{
				this._currentOnDialogClosedDelegate.oneShot = true;
			}
			this._showDelay = this._currentTutorialData.ShowDelay;
			if (this.currentChildTarget != null)
			{
				Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(this.currentChildTarget);
				this.CurrentTutorialArea = new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
			}
			if (this._currentTutorialData.dialogType == TutorialData.DialogTypes.TutorialGuy || this._currentTutorialData.dialogType == TutorialData.DialogTypes.Informative)
			{
				GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.OptionsCursor);
			}
			this.UnderTutorial = true;
			TutorialUIController.Log.Debug("XXX SetupTutorialDialog");
		}

		private void SetupInformativeTutorialDialog()
		{
			this.InformativeOkButton.isEnabled = true;
			this.InformativePanelTitle.text = Language.Get(this._currentTutorialData.Title, TranslationContext.Tutorial);
			this.InformativeTexture.sprite2D = this._currentTutorialData.InformativeSprite;
			TutorialDataDescription[] descriptions = this._currentTutorialData.Descriptions;
			int i = 0;
			while (i < descriptions.Length && i < this.InformativePanelDescriptions.Length)
			{
				TutorialDataDescription tutorialDataDescription = descriptions[i];
				this.InformativePanelDescriptions[i].DescriptionLabel.text = Language.Get(tutorialDataDescription.Draft, tutorialDataDescription.Sheet);
				ISprite sprite = null;
				string empty = string.Empty;
				if (tutorialDataDescription.ControllerInputAction != -1)
				{
					if (this._inputGetActiveDevicePoller.GetActiveDevice() == 3 && tutorialDataDescription.ControllerInputAction == 4)
					{
						this._inputTranslation.TryToGetInputJoystickAssetOrFallbackToTranslation(22, ref sprite, ref empty);
					}
					else
					{
						this._inputTranslation.TryToGetInputActionActiveDeviceAssetOrFallbackToTranslation(tutorialDataDescription.ControllerInputAction, ref sprite, ref empty);
					}
				}
				Sprite sprite2 = (sprite == null) ? null : (sprite as UnitySprite).GetSprite();
				this.InformativePanelDescriptions[i].InputSprite.sprite2D = sprite2;
				this.InformativePanelDescriptions[i].InputSprite.gameObject.SetActive(sprite2);
				this.InformativePanelDescriptions[i].InputLabelGroupGameObject.gameObject.SetActive(!sprite2 && !string.IsNullOrEmpty(empty));
				this.InformativePanelDescriptions[i].InputLabel.text = empty;
				i++;
			}
			while (i < this.InformativePanelDescriptions.Length)
			{
				this.InformativePanelDescriptions[i].DescriptionLabel.text = string.Empty;
				this.InformativePanelDescriptions[i].InputSprite.gameObject.SetActive(false);
				this.InformativePanelDescriptions[i].InputLabelGroupGameObject.gameObject.SetActive(false);
				i++;
			}
			this.InformativePanelDescWarn.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(this._currentTutorialData.DescWarning));
			if (this.InformativePanelDescWarn.transform.parent.gameObject.activeSelf)
			{
				this.InformativePanelDescWarn.text = Language.Get(this._currentTutorialData.DescWarning, TranslationContext.Tutorial);
			}
		}

		private void ResetDelayedDialog()
		{
			this._delayedOnButtonEvent = null;
			this._delayedTargetGO = null;
			this._showDelay = 0f;
		}

		public void UpdateDescriptionParameter(params object[] parameter)
		{
			this.parameterObjectiveLabelParameter = parameter;
			if (this._currentTutorialData != null)
			{
				TutorialData.DialogTypes dialogType = this._currentTutorialData.dialogType;
				if (dialogType == TutorialData.DialogTypes.Objective)
				{
					this.ObjectiveCounterLabel.text = string.Format(this._objectiveParameterFormat, parameter);
					if (!this.ObjectiveCounterLabel.gameObject.activeSelf)
					{
						this.ObjectiveCounterLabel.gameObject.SetActive(true);
					}
				}
			}
		}

		private void Update()
		{
			if (this._showDelay > 0f)
			{
				this._showDelay -= Time.deltaTime;
				if (this._showDelay <= 0f)
				{
					this.InnerSetupTutorialDialog(this._delayedOnButtonEvent, this._delayedTargetGO);
					this.ResetDelayedDialog();
				}
			}
			if (!Application.isPlaying || this._currentTutorialData == null)
			{
				return;
			}
			if (this._currentTutorialData.dialogType == TutorialData.DialogTypes.TutorialGuy && this._currentTutorialData.tutorialGuyLifetime > 0f)
			{
				this._currentTutorialData.tutorialGuyLifetime -= Time.deltaTime;
				if (this._currentTutorialData.tutorialGuyLifetime <= 0f)
				{
					this.OnOkButtonClick();
				}
			}
			if (this.currentChildTarget == null)
			{
				return;
			}
			if (this.currentChildTarget)
			{
				Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(this.currentChildTarget);
				this.CurrentTutorialArea = new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
			}
			this._mousePos = Input.mousePosition;
			this._mousePos.z = 0f;
			this._mousePos = UICamera.currentCamera.ScreenToWorldPoint(this._mousePos);
			TutorialUIController.screenCollider.enabled = (this.IsInTutorialTransition || !this.CurrentTutorialArea.Contains(this._mousePos));
		}

		public void UpdateButton(TutorialButtonKind buttonKind, float barSize)
		{
			switch (buttonKind)
			{
			case TutorialButtonKind.A:
				this.UpdateButtonObject(this.ObjectivePanelButtonAFeedback, barSize);
				break;
			case TutorialButtonKind.D:
				this.UpdateButtonObject(this.ObjectivePanelButtonDFeedback, barSize);
				break;
			case TutorialButtonKind.W:
				this.UpdateButtonObject(this.ObjectivePanelButtonWFeedback, barSize);
				break;
			case TutorialButtonKind.Drift:
				this.UpdateButtonObject(this.ObjectivePanelButtonDriftFeedback, barSize);
				break;
			}
		}

		private void UpdateButtonObject(TutorialButtonFeedbackReference buttonOjbectReference, float size)
		{
			buttonOjbectReference.ProgressBar.value = size;
			if ((int)size < 1)
			{
				return;
			}
			this.FeedbackButtonsRunningFade.Add(buttonOjbectReference);
			EventDelegate onGlowDeactivationFinish = new EventDelegate(delegate()
			{
				this.FeedbackButtonsRunningFade.Remove(buttonOjbectReference);
			});
			EventDelegate item = new EventDelegate(delegate()
			{
				TweenAlpha tweenAlpha2 = TweenAlpha.Begin(buttonOjbectReference.Glow.gameObject, this.GlowFadeOutAnimationTime, 0f);
				tweenAlpha2.onFinished.Add(onGlowDeactivationFinish);
			});
			TweenColor.Begin(buttonOjbectReference.ProgressBar.gameObject, this.ProgressBarAnimationTime, this.GlowColor);
			TweenAlpha tweenAlpha = TweenAlpha.Begin(buttonOjbectReference.Glow.gameObject, this.GlowFadeInAnimationTime, 0.8f);
			tweenAlpha.onFinished.Add(item);
		}

		private void OnDrawGizmos()
		{
			if (this.currentChildTarget == null)
			{
				return;
			}
			Gizmos.DrawCube(this.CurrentTutorialArea.center, new Vector3(this.CurrentTutorialArea.width, this.CurrentTutorialArea.height, 1f));
		}

		public void PassTutorial(string tutorialStepName)
		{
			Dictionary<string, TutorialData> tutorialDatasDictionary = GameHubBehaviour.Hub.TutorialHub.TutorialControllerInstance.tutorialDatasDictionary;
			if (!tutorialDatasDictionary.ContainsKey(tutorialStepName))
			{
				return;
			}
			this.IsInTutorialTransition = true;
			if (this._currentTutorialData != null && this._currentTutorialData.Name == tutorialStepName)
			{
				this.CloseTutorialDialog(null);
			}
			this.currentRootTarget = null;
			this.currentChildTarget = null;
		}

		public void DisposeTutorialDialog()
		{
			this.ResetDelayedDialog();
			if (this._currentWindow != null && this._currentTutorialData != null)
			{
				this._currentWindow.SetWindowVisibility(false);
				this._uiNavigationGroupHolder.RemoveGroup();
				this._currentWindow.gameObject.SetActive(false);
				if (this._currentTutorialData.dialogType == TutorialData.DialogTypes.TutorialGuy || this._currentTutorialData.dialogType == TutorialData.DialogTypes.Informative)
				{
					GameHubBehaviour.Hub.CursorManager.Pop();
				}
				this._currentTutorialData = null;
				this.UnderTutorial = false;
			}
		}

		public void CloseTutorialDialog(Action whenDone)
		{
			this.ResetDelayedDialog();
			base.StartCoroutine(this.InnerCloseTutorialDialog(whenDone));
		}

		private IEnumerator InnerCloseTutorialDialog(Action whenDone)
		{
			if (this._currentWindow == null || this._currentTutorialData == null)
			{
				yield break;
			}
			TutorialUIController.Log.DebugFormat("XXX CloseTutorialDialog {0}", new object[]
			{
				this._currentWindow.name
			});
			this._currentWindow.SetWindowVisibility(false);
			this._uiNavigationGroupHolder.RemoveGroup();
			if (this._currentTutorialData.dialogType == TutorialData.DialogTypes.TutorialGuy || this._currentTutorialData.dialogType == TutorialData.DialogTypes.Informative)
			{
				GameHubBehaviour.Hub.CursorManager.Pop();
			}
			if (!this._currentTutorialData.IsObjective())
			{
				if (this._currentTutorialData.HasNextWindow)
				{
					yield return new WaitForSeconds(this.CurrentAnimationDuration(false));
				}
				else
				{
					this.HideOvelay(this.CurrentAnimationDuration(false));
					yield return new WaitForSeconds(this.CurrentAnimationDuration(false));
					this.HidePanel();
					if (this._tutorialSnapshotToken != null)
					{
						this._tutorialSnapshotToken.KeyOff();
						this._tutorialSnapshotToken = null;
					}
				}
			}
			this._currentTutorialData = null;
			this.UnderTutorial = false;
			if (whenDone != null)
			{
				whenDone();
			}
			yield break;
		}

		public void ShowPanel()
		{
			TweenAlpha.Begin(base.gameObject, 0.1f, 1f);
		}

		public void HidePanel()
		{
			TweenAlpha.Begin(base.gameObject, 0.1f, 0f);
		}

		public float OriginalAlphaOverlay
		{
			get
			{
				if (this._originalAlphaOverlay < 0f)
				{
					this.SetupOverlay();
				}
				return this._originalAlphaOverlay;
			}
		}

		private void SetupOverlay()
		{
			this._originalAlphaOverlay = this.Overlay.alpha;
			this.Overlay.alpha = 0f;
			this.Overlay.gameObject.SetActive(false);
		}

		[Obsolete]
		public void ShowOvelay(float duration)
		{
		}

		[Obsolete]
		public void HideOvelay(float duration)
		{
		}

		private float CurrentAnimationDuration(bool show)
		{
			if (this._currentTutorialData == null)
			{
				return -1f;
			}
			return (!show) ? 0.5f : 1f;
		}

		public bool TutorialIsRunning()
		{
			return this.UnderTutorial;
		}

		public void OnOkButtonClick()
		{
			if (this.CurrentTutorialTrigger != null)
			{
				if (this._currentOnDialogClosedDelegate != null)
				{
					this._currentOnDialogClosedDelegate.Execute();
				}
			}
			else if (this._currentTutorialData != null)
			{
				if (this._currentWindow != null && !this._currentWindow.IsWindowVisible())
				{
					return;
				}
				this.UpdateButtonState(false);
				this.PassTutorial(this._currentTutorialData.Name);
				if (this.CurrentOkButtonClickedDelegate != null)
				{
					this.CurrentOkButtonClickedDelegate.Execute();
					this.CurrentOkButtonClickedDelegate = null;
				}
				this.CloseTutorialDialog(delegate
				{
					if (this._currentOnDialogClosedDelegate != null)
					{
						this._currentOnDialogClosedDelegate.Execute();
					}
				});
			}
		}

		private void UpdateButtonState(bool pEnabled)
		{
			if (this._currentTutorialData != null)
			{
				TutorialData.DialogTypes dialogType = this._currentTutorialData.dialogType;
				if (dialogType != TutorialData.DialogTypes.Informative)
				{
					if (dialogType == TutorialData.DialogTypes.TutorialGuy)
					{
						this.tutorialGuyOkButton.isEnabled = pEnabled;
					}
				}
				else
				{
					this.InformativeOkButton.isEnabled = pEnabled;
				}
			}
		}

		public IEnumerator EnableGlowFeedback(string glowFeedbackName, Vector3 position, float delay = 0f, float duration = 0.5f)
		{
			Debug.Assert(!string.IsNullOrEmpty(glowFeedbackName), "Can't enable glow feedback with empty name!", Debug.TargetTeam.All);
			IEnumerator enumerator = this.GlowUiPanel.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					if (transform.name == glowFeedbackName)
					{
						this._currentGlowFeedbackObj = transform.gameObject;
						this._currentGlowFeedbackObj.transform.position = position;
						this._currentGlowFeedbackObj.SetActive(true);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			Tweener tweener = HOTween.To(this.GlowUiPanel, duration, new TweenParms().Prop("alpha", 1).Delay(delay));
			return tweener.WaitForCompletion();
		}

		public void UpdateGlowFeedback(Vector3 newPos)
		{
			if (this._currentGlowFeedbackObj != null)
			{
				this._currentGlowFeedbackObj.transform.position = newPos;
			}
			else
			{
				TutorialUIController.Log.WarnFormat("Someone is trying to update GlowFeedback position, but GlowFeedback is not beeing used right now...", new object[0]);
			}
		}

		public void DisableGlowFeedback(float duration = 0.5f)
		{
			if (this._currentGlowFeedbackObj != null)
			{
				HOTween.To(this.GlowUiPanel, duration, new TweenParms().Prop("alpha", 0).OnComplete(delegate()
				{
					this._currentGlowFeedbackObj.SetActive(false);
				}));
			}
		}

		private void TryToInstallControlListeners()
		{
			bool flag = GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Match != null && GameHubBehaviour.Hub.Match.LevelIsTutorial();
			this.TheLegendOfTheDisabledGameObject.SetActive(flag);
			if (flag)
			{
				if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Options != null && GameHubBehaviour.Hub.Options.Controls != null)
				{
					if (GameHubBehaviour.Hub.State.Current.StateKind == GameState.GameStateKind.Game)
					{
						this.InstallInputObservers();
					}
					else
					{
						this.UninstallInputObservers();
					}
				}
			}
			else
			{
				this.HidePanel();
			}
		}

		private void InstallInputObservers()
		{
			this._inputBindNotifierDisposable = ObservableExtensions.Subscribe<int>(Observable.Do<int>(this._inputBindNotifier.ObserveBind(), delegate(int actionId)
			{
				this.TryToRefreshUiInputText();
			}));
			this._inputBindResetDefaultNotifierDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._inputBindNotifier.ObserveResetDefault(), delegate(Unit _)
			{
				this.TryToRefreshUiInputText();
			}));
			this._activeDeviceChangeDisposable = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(this._inputActiveDeviceChangeNotifier.ObserveActiveDeviceChange(), delegate(InputDevice _)
			{
				this.TryToRefreshUiInputText();
			}));
		}

		private void UninstallInputObservers()
		{
			this.TryToDispose(this._inputBindNotifierDisposable);
			this.TryToDispose(this._inputBindResetDefaultNotifierDisposable);
			this.TryToDispose(this._activeDeviceChangeDisposable);
		}

		private void TryToDispose(IDisposable disposable)
		{
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

		private void TryToRefreshUiInputText()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Match != null && GameHubBehaviour.Hub.Match.LevelIsTutorial() && this._currentTutorialData != null && this._currentTutorialData.dialogType == TutorialData.DialogTypes.Informative)
			{
				this.SetupInformativeTutorialDialog();
			}
		}

		public void OnDisable()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			this.BottomPanelComponent.Unload();
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Options != null && GameHubBehaviour.Hub.Options.Controls != null)
			{
				this.UninstallInputObservers();
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(TutorialUIController));

		private static TutorialUIController _instance;

		private TutorialTrigger CurrentTutorialTrigger;

		public bool UnderTutorial;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[ReadOnly]
		public int NextTutorialDataId;

		public TutorialUIContentData tutorialContentData;

		public InterfacePing InterfacePing;

		public GameObject TheLegendOfTheDisabledGameObject;

		public UI2DSprite Overlay;

		public HudWindow ObjectivePanel;

		public TutorialBottomComponent BottomPanelComponent;

		public UILabel ObjectiveTitle;

		public UILabel ObjectiveDescMouse;

		public UILabel ObjectiveDescJoy;

		public UILabel ObjectiveCounterLabel;

		public UI2DSprite ObjectiveMouseIcon;

		public UI2DSprite ObjectiveJoystickIcon;

		[SerializeField]
		private string _objectiveParameterFormat = "[{0}/{1}]";

		public object[] parameterObjectiveLabelParameter;

		public HudWindow tutorialGuyPanel;

		public UILabel tutorialGuyLabel;

		public UIButton tutorialGuyOkButton;

		public HudWindow InformativePanel;

		public UIButton InformativeOkButton;

		public UILabel InformativePanelTitle;

		public UI2DSprite InformativeTexture;

		[SerializeField]
		private TutorialUIController.DescriptionGuiComponents[] InformativePanelDescriptions;

		public UILabel InformativePanelDescWarn;

		private UITable[] _uiTablesToRefresh;

		public UIPanel GlowUiPanel;

		public AudioEventAsset TutorialSnapshot;

		private FMODAudioManager.FMODAudio _tutorialSnapshotToken;

		private float _showDelay;

		private const float FadeTime = 0.1f;

		private bool _isInTutorialTransition;

		private Vector2 tipTextSize;

		private bool _tutorialPaused;

		private Vector3 _mousePos;

		private Vector3 lastTutorialGuyPosition;

		private Vector3 nextTutorialGuyPosition;

		[TutorialDataReference(false)]
		public string LastTutorialStep;

		private Rect CurrentTutorialArea;

		private Transform currentChildTarget;

		private GameObject currentRootTarget;

		private TutorialData _currentTutorialData;

		private HudWindow _currentWindow;

		public EventDelegate CurrentOkButtonClickedDelegate;

		private EventDelegate _currentOnDialogClosedDelegate;

		private bool _isInitialized;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[InjectOnClient]
		private IInputBindNotifier _inputBindNotifier;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		private IDisposable _inputBindNotifierDisposable;

		private IDisposable _inputBindResetDefaultNotifierDisposable;

		private IDisposable _activeDeviceChangeDisposable;

		private EventDelegate _delayedOnButtonEvent;

		private GameObject _delayedTargetGO;

		[Header("Old Button Fill Feedback")]
		public TutorialButtonFeedbackReference ObjectivePanelButtonWFeedback;

		public TutorialButtonFeedbackReference ObjectivePanelButtonAFeedback;

		public TutorialButtonFeedbackReference ObjectivePanelButtonDFeedback;

		public TutorialButtonFeedbackReference ObjectivePanelButtonDriftFeedback;

		public Color GlowColor = new Color(0.3f, 0.67f, 0.21f);

		public float ProgressBarAnimationTime = 0.5f;

		public float GlowFadeInAnimationTime = 1f;

		public float GlowFadeOutAnimationTime = 1f;

		private List<TutorialButtonFeedbackReference> FeedbackButtonsRunningFade = new List<TutorialButtonFeedbackReference>();

		private float _originalAlphaOverlay = -1f;

		private GameObject _currentGlowFeedbackObj;

		[Serializable]
		private struct DescriptionGuiComponents
		{
			public UILabel DescriptionLabel;

			public UI2DSprite InputSprite;

			public GameObject InputLabelGroupGameObject;

			public UILabel InputLabel;
		}
	}
}
