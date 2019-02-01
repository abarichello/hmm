using System;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class ActionTutorialBehaviourBase : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			if (this.ActionTime == ActionTutorialBehaviourBase.EActionTime.OnClientOkButtonClicked)
			{
				TutorialUIController.Instance.CurrentOkButtonClickedDelegate = new EventDelegate(new EventDelegate.Callback(this.ExecuteAndComplete));
				TutorialUIController.Instance.CurrentOkButtonClickedDelegate.oneShot = true;
				return;
			}
			if (this.ActionTime != ActionTutorialBehaviourBase.EActionTime.OnClientBehaviourStart && this.ActionTime != ActionTutorialBehaviourBase.EActionTime.OnBothBehaviourStart)
			{
				return;
			}
			this.ExecuteAndComplete();
		}

		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			if (this.ActionTime != ActionTutorialBehaviourBase.EActionTime.OnServerBehaviourStart && this.ActionTime != ActionTutorialBehaviourBase.EActionTime.OnBothBehaviourStart)
			{
				return;
			}
			this.ExecuteAndComplete();
		}

		protected override void OnStepCompletedOnClient()
		{
			base.OnStepCompletedOnClient();
			if (this.ActionTime != ActionTutorialBehaviourBase.EActionTime.OnClientStepCompleted && this.ActionTime != ActionTutorialBehaviourBase.EActionTime.OnBothStepCompleted)
			{
				return;
			}
			this.ExecuteAndComplete();
		}

		protected override void OnStepCompletedOnServer()
		{
			base.OnStepCompletedOnServer();
			if (this.ActionTime != ActionTutorialBehaviourBase.EActionTime.OnServerStepCompleted && this.ActionTime != ActionTutorialBehaviourBase.EActionTime.OnBothStepCompleted)
			{
				return;
			}
			this.ExecuteAndComplete();
		}

		private void ExecuteAndComplete()
		{
			this.ExecuteAction();
			this.CompleteBehaviourAndSync();
		}

		protected virtual void ExecuteAction()
		{
			ActionTutorialBehaviourBase.Log.Warn("ExecuteAction called but not implemented! Maybe you should override method ExecuteAction!!!");
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ActionTutorialBehaviourBase));

		[SerializeField]
		protected ActionTutorialBehaviourBase.EActionTime ActionTime;

		protected enum EActionTime
		{
			OnClientBehaviourStart,
			OnClientStepCompleted,
			OnServerBehaviourStart,
			OnServerStepCompleted,
			OnBothBehaviourStart,
			OnBothStepCompleted,
			OnClientOkButtonClicked
		}
	}
}
