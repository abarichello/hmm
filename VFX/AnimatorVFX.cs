using System;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.VFX
{
	public class AnimatorVFX : BaseVFX
	{
		protected void Start()
		{
			if (this._animator == null)
			{
				this._animator = base.GetComponent<Animator>();
			}
		}

		protected void Update()
		{
			if (this._endDurationTime > 0f && Time.time > this._endDurationTime)
			{
				this.CanCollectToCache = true;
			}
		}

		protected override void OnActivate()
		{
			this._animatorLocation = ((!this._animatorFromTarget) ? this._animatorLocation : AnimatorVFX.EAnimatorLocation.EffectOwner);
			switch (this._animatorLocation)
			{
			case AnimatorVFX.EAnimatorLocation.EffectOwner:
				if (this._targetFXInfo.Owner == null)
				{
					AnimatorVFX.Log.WarnFormat("Failed to activate AnimatiorVFX [owner]. targetFXInfo:{0}", new object[]
					{
						this._targetFXInfo
					});
					this._animator = null;
				}
				else
				{
					this._animator = this._targetFXInfo.Owner.GetComponentInChildren<Animator>();
				}
				break;
			case AnimatorVFX.EAnimatorLocation.EffectTarget:
				if (this._targetFXInfo.Target == null)
				{
					AnimatorVFX.Log.WarnFormat("Failed to activate AnimatiorVFX [target]. targetFXInfo:{0}", new object[]
					{
						this._targetFXInfo
					});
					this._animator = null;
				}
				else
				{
					this._animator = this._targetFXInfo.Target.GetComponentInChildren<Animator>();
				}
				break;
			}
			if (this._animator != null)
			{
				if (!string.IsNullOrEmpty(this._triggerName))
				{
					this._animator.SetTrigger(this._triggerName);
				}
				else
				{
					this._animator.Play(0);
				}
			}
			if (this._minDuration > 0f)
			{
				this._endDurationTime = Time.time + this._minDuration;
				this.CanCollectToCache = false;
			}
		}

		protected override void WillDeactivate()
		{
			if (this.spawnStage == BaseVFX.SpawnState.OnDestroy)
			{
				this.OnActivate();
			}
		}

		protected override void OnDeactivate()
		{
		}

		private static readonly BitLogger Log = new BitLogger(typeof(AnimatorVFX));

		[FormerlySerializedAs("animator")]
		[SerializeField]
		private Animator _animator;

		[FormerlySerializedAs("animatorFromTarget")]
		[SerializeField]
		private bool _animatorFromTarget;

		[FormerlySerializedAs("AnimatorLocation")]
		[SerializeField]
		private AnimatorVFX.EAnimatorLocation _animatorLocation;

		[FormerlySerializedAs("triggerName")]
		[SerializeField]
		private string _triggerName = string.Empty;

		[FormerlySerializedAs("minDuration")]
		[SerializeField]
		private float _minDuration;

		[NonSerialized]
		private float _endDurationTime;

		public enum EAnimatorLocation
		{
			Self,
			EffectOwner,
			EffectTarget
		}
	}
}
