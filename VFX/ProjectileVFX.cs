using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(MasterVFX))]
	public class ProjectileVFX : BaseVFX
	{
		public override int Priority
		{
			get
			{
				return 1999;
			}
		}

		private void Awake()
		{
			this._master = base.GetComponent<MasterVFX>();
			if (GameHubBehaviour.Hub && this.ProjectilePrefab != null && this._master != null)
			{
				GameHubBehaviour.Hub.Resources.PrefabPreCache(this.ProjectilePrefab, 1);
			}
			else
			{
				base.enabled = false;
				this.CanCollectToCache = true;
			}
		}

		private void OnDestroy()
		{
			this._master = null;
		}

		protected override void OnActivate()
		{
			this._activationTime = Time.time;
			this._origin = base.transform.position;
			this._direction = base.transform.forward;
			this._radius = this._targetFXInfo.Gadget.Radius;
			this._range = this._targetFXInfo.Gadget.GetRange();
			this._hasHitScenery = false;
			if (this.HitScenery)
			{
				RaycastHit2D raycastHit2D = PhysicsUtils.CircleCast(this._origin, this._radius, this._direction, this._range, 512);
				this._hasHitScenery = (raycastHit2D.collider != null);
				if (this._hasHitScenery)
				{
					this._range = raycastHit2D.distance;
				}
			}
			this._vfxInstance = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.ProjectilePrefab, base.transform.position, base.transform.rotation);
			this.CanCollectToCache = (this._vfxInstance == null);
			if (!this.CanCollectToCache)
			{
				this._vfxTransform = this._vfxInstance.transform;
				GameHubBehaviour.Hub.Drawer.AddEffect(this._vfxInstance.transform);
				this._vfxInstance.baseMasterVFX = this.ProjectilePrefab;
				this._vfxInstance.Activate(this._master.TargetFX, this._vfxInstance.transform);
			}
		}

		private void Update()
		{
			if (this.CanCollectToCache)
			{
				return;
			}
			float num = Mathf.Min(this._range, this.Speed * (Time.time - this._activationTime));
			this._vfxTransform.position = this._origin + num * this._direction;
			this._vfxTransform.rotation = Quaternion.LookRotation(this._direction);
			if (num >= this._range)
			{
				this.CanCollectToCache = true;
				this._vfxInstance.Destroy((!this._hasHitScenery) ? BaseFX.EDestroyReason.Default : BaseFX.EDestroyReason.HitScenery);
				this._vfxInstance = null;
				this._vfxTransform = null;
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		[Tooltip("The projectile prefab (MasterVFX) to spawn.")]
		public MasterVFX ProjectilePrefab;

		private MasterVFX _vfxInstance;

		private Transform _vfxTransform;

		private MasterVFX _master;

		[Tooltip("The movement speed of the projectile.")]
		public float Speed;

		[Tooltip("Should the projectile hit the scenery (walls, pillars, etc)")]
		public bool HitScenery;

		private float _radius;

		private float _range;

		private float _activationTime;

		private bool _hasHitScenery;

		private Vector3 _direction;

		private Vector3 _origin;
	}
}
