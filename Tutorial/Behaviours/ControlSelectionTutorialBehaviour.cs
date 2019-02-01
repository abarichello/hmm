using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class ControlSelectionTutorialBehaviour : InGameTutorialBehaviourBase
	{
		private void Awake()
		{
			this.guiRoot.gameObject.SetActive(false);
		}

		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			base.SetPlayerInputsActive(false);
			this.OpenWindows();
		}

		private void AddListeners()
		{
			this.RemoveListeners();
			ControlTypeWindow controlTypeWindow = this.arcadeWindow;
			controlTypeWindow.OnTest = (Action<ControlTypeWindow>)Delegate.Combine(controlTypeWindow.OnTest, new Action<ControlTypeWindow>(this.OnTest));
			ControlTypeWindow controlTypeWindow2 = this.simulationWindow;
			controlTypeWindow2.OnTest = (Action<ControlTypeWindow>)Delegate.Combine(controlTypeWindow2.OnTest, new Action<ControlTypeWindow>(this.OnTest));
		}

		private void RemoveListeners()
		{
			ControlTypeWindow controlTypeWindow = this.arcadeWindow;
			controlTypeWindow.OnTest = (Action<ControlTypeWindow>)Delegate.Remove(controlTypeWindow.OnTest, new Action<ControlTypeWindow>(this.OnTest));
			ControlTypeWindow controlTypeWindow2 = this.simulationWindow;
			controlTypeWindow2.OnTest = (Action<ControlTypeWindow>)Delegate.Remove(controlTypeWindow2.OnTest, new Action<ControlTypeWindow>(this.OnTest));
		}

		private void SetControls(ControlTypeWindow pControlTypeWindow)
		{
			GameHubBehaviour.Hub.Options.Game.MovementModeIndex = 0;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		private void OpenWindows()
		{
			this.guiRoot.gameObject.SetActive(true);
			this.AddListeners();
		}

		private void CloseWindows()
		{
			this.guiRoot.gameObject.SetActive(false);
			this.RemoveListeners();
		}

		private void OnTest(ControlTypeWindow pControlTypeWindow)
		{
			this.SetControls(pControlTypeWindow);
			this.CloseWindows();
			this.StartCameraBehaviour();
		}

		private void StartCameraBehaviour()
		{
			base.SetPlayerInputsActive(false);
			this.waypoint.gameObject.SetActive(false);
			this.cameraFocus.StartBehaviour(new Action<InGameTutorialBehaviourBase>(this.OnCameraFocusBehaviourFinished));
		}

		private void OnCameraFocusBehaviourFinished(InGameTutorialBehaviourBase pInGameTutorialBehaviourBase)
		{
			this.cameraFocus.OnStepCompleted();
			this.StartWaypoints();
		}

		private void StartWaypoints()
		{
			base.SetPlayerInputsActive(true);
			if (this.waypoint.inputModifier != null)
			{
				this.waypoint.inputModifier.ApplyInputModifiers();
			}
			this.waypoint.path.Reset();
			this.waypoint.gameObject.SetActive(true);
			this.waypoint.StartBehaviour(new Action<InGameTutorialBehaviourBase>(this.OnWaypointBehaviourFinished));
		}

		private void OnWaypointBehaviourFinished(InGameTutorialBehaviourBase pInGameTutorialBehaviourBase)
		{
			base.SetPlayerInputsActive(false);
			this.OpenWindows();
			this.OnFinish();
		}

		private void OnFinish()
		{
			this.CloseWindows();
			Guid tConfirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = tConfirmWindowGuid,
				QuestionText = Language.Get("CONTROLS_SELECTION_CONFIRM_QUESTION", TranslationSheets.Tutorial),
				RefuseButtonText = Language.Get("CONTROLS_SELECTION_CONFIRM_QUESTION_REFUSE_BUTTON", TranslationSheets.Tutorial),
				ConfirmButtonText = Language.Get("CONTROLS_SELECTION_CONFIRM_QUESTION_ACCEPT_BUTTON", TranslationSheets.Tutorial),
				OnRefuse = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(tConfirmWindowGuid);
					this.SetPlayerInputsActive(false);
					this.OpenWindows();
				},
				OnConfirm = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(tConfirmWindowGuid);
					this.CompleteBehaviourAndSync();
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public GameObject guiRoot;

		public ControlTypeWindow arcadeWindow;

		public ControlTypeWindow simulationWindow;

		public CameraFocusTutorialBehaviour cameraFocus;

		public WaypointTutorialBehaviour waypoint;
	}
}
