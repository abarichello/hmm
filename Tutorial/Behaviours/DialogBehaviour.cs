using System;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class DialogBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			this.ShowDialog(new EventDelegate.Callback(this.CompleteBehaviourAndSync));
		}

		protected override void OnStepCompletedOnClient()
		{
			base.OnStepCompletedOnClient();
			TutorialUIController.Instance.CloseTutorialDialog(null);
		}

		public void ShowDialogForBehaviour()
		{
			base.behaviourCompleted = false;
			this.ShowDialog(new EventDelegate.Callback(this.DialogNextButtonPressedForCamera));
		}

		private void DialogNextButtonPressedForCamera()
		{
			base.behaviourCompleted = true;
		}

		private void ShowDialog(EventDelegate.Callback callback)
		{
			TutorialUIController.Instance.ShowDialog(this.TutorialData, new EventDelegate(callback));
		}

		public override void ResetBehaviour()
		{
			base.ResetBehaviour();
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				TutorialUIController.Instance.CloseTutorialDialog(null);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(DialogBehaviour));

		public bool CameraStopFollowingTarget;

		public TutorialData TutorialData;
	}
}
