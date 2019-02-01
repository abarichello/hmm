using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(Rigidbody))]
	public class PerkMoveToTargetPosition : BasePerk
	{
		public override void PerkInitialized()
		{
			EffectEvent data = this.Effect.Data;
			float num = Vector3.Distance(data.Origin, data.Target);
			this._duration = num / data.MoveSpeed * 1000f;
			if (!GameHubBehaviour.Hub.Net.IsServer() && this.Effect.Data.FirstPackageSent)
			{
				this.UpdateAfterVisibilityChange();
			}
			else
			{
				this._startTime = ((!GameHubBehaviour.Hub.Net.IsServer()) ? GameHubBehaviour.Hub.GameTime.GetPlaybackTime() : data.EventTime);
			}
			this.isSleeping = false;
		}

		private void UpdateAfterVisibilityChange()
		{
			this._startTime = this.Effect.Data.EventTime;
			this.UpdatePosition();
		}

		private float UpdatePosition()
		{
			float num = this.LerpCurve.Evaluate((float)(GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._startTime) / this._duration);
			this._trans.position = Vector3.Lerp(this.Effect.Data.Origin, this.Effect.Data.Target, num);
			return num;
		}

		private void Update()
		{
			if (this.isSleeping)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			this.UpdatePosition();
		}

		private void FixedUpdate()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this.UpdatePosition() < 1f)
			{
				return;
			}
			this.Effect.TriggerDestroy(-1, this._trans.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
			this.isSleeping = true;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkMoveToTargetPosition));

		public AnimationCurve LerpCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		});

		private new Transform _trans;

		private float _duration;

		private int _startTime;

		private bool isSleeping;
	}
}
