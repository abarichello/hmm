using System;
using HeavyMetalMachines.Combat;
using Hoplon.Timeline;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	public abstract class TimelineStream<T> : MovementStream where T : struct
	{
		protected abstract void SetCurrentPose(ref T pose);

		protected abstract void ResetCurrentPose(ref T pose);

		protected abstract Timeline<T> InstantiateTimeline();

		protected override void Awake()
		{
			base.Awake();
			bool flag = GameHubBehaviour.Hub.Net.IsClient();
			if (flag)
			{
				this.Timeline = this.InstantiateTimeline();
				this.ScoreBoard = GameHubBehaviour.Hub.BombManager.ScoreBoard;
			}
			base.enabled = flag;
			this.smoothingFactor = this.configuration.SmoothingFactor;
		}

		public void OnEnable()
		{
			this.Timeline.Clear();
			this.ResetCurrentPose(ref this._currentPose);
		}

		protected virtual void Update()
		{
			if (this.Timeline.TryGetCurrentPose(ref this._idealPose))
			{
				BombScoreBoard.State currentState = this.ScoreBoard.CurrentState;
				bool flag = this.Timeline.Size > 1u && (currentState == BombScoreBoard.State.BombDelivery || currentState == BombScoreBoard.State.PreReplay);
				float t = (!flag) ? 1f : ((float)(1.0 - Math.Pow(this.smoothingFactor, (double)Time.smoothDeltaTime)));
				this.Timeline.Interpolate(ref this._currentPose, ref this._idealPose, t, out this._currentPose);
				this.SetCurrentPose(ref this._currentPose);
			}
		}

		protected Timeline<T> Timeline;

		protected BombScoreBoard ScoreBoard;

		[SerializeField]
		protected TimelineConfiguration configuration;

		private double smoothingFactor;

		private T _idealPose;

		private T _currentPose;
	}
}
