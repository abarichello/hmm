using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Render;
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
			if (this._animator != null && !this.checkGadgetPercentage)
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
			if (this.checkGadgetPercentage)
			{
				this.PlayByPercentage();
			}
			if (this._minDuration > 0f)
			{
				this._endDurationTime = Time.time + this._minDuration;
				this.CanCollectToCache = false;
			}
		}

		private void PlayByPercentage()
		{
			GadgetsPropertiesData componentInChildren = this._targetFXInfo.Owner.GetComponentInChildren<GadgetsPropertiesData>();
			if (!componentInChildren)
			{
				return;
			}
			float cooldownPercentage = componentInChildren.GetCooldownPercentage(this.gadgetSlot);
			this._animator.Play(this.animationStateName, -1, cooldownPercentage);
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

		[SerializeField]
		[FormerlySerializedAs("animator")]
		private Animator _animator;

		[SerializeField]
		[FormerlySerializedAs("animatorFromTarget")]
		private bool _animatorFromTarget;

		[SerializeField]
		[FormerlySerializedAs("AnimatorLocation")]
		private AnimatorVFX.EAnimatorLocation _animatorLocation;

		[SerializeField]
		[FormerlySerializedAs("triggerName")]
		private string _triggerName = string.Empty;

		[SerializeField]
		[FormerlySerializedAs("minDuration")]
		private float _minDuration;

		[SerializeField]
		private bool checkGadgetPercentage;

		[SerializeField]
		private GadgetSlot gadgetSlot;

		[SerializeField]
		private string animationStateName;

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
