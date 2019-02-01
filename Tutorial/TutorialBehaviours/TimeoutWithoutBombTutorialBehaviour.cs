using System;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Tutorial.InGame;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.TutorialBehaviours
{
	public class TimeoutWithoutBombTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this._active = true;
			this._timeoutChronometer = new TimeUtils.Chronometer(new Func<int>(GameHubBehaviour.Hub.GameTime.GetPlaybackTime));
			this._timeoutChronometer.Start();
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.BombManagerOnListenToBombDelivery;
		}

		protected override void Destroy()
		{
			base.Destroy();
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.BombManagerOnListenToBombDelivery;
		}

		private void BombManagerOnListenToBombDelivery(int causerId, TeamKind scoredTeam, Vector3 deliveryPosition)
		{
			this._active = false;
		}

		protected override void UpdateOnServer()
		{
			base.UpdateOnServer();
			if (!this._active || GameHubBehaviour.Hub.BombManager.IsCarryingBomb(base.playerController.Combat.Id.ObjId))
			{
				this._timeoutChronometer.Reset();
				this._timeoutChronometer.Start();
			}
			else if (this._timeoutChronometer.GetTime() > this._timeoutMillis)
			{
				TutorialStepsController componentInParent = base.GetComponentInParent<TutorialStepsController>();
				componentInParent.ForceStep(this._tutorialStepToGo);
			}
		}

		private TimeUtils.Chronometer _timeoutChronometer;

		[SerializeField]
		private int _timeoutMillis;

		[SerializeField]
		private int _tutorialStepToGo;

		private bool _active;
	}
}
