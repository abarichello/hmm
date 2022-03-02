using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using NewParticleSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(MasterVFX))]
	public class DethLaserVFX : HMMTeamLineVFX
	{
		protected override void Awake()
		{
			base.Awake();
			this._updater = new TimedUpdater(50, false, false);
			this._masterVfx = base.GetComponent<MasterVFX>();
			if (this.DamageVFX != null)
			{
				GameHubBehaviour.Hub.Resources.PrefabPreCache(this.DamageVFX, 1);
			}
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			this._line.material.SetTextureScale("_MainTex", new Vector2(1f, 1f));
			if (this._targetFXInfo.Target != null && this.DamageVFX != null)
			{
				PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Target.ObjId);
				if (playerOrBotsByObjectId != null)
				{
					Bounds bounds = playerOrBotsByObjectId.CharacterInstance.GetComponent<Collider>().bounds;
					this._targetRadius = Mathf.Max(bounds.size.x, bounds.size.z) * 0.5f;
				}
				else
				{
					Debug.LogWarning(string.Format("PlayerData for target ObjId:{0} not found", this._targetFXInfo.Target.ObjId), this);
				}
				this._damageVFX = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.DamageVFX, this._target.position, this._target.rotation);
				GameHubBehaviour.Hub.Drawer.AddEffect(this._damageVFX.transform);
				this._damageVFX.baseMasterVFX = this.DamageVFX;
				this._damageVFX.Activate(this._masterVfx.TargetFX);
				if (this.HitPointParticle)
				{
					this.HitPointParticle.Play();
				}
			}
			if (this.MuzzleParticle)
			{
				this.MuzzleParticle.Play();
			}
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			if (this.MuzzleParticle)
			{
				this.MuzzleParticle.Stop();
			}
			if (this.HitPointParticle)
			{
				this.HitPointParticle.Stop();
			}
			if (this._damageVFX)
			{
				this._damageVFX.Destroy(BaseFX.EDestroyReason.Default);
			}
			this._targetRadius = 0f;
			this._damageVFX = null;
		}

		protected override void LateUpdate()
		{
			if (!this._active)
			{
				return;
			}
			this.UpdateSourceAndTarget();
			if (this._line)
			{
				this._line.SetPosition(0, this.source);
				this._line.SetPosition(1, this.target);
				Quaternion rotation = Quaternion.LookRotation(this._laserDir);
				if (this.MuzzleParticle)
				{
					this.MuzzleParticle.transform.position = this.source;
					this.MuzzleParticle.transform.rotation = rotation;
				}
				if (this.HitPointParticle)
				{
					this.HitPointParticle.transform.position = this.target;
					this.HitPointParticle.transform.rotation = rotation;
				}
			}
		}

		protected override void UpdateSourceAndTarget()
		{
			base.UpdateSourceAndTarget();
			if (this._target == null)
			{
				Vector3 forward = this._owner.forward;
				forward.y = 0f;
				forward.Normalize();
				float range = this._targetFXInfo.Gadget.GetRange();
				this.target = this.source + forward * range;
				if (!this._updater.ShouldHalt())
				{
					RaycastHit2D raycastHit2D = PhysicsUtils.CircleCast(this._targetFXInfo.Owner.transform.position, this._targetFXInfo.Gadget.Radius, forward, range, 512);
					if (raycastHit2D.collider != null)
					{
						this._targetCollision = this.source + forward * raycastHit2D.distance;
						if (this.HitPointParticle)
						{
							this.HitPointParticle.Play();
						}
					}
					else
					{
						this._targetCollision = Vector3.zero;
						if (this.HitPointParticle)
						{
							this.HitPointParticle.Stop();
						}
					}
				}
				if (this._targetCollision != Vector3.zero)
				{
					this.target = this._targetCollision;
				}
				this._laserDir = forward;
			}
			else
			{
				this.target.y = this.source.y;
				this._laserDir = (this.target - this.source).normalized;
				this.target -= this._laserDir * this._targetRadius;
			}
		}

		public static BitLogger Log = new BitLogger(typeof(DethLaserVFX));

		public HoplonParticleSystem MuzzleParticle;

		public HoplonParticleSystem HitPointParticle;

		public MasterVFX DamageVFX;

		private MasterVFX _masterVfx;

		private MasterVFX _damageVFX;

		private Vector3 _laserDir;

		private float _targetRadius;

		private TimedUpdater _updater;

		private Vector3 _targetCollision;
	}
}
