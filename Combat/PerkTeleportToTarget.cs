using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(Rigidbody))]
	public class PerkTeleportToTarget : BasePerk, PerkAttachToObject.IEffectAttachListener
	{
		protected override void Awake()
		{
			base.Awake();
			this._trans = base.transform;
		}

		public override void PerkInitialized()
		{
			this._duration = this.height / this.Effect.Data.MoveSpeed;
			if (!GameHubBehaviour.Hub.Net.IsServer() && this.Effect.Data.FirstPackageSent)
			{
				this.UpdateAfterVisibilityChange();
			}
			else
			{
				int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
				this._animationStartTime = (long)playbackTime;
				this._startTime = playbackTime;
			}
			this.isSleeping = false;
		}

		private void UpdateAfterVisibilityChange()
		{
			this._animationStartTime = (long)(this._startTime = this.Effect.Data.EventTime);
			this.UpdatePosition();
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
			if (this.isSleeping)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this.UpdatePosition())
			{
				return;
			}
			this.Effect.TriggerDestroy(-1, this._trans.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
			this.isSleeping = true;
		}

		private bool UpdatePosition()
		{
			EffectEvent data = this.Effect.Data;
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			float num = (float)(playbackTime - this._startTime) / 1000f;
			if (num > 0f && num <= 2f)
			{
				this._animationStartTime = (long)playbackTime;
			}
			else if (num > 2f && num <= 4f)
			{
				this.t = (float)((long)playbackTime - this._animationStartTime) / (this._duration * 1000f);
				this._trans.position = Vector3.Lerp(data.Origin, data.Origin + Vector3.up * 10f, this.t);
			}
			else if (num > 4f && num <= 6f)
			{
				this._trans.position = new Vector3(data.Target.x, data.Origin.y + 10f, data.Target.z);
				this._animationStartTime = (long)playbackTime;
			}
			else
			{
				if (num <= 6f || num > 8f)
				{
					return false;
				}
				this.t = (float)((long)playbackTime - this._animationStartTime) / (this._duration * 1000f);
				this._trans.position = Vector3.Lerp(new Vector3(data.Target.x, data.Origin.y + 10f, data.Target.z), new Vector3(data.Target.x, data.Origin.y, data.Target.z), this.t);
			}
			return true;
		}

		public void OnAttachEffect(PerkAttachToObject.EffectAttachToTarget msg)
		{
			this._trans = msg.Target;
		}

		private new Transform _trans;

		private float _duration;

		private float t;

		public float height = 10f;

		public long _animationStartTime;

		private int _startTime;

		private bool isSleeping;
	}
}
