using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMTeamAnimatorVFX : BaseVFX
	{
		private void Start()
		{
			if (this.animator == null)
			{
				this.animator = base.GetComponent<Animator>();
			}
		}

		private void Update()
		{
			if (this.endDurationTime > 0f && Time.time > this.endDurationTime)
			{
				this.CanCollectToCache = true;
			}
		}

		protected override void OnActivate()
		{
			TeamKind team = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
			VFXTeam vfxteam = (team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam) ? VFXTeam.Ally : VFXTeam.Enemy;
			if (this.animatorFromTarget)
			{
				this.animator = this._targetFXInfo.Owner.GetComponentInChildren<Animator>();
			}
			if (this.animator)
			{
				if (!string.IsNullOrEmpty(this.triggerNameAlly) && vfxteam == VFXTeam.Ally)
				{
					this.animator.SetTrigger(this.triggerNameAlly);
				}
				else if (!string.IsNullOrEmpty(this.triggerNameEnemy) && vfxteam == VFXTeam.Enemy)
				{
					this.animator.SetTrigger(this.triggerNameEnemy);
				}
				else
				{
					this.animator.Play(0);
				}
			}
			if (this.minDuration > 0f)
			{
				this.endDurationTime = Time.time + this.minDuration;
				this.CanCollectToCache = false;
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		public Animator animator;

		public bool animatorFromTarget;

		public string triggerNameAlly;

		public string triggerNameEnemy;

		public float minDuration;

		private float endDurationTime;
	}
}
