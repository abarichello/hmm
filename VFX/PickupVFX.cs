using System;
using HeavyMetalMachines.Combat;
using Hoplon.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(MasterVFX))]
	public class PickupVFX : BaseVFX
	{
		protected void Awake()
		{
			this._master = base.GetComponent<MasterVFX>();
			if (this.PickupPrefab != null && this._master != null)
			{
				int num = Mathf.CeilToInt(this.TimeToReachDestination / this.SpawnPeriod);
				this._pickups = new CircularQueue<PickupVFX.PickupData>((uint)num);
				this._updater = new TimedUpdater(Mathf.CeilToInt(1000f * this.SpawnPeriod), false, false);
				GameHubBehaviour.Hub.Resources.PrefabPreCache(this.PickupPrefab, num);
			}
			else
			{
				base.enabled = false;
				this.CanCollectToCache = true;
			}
			this._invTimeToReach = ((this.TimeToReachDestination <= 0f) ? 0f : (1f / this.TimeToReachDestination));
		}

		protected void OnDestroy()
		{
			if (this._pickups != null)
			{
				this._pickups.Clear();
			}
			this._master = null;
			this._pickups = null;
			this._originCombat = (this._destinationCombat = null);
		}

		protected override void OnActivate()
		{
			if (this._pickups == null)
			{
				base.enabled = false;
				this.CanCollectToCache = true;
				return;
			}
			PickupVFX.DirectionType direction = this.Direction;
			if (direction != PickupVFX.DirectionType.FromOwnerToTarget)
			{
				if (direction == PickupVFX.DirectionType.FromTargetToOwner)
				{
					this._originCombat = this._targetFXInfo.Target.GetBitComponent<CombatObject>();
					this._destinationCombat = this._targetFXInfo.Owner.GetBitComponent<CombatObject>();
				}
			}
			else
			{
				this._originCombat = this._targetFXInfo.Owner.GetBitComponent<CombatObject>();
				this._destinationCombat = this._targetFXInfo.Target.GetBitComponent<CombatObject>();
			}
			this._isSpawningEnabled = true;
			this.CanCollectToCache = false;
		}

		private void Update()
		{
			if (this.CanCollectToCache)
			{
				return;
			}
			if (this._isSpawningEnabled && !this._updater.ShouldHalt() && !this._pickups.IsFull)
			{
				if (this._originCombat.IsAlive())
				{
					this.SpawnPickup();
				}
				else
				{
					this._isSpawningEnabled = false;
				}
			}
			this.UpdatePickups();
			this.CanCollectToCache = (!this._isSpawningEnabled && this._pickups.Size == 0U);
		}

		protected override void OnDeactivate()
		{
			this._isSpawningEnabled = false;
		}

		protected override void WillDeactivate()
		{
		}

		private void SpawnPickup()
		{
			PickupVFX.PickupData pickupData;
			pickupData.StartTime = Time.time;
			pickupData.Height = Random.Range(0.75f, 1.15f) * this.Height;
			pickupData.Amplitude = ((Random.value <= 0.5f) ? -1f : 1f) * Random.Range(0.75f, 1.15f) * this.Amplitude;
			pickupData.StartPosition = this._originCombat.transform.position;
			pickupData.MasterVfx = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.PickupPrefab, pickupData.StartPosition, Quaternion.identity);
			pickupData.MasterVfx.baseMasterVFX = this.PickupPrefab;
			pickupData.VfxTransform = pickupData.MasterVfx.transform;
			GameHubBehaviour.Hub.Drawer.AddEffect(pickupData.VfxTransform);
			pickupData.MasterVfx.Activate(this._master.TargetFX, pickupData.VfxTransform);
			this._pickups.Push(pickupData);
		}

		private void UpdatePickups()
		{
			int num = 0;
			bool flag = this._destinationCombat.IsAlive();
			if (flag)
			{
				Vector3 position = this._destinationCombat.transform.position;
				for (uint num2 = 0U; num2 < this._pickups.Size; num2 += 1U)
				{
					float num3 = (Time.time - this._pickups[num2].StartTime) * this._invTimeToReach;
					Vector3 vector = Vector3.Lerp(this._pickups[num2].StartPosition, position, num3);
					PickupVFX.MovementType movement = this.Movement;
					if (movement != PickupVFX.MovementType.Parabolic)
					{
						if (movement != PickupVFX.MovementType.Sinusoid)
						{
							if (movement == PickupVFX.MovementType.ParabolicAndSinusoid)
							{
								Vector3 normalized = Vector3.Cross(position - this._pickups[num2].StartPosition, Vector3.up).normalized;
								vector.y += -4f * this._pickups[num2].Height * num3 * (num3 - 1f);
								vector += this._pickups[num2].Amplitude * Mathf.Sin(6.2831855f * (float)this.Octave * num3) * normalized;
							}
						}
						else
						{
							Vector3 normalized2 = Vector3.Cross(position - this._pickups[num2].StartPosition, Vector3.up).normalized;
							vector += this._pickups[num2].Amplitude * Mathf.Sin(6.2831855f * (float)this.Octave * num3) * normalized2;
						}
					}
					else
					{
						vector.y += -4f * this._pickups[num2].Height * num3 * (num3 - 1f);
					}
					this._pickups[num2].VfxTransform.position = vector;
					num += ((num3 < 1f) ? 0 : 1);
				}
			}
			else
			{
				num = (int)this._pickups.Size;
				this._isSpawningEnabled = false;
			}
			while (num-- > 0)
			{
				this._pickups[0U].MasterVfx.Destroy((!flag) ? BaseFX.EDestroyReason.Default : BaseFX.EDestroyReason.HitIdentifiable);
				this._pickups.Pop();
			}
		}

		[Tooltip("The pickup prefab (MasterVFX) to spawn.")]
		public MasterVFX PickupPrefab;

		private MasterVFX _master;

		[Tooltip("The period in seconds for spawning a new pickup.")]
		public float SpawnPeriod = 1f;

		[Tooltip("The time in seconds that will take for the pickup to reach its destination.")]
		public float TimeToReachDestination = 1f;

		private float _invTimeToReach;

		[Tooltip("The movement direction of the pickup.")]
		public PickupVFX.DirectionType Direction;

		public PickupVFX.MovementType Movement;

		[Tooltip("The parabola vertex (maximum height).")]
		public float Height = 5f;

		[Tooltip("The amplitude of the sine wave.")]
		public float Amplitude = 10f;

		[Tooltip("The octave of the sine wave. Larger values will make it cycle with higher frequency.")]
		public int Octave = 1;

		private CircularQueue<PickupVFX.PickupData> _pickups;

		private CombatObject _originCombat;

		private CombatObject _destinationCombat;

		private TimedUpdater _updater;

		private bool _isSpawningEnabled;

		public enum DirectionType
		{
			FromOwnerToTarget,
			FromTargetToOwner
		}

		public enum MovementType
		{
			Linear,
			Parabolic,
			Sinusoid,
			ParabolicAndSinusoid
		}

		private struct PickupData
		{
			public float StartTime;

			public float Height;

			public float Amplitude;

			public Vector3 StartPosition;

			public MasterVFX MasterVfx;

			public Transform VfxTransform;
		}
	}
}
