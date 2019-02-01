using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(MasterVFX))]
	public class CloneVFX : BaseVFX
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.Resources.PrefabPreCache(this.PrefabToClone, this.PrefabCacheCopies);
				this.clones = new Queue<MasterVFX>(this.PrefabCacheCopies);
				this.master = base.GetComponent<MasterVFX>();
				this.spawnDistanceSqr = this.SpawnDistance * this.SpawnDistance;
			}
			else
			{
				Debug.LogWarning("No cache available! Disabling CloneVFX");
				base.enabled = false;
			}
		}

		private void Update()
		{
			if (this._active)
			{
				int num = 1;
				bool flag = false;
				CloneVFX.ECloneBy cloneBy = this.CloneBy;
				if (cloneBy != CloneVFX.ECloneBy.Time)
				{
					if (cloneBy == CloneVFX.ECloneBy.Distance)
					{
						if (Time.timeScale > 0f)
						{
							float num2 = Vector3.SqrMagnitude(this._targetFXInfo.EffectTransform.position - this.spawnPosition);
							if (this.spawnDistanceSqr < num2)
							{
								flag = true;
								num = Mathf.FloorToInt(Mathf.Sqrt(num2 / this.spawnDistanceSqr));
							}
						}
						else
						{
							this.spawnPosition = this._targetFXInfo.EffectTransform.position;
						}
					}
				}
				else
				{
					this.spawnTimer += Time.deltaTime;
					if (this.spawnTimer > this.SpawnTime)
					{
						this.spawnTimer = 0f;
						flag = true;
					}
				}
				if (flag)
				{
					Vector3 b = Vector3.zero;
					switch (this.CloneDirection)
					{
					case CloneVFX.ECloneDirection.Forward:
						b = base.transform.forward * (this.SpawnDistance + this.OffSet);
						break;
					case CloneVFX.ECloneDirection.Backward:
						b = -base.transform.forward * (this.SpawnDistance + this.OffSet);
						break;
					}
					while (num-- > 0)
					{
						this.spawnPosition += b;
						MasterVFX masterVFX = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.PrefabToClone, this.spawnPosition, base.transform.rotation);
						masterVFX.transform.parent = GameHubBehaviour.Hub.Drawer.Effects;
						masterVFX.baseMasterVFX = this.PrefabToClone;
						masterVFX.Activate(this.master.TargetFX);
						this.clones.Enqueue(masterVFX);
					}
				}
			}
			if (this.clones.Count > 0)
			{
				MasterVFX masterVFX2 = this.clones.Peek();
				if (masterVFX2.currentState != MasterVFX.State.Activating && Time.time - masterVFX2.StartTime > this.ExpireTime)
				{
					this.clones.Dequeue();
					masterVFX2.Destroy(this.destroyReason);
				}
			}
			else if (!this._active)
			{
				this.CanCollectToCache = true;
			}
		}

		protected override void OnActivate()
		{
			this._active = true;
			this.spawnTimer = 0f;
			this.spawnPosition = this._targetFXInfo.EffectTransform.position;
			if (this.ForceSpawnOnGround)
			{
				this.spawnPosition.y = 0f;
			}
			this.CanCollectToCache = false;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			this._active = false;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CloneVFX));

		public MasterVFX PrefabToClone;

		public int PrefabCacheCopies = 1;

		public CloneVFX.ECloneDirection CloneDirection;

		public CloneVFX.ECloneBy CloneBy;

		public float SpawnDistance = 10f;

		public float SpawnTime = 0.25f;

		public float ExpireTime = 0.5f;

		public float OffSet;

		[Tooltip("Forces the clones to spawn at y = 0 (may be useful when using AttachVFX on dummies)")]
		public bool ForceSpawnOnGround;

		private bool _active;

		private float spawnDistanceSqr;

		private float spawnTimer;

		private Vector3 spawnPosition;

		private Queue<MasterVFX> clones;

		private MasterVFX master;

		public enum ECloneDirection
		{
			Origin,
			Forward,
			Backward
		}

		public enum ECloneBy
		{
			Time,
			Distance
		}
	}
}
