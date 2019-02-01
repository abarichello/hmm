using System;
using System.Collections;
using System.Collections.Generic;
using FMod;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Tutorial.Behaviours;
using HeavyMetalMachines.Tutorial.UnityUI;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Holoville.HOTween;
using Pocketverse;
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
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			TutorialUIController._instance = this;
			base.transform.parent = GameHubBehaviour.Hub.State.CurrentSceneStateData.StateGuiController.transform;
			base.transform.localScale = Vector3.one;
			base.transform.localPosition = Vector3.zero;
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
			Vector3 size = new Vector3((float)Screen.width, (float)Screen.height, 1f);
			TutorialUIController.screenCollider = base.gameObject.AddComponent<BoxCollider>();
			TutorialUIController.screenCollider.size = size;
			this.arrowPanel.SetActive(true);
			this.tooltipAnchor.gameObject.SetActive(true);
			this.tutorialGuyPanel.SetWindowVisibility(false);
			this.ObjectivePanel.SetWindowVisibility(false);
			this.BottomPanelComponent.Load();
			this.HintPanelComponent.Load();
			this.ObjectiveRightPanel.SetWindowVisibility(false);
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
			TweenAlpha.Begin(this.arrowPanel, 0f, 0f);
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
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(ClientBITags.TutorialStart, string.Format("Tutorial={0}", this._currentTutorialData.Name), false);
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
			this.arrowPanel.SetActive(this._currentTutorialData.EnableArrowPanel);
			if (this._currentTutorialData.EnableArrowPanel)
			{
				this.tooltipLabel.text = Language.Get(this._currentTutorialData.Tip, "Tutorial");
			}
			Camera currentCamera = UICamera.currentCamera;
			this.tutorialGuyPanel.gameObject.SetActive(this._currentTutorialData.dialogType == TutorialData.DialogTypes.TutorialGuy);
			this.InformativePanel.gameObject.SetActive(this._currentTutorialData.dialogType == TutorialData.DialogTypes.Informative);
			this.ObjectiveRightPanel.gameObject.SetActive(this._currentTutorialData.dialogType == TutorialData.DialogTypes.ObjectiveRight);
			string text = (this._currentTutorialData.ControlAction1 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction1, ControlOptions.ControlActionInputType.Primary);
			string text2 = (this._currentTutorialData.ControlAction2 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction2, ControlOptions.ControlActionInputType.Primary);
			string text3 = (this._currentTutorialData.ControlAction3 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction3, ControlOptions.ControlActionInputType.Primary);
			string text4 = (this._currentTutorialData.ControlAction4 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction4, ControlOptions.ControlActionInputType.Primary);
			string text5 = (!string.IsNullOrEmpty(this._currentTutorialData.DirectControlJoyAction1) || this._currentTutorialData.ControlAction1 == ControlAction.None) ? this._currentTutorialData.DirectControlJoyAction1 : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction1, ControlOptions.ControlActionInputType.Secondary);
			string text6 = (!string.IsNullOrEmpty(this._currentTutorialData.DirectControlJoyAction2) || this._currentTutorialData.ControlAction2 == ControlAction.None) ? this._currentTutorialData.DirectControlJoyAction2 : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction2, ControlOptions.ControlActionInputType.Secondary);
			string text7 = (!string.IsNullOrEmpty(this._currentTutorialData.DirectControlJoyAction3) || this._currentTutorialData.ControlAction3 == ControlAction.None) ? this._currentTutorialData.DirectControlJoyAction3 : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction3, ControlOptions.ControlActionInputType.Secondary);
			string text8 = (!string.IsNullOrEmpty(this._currentTutorialData.DirectControlJoyAction4) || this._currentTutorialData.ControlAction4 == ControlAction.None) ? this._currentTutorialData.DirectControlJoyAction4 : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction4, ControlOptions.ControlActionInputType.Secondary);
			switch (this._currentTutorialData.dialogType)
			{
			case TutorialData.DialogTypes.TutorialGuy:
				this._currentWindow = this.tutorialGuyPanel;
				this.tutorialGuyLabel.text = string.Format(Language.Get(this._currentTutorialData.DescMouse, "Tutorial"), new object[]
				{
					text,
					text2,
					text3,
					text4
				});
				this.tutorialGuyOkButton.gameObject.SetActive(this._currentTutorialData.tutorialGuyLifetime <= 0f);
				this.tutorialGuyOkButton.SetEnabledAndSelected();
				break;
			case TutorialData.DialogTypes.Informative:
				this._currentWindow = this.InformativePanel;
				this.InformativeOkButton.SetEnabledAndSelected();
				this.InformativePanelTitle.text = Language.Get(this._currentTutorialData.Title, "Tutorial");
				this.InformativeTexture.sprite2D = this._currentTutorialData.InformativeSprite;
				this.InformativePanelDescMouse.text = string.Format(Language.Get(this._currentTutorialData.DescMouse, "Tutorial"), new object[]
				{
					text,
					text2,
					text3,
					text4
				});
				this.InformativePanelDescJoy.text = string.Format(Language.Get(this._currentTutorialData.DescJoystick, "Tutorial"), new object[]
				{
					text5,
					text6,
					text7,
					text8
				});
				this.InformativePanelDescWarn.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(this._currentTutorialData.DescWarning));
				if (this.InformativePanelDescWarn.transform.parent.gameObject.activeSelf)
				{
					this.InformativePanelDescWarn.text = string.Format(Language.Get(this._currentTutorialData.DescWarning, "Tutorial"), new object[]
					{
						text,
						text2,
						text3,
						text4
					});
				}
				this.InformativeMouseIcon.gameObject.SetActive(this._currentTutorialData.MouseAndJoystickIcons);
				this.InformativeJoystickIcon.gameObject.SetActive(this._currentTutorialData.MouseAndJoystickIcons);
				break;
			case TutorialData.DialogTypes.ObjectiveRight:
				this._currentWindow = this.ObjectiveRightPanel;
				this.ObjectiveRightTitle.text = Language.Get(this._currentTutorialData.Title, "Tutorial");
				this.ObjectiveRightDescMouse.text = string.Format(Language.Get(this._currentTutorialData.DescMouse, "Tutorial"), new object[]
				{
					text,
					text2,
					text3,
					text4
				});
				this.ObjectiveRightDescJoy.text = string.Format(Language.Get(this._currentTutorialData.DescJoystick, "Tutorial"), new object[]
				{
					text5,
					text6,
					text7,
					text8
				});
				this.parameterObjectiveLabelParameter = null;
				this.ObjectiveRightCounterLabel.gameObject.SetActive(false);
				this.ObjectiveRightMouseIcon.gameObject.SetActive(this._currentTutorialData.MouseAndJoystickIcons);
				this.ObjectiveRightJoystickIcon.gameObject.SetActive(this._currentTutorialData.MouseAndJoystickIcons);
				break;
			}
			for (int i = 0; i < this._uiTablesToRefresh.Length; i++)
			{
				UITable uitable = this._uiTablesToRefresh[i];
				uitable.Reposition();
			}
			if (this._currentWindow != null)
			{
				this._currentWindow.SetWindowVisibility(true);
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
			TweenAlpha.Begin(this.arrowPanel, 0f, 0f);
			if (this.currentChildTarget != null)
			{
				Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(this.currentChildTarget);
				this.CurrentTutorialArea = new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
				this.tooltipAnchor.container = this.currentChildTarget.gameObject;
				if (this.currentChildTarget.position.y > 0f)
				{
					this.tooltipAnchor.pixelOffset = new Vector2(this.tooltipAnchor.pixelOffset.x, -35f);
					this.tooltipAnchor.side = UIAnchor.Side.Bottom;
					if (this.arrow.transform.localPosition.y < 0f)
					{
						Vector3 localPosition = this.arrow.transform.localPosition;
						localPosition.y *= -1f;
						this.arrow.transform.localPosition = localPosition;
					}
					Quaternion localRotation = this.arrow.transform.localRotation;
					localRotation.z = 0f;
					this.arrow.transform.localRotation = localRotation;
				}
				else
				{
					this.tooltipAnchor.pixelOffset = new Vector2(this.tooltipAnchor.pixelOffset.x, 35f);
					this.tooltipAnchor.side = UIAnchor.Side.Top;
					if (this.arrow.transform.localPosition.y > 0f)
					{
						Vector3 localPosition2 = this.arrow.transform.localPosition;
						localPosition2.y *= -1f;
						this.arrow.transform.localPosition = localPosition2;
					}
					Quaternion localRotation2 = this.arrow.transform.localRotation;
					localRotation2.z = 180f;
					this.arrow.transform.localRotation = localRotation2;
				}
			}
			if (this._currentTutorialData.dialogType == TutorialData.DialogTypes.TutorialGuy || this._currentTutorialData.dialogType == TutorialData.DialogTypes.Informative)
			{
				GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.OptionsCursor);
			}
			this.UnderTutorial = true;
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
				if (dialogType != TutorialData.DialogTypes.Objective)
				{
					if (dialogType == TutorialData.DialogTypes.ObjectiveRight)
					{
						this.ObjectiveRightCounterLabel.text = string.Format(this._objectiveParameterFormat, parameter);
						if (!this.ObjectiveRightCounterLabel.gameObject.activeSelf)
						{
							this.ObjectiveRightCounterLabel.gameObject.SetActive(true);
						}
					}
				}
				else
				{
					this.ObjectiveCounterLabel.text = string.Format(this._objectiveParameterFormat, parameter);
					if (!this.ObjectiveCounterLabel.gameObject.activeSelf)
					{
						this.ObjectiveCounterLabel.gameObject.SetActive(true);
					}
				}
			}
		}

		private void RefreshUIInputText()
		{
			if (this._currentTutorialData == null)
			{
				return;
			}
			string text = (this._currentTutorialData.ControlAction1 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction1, ControlOptions.ControlActionInputType.Primary);
			string text2 = (this._currentTutorialData.ControlAction2 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction2, ControlOptions.ControlActionInputType.Primary);
			string text3 = (this._currentTutorialData.ControlAction3 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction3, ControlOptions.ControlActionInputType.Primary);
			string text4 = (this._currentTutorialData.ControlAction4 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction4, ControlOptions.ControlActionInputType.Primary);
			string text5 = (this._currentTutorialData.ControlAction1 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction1, ControlOptions.ControlActionInputType.Secondary);
			string text6 = (this._currentTutorialData.ControlAction2 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction2, ControlOptions.ControlActionInputType.Secondary);
			string text7 = (this._currentTutorialData.ControlAction3 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction3, ControlOptions.ControlActionInputType.Secondary);
			string text8 = (this._currentTutorialData.ControlAction4 == ControlAction.None) ? string.Empty : ControlOptions.GetNGUIIconOrTextLocalized(this._currentTutorialData.ControlAction4, ControlOptions.ControlActionInputType.Secondary);
			switch (this._currentTutorialData.dialogType)
			{
			case TutorialData.DialogTypes.TutorialGuy:
				this.tutorialGuyLabel.text = string.Format(Language.Get(this._currentTutorialData.DescMouse, "Tutorial"), new object[]
				{
					text,
					text2,
					text3,
					text4
				});
				break;
			case TutorialData.DialogTypes.Informative:
				this.InformativePanelTitle.text = Language.Get(this._currentTutorialData.Title, "Tutorial");
				this.InformativePanelDescMouse.text = string.Format(Language.Get(this._currentTutorialData.DescMouse, "Tutorial"), new object[]
				{
					text,
					text2,
					text3,
					text4
				});
				this.InformativePanelDescJoy.text = string.Format(Language.Get(this._currentTutorialData.DescJoystick, "Tutorial"), new object[]
				{
					text5,
					text6,
					text7,
					text8
				});
				break;
			case TutorialData.DialogTypes.Objective:
				this.ObjectiveTitle.text = Language.Get(this._currentTutorialData.Title, "Tutorial");
				this.ObjectiveDescMouse.text = string.Format(Language.Get(this._currentTutorialData.DescMouse, "Tutorial"), new object[]
				{
					text,
					text2,
					text3,
					text4
				});
				this.ObjectiveDescJoy.text = string.Format(Language.Get(this._currentTutorialData.DescJoystick, "Tutorial"), new object[]
				{
					text5,
					text6,
					text7,
					text8
				});
				break;
			case TutorialData.DialogTypes.ObjectiveRight:
				this.ObjectiveRightTitle.text = Language.Get(this._currentTutorialData.Title, "Tutorial");
				this.ObjectiveRightDescMouse.text = string.Format(Language.Get(this._currentTutorialData.DescMouse, "Tutorial"), new object[]
				{
					text,
					text2,
					text3,
					text4
				});
				this.ObjectiveRightDescJoy.text = string.Format(Language.Get(this._currentTutorialData.DescJoystick, "Tutorial"), new object[]
				{
					text5,
					text6,
					text7,
					text8
				});
				break;
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
			GameHubBehaviour.Hub.TutorialHub.TutorialControllerInstance.PassTutorial(tutorialStepName);
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
			this._currentWindow.SetWindowVisibility(false);
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
					TweenAlpha.Begin(this.arrowPanel, this.CurrentAnimationDuration(false), 0f);
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
				this.CurrentTutorialTrigger.FinishTutorial();
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
			HeavyMetalMachines.Utils.Debug.Assert(!string.IsNullOrEmpty(glowFeedbackName), "Can't enable glow feedback with empty name!", HeavyMetalMachines.Utils.Debug.TargetTeam.All);
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
						this.UninstallControlsListeners();
						this.InstallControlsListeners();
					}
					else
					{
						this.UninstallControlsListeners();
					}
				}
			}
			else
			{
				this.HidePanel();
			}
		}

		private void InstallControlsListeners()
		{
			ControlOptions controls = GameHubBehaviour.Hub.Options.Controls;
			controls.OnKeyChangedCallback += this.OnKeyChangedCallback;
			controls.OnResetDefaultCallback += this.OnResetDefaultCallback;
			controls.OnResetPrimaryDefaultCallback += this.OnResetDefaultCallback;
			controls.OnResetSecondaryDefaultCallback += this.OnResetDefaultCallback;
		}

		private void UninstallControlsListeners()
		{
			ControlOptions controls = GameHubBehaviour.Hub.Options.Controls;
			controls.OnKeyChangedCallback -= this.OnKeyChangedCallback;
			controls.OnResetDefaultCallback -= this.OnResetDefaultCallback;
			controls.OnResetPrimaryDefaultCallback -= this.OnResetDefaultCallback;
			controls.OnResetSecondaryDefaultCallback -= this.OnResetDefaultCallback;
		}

		private void OnResetDefaultCallback()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Match != null && GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this.RefreshUIInputText();
			}
		}

		private void OnKeyChangedCallback(ControlAction controlAction)
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Match != null && GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this.RefreshUIInputText();
			}
		}

		public void OnDisable()
		{
			UnityEngine.Debug.LogError("OnDisable");
			if (!Application.isPlaying)
			{
				return;
			}
			UnityEngine.Debug.LogError("OnDisable after");
			this.BottomPanelComponent.Unload();
			this.HintPanelComponent.Unload();
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Options != null && GameHubBehaviour.Hub.Options.Controls != null)
			{
				this.UninstallControlsListeners();
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(TutorialUIController));

		private static TutorialUIController _instance;

		private TutorialTrigger CurrentTutorialTrigger;

		public bool UnderTutorial;

		[ReadOnly]
		public int NextTutorialDataId;

		public TutorialUIContentData tutorialContentData;

		public InterfacePing InterfacePing;

		public GameObject TheLegendOfTheDisabledGameObject;

		public UI2DSprite Overlay;

		public GameObject arrowPanel;

		public UISprite arrow;

		public UIAnchor tooltipAnchor;

		public UILabel tooltipLabel;

		public UISprite tooltipBG;

		public HudWindow ObjectivePanel;

		public TutorialBottomComponent BottomPanelComponent;

		public TutorialHintComponent HintPanelComponent;

		public UILabel ObjectiveTitle;

		public UILabel ObjectiveDescMouse;

		public UILabel ObjectiveDescJoy;

		public UILabel ObjectiveCounterLabel;

		public UI2DSprite ObjectiveMouseIcon;

		public UI2DSprite ObjectiveJoystickIcon;

		public HudWindow ObjectiveRightPanel;

		public UILabel ObjectiveRightTitle;

		public UILabel ObjectiveRightDescMouse;

		public UILabel ObjectiveRightDescJoy;

		public UILabel ObjectiveRightCounterLabel;

		public UI2DSprite ObjectiveRightMouseIcon;

		public UI2DSprite ObjectiveRightJoystickIcon;

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

		public UILabel InformativePanelDescMouse;

		public UILabel InformativePanelDescJoy;

		public UILabel InformativePanelDescWarn;

		public UI2DSprite InformativeMouseIcon;

		public UI2DSprite InformativeJoystickIcon;

		private UITable[] _uiTablesToRefresh;

		public UIPanel GlowUiPanel;

		public Animator MovieTextureGroup;

		public UITexture MovieTexture;

		public UITexture MovieTextureBg;

		public Animator MovieButtonsGroup;

		public UIButton MovieRewindButton;

		public UIButton MovieNextButton;

		public FMODAsset TutorialSnapshot;

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
	}
}
