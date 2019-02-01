using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnRangeOwner2Target : BasePerk
	{
		private Vector3 TargetPosition
		{
			get
			{
				return (!this._target) ? this.Effect.Data.Origin : this._target.position;
			}
		}

		public override void PerkInitialized()
		{
			Identifiable owner = this.Effect.Owner;
			Identifiable target = this.Effect.Target;
			if (owner)
			{
				this._owner = this.Effect.Owner.transform;
			}
			if (target && target != owner)
			{
				this._target = this.Effect.Target.transform;
			}
			this._sqrRange = this.Effect.Data.Range * this.Effect.Data.Range;
			this._destroyed = false;
		}

		private void FixedUpdate()
		{
			if (this._updater.ShouldHalt())
			{
				return;
			}
			if (!this._owner || this._destroyed)
			{
				return;
			}
			float num = Vector3.SqrMagnitude(this._owner.position - this.TargetPosition);
			if (num > this._sqrRange)
			{
				if ((float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() >= this._timeToDestroy)
				{
					if (GameHubBehaviour.Hub.Net.IsServer())
					{
						this.ServerDestroy();
					}
					else if (this.m_boIsActive)
					{
						this.Effect.DeactivateEffect(this.PerkVFXCondition);
						this.m_boIsActive = false;
					}
				}
				else if (!this.m_boIsActive && !GameHubBehaviour.Hub.Net.IsServer())
				{
					this.Effect.ActivateEffect(this.PerkVFXCondition);
					this.m_boIsActive = true;
				}
			}
			else
			{
				this._timeToDestroy = (float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + this.TimeToReturnToRangeInMillis;
				if (this.m_boIsActive && !GameHubBehaviour.Hub.Net.IsServer())
				{
					this.Effect.DeactivateEffect(this.PerkVFXCondition);
					this.m_boIsActive = false;
				}
			}
		}

		private void ServerDestroy()
		{
			if (this.DestroyDamage != null && this.DestroyDamage.Length > 0)
			{
				for (int i = 0; i < this.DestroyDamage.Length; i++)
				{
					this.DestroyDamage[i].CauseDamage();
				}
			}
			this._timeToDestroy = 0f;
			this._destroyed = true;
			this.Effect.TriggerDestroy(-1, base.transform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDestroyOnRangeOwner2Target));

		private Transform _target;

		private Transform _owner;

		private float _sqrRange;

		private bool _destroyed;

		public float TimeToReturnToRangeInMillis;

		private float _timeToDestroy;

		public PerkDamageOnRequest[] DestroyDamage;

		private bool m_boIsActive;

		private TimedUpdater _updater = new TimedUpdater(100, false, false);
	}
}
