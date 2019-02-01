using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkChangeColliderSize : BasePerk
	{
		public override void PerkInitialized()
		{
			this._target = base.GetTargetCombat(this.Effect, this.Target);
			PerkChangeColliderSize.ColliderType colliderShape = this.ColliderShape;
			if (colliderShape != PerkChangeColliderSize.ColliderType.BoxCollider)
			{
				if (colliderShape != PerkChangeColliderSize.ColliderType.SphereCollider)
				{
					if (colliderShape == PerkChangeColliderSize.ColliderType.CapsuleCollider)
					{
						CapsuleCollider bitComponent = this._target.Id.GetBitComponent<CapsuleCollider>();
						if (bitComponent == null)
						{
							PerkChangeColliderSize.Log.ErrorFormat("{0} does not have a CapsuleCollider. {1}", new object[]
							{
								this._target,
								this.Effect
							});
							return;
						}
						this._oldCenter = bitComponent.center;
						this._oldRadius = bitComponent.radius;
						this._oldHeight = bitComponent.height;
						bitComponent.radius = this.Radius;
						bitComponent.height = this.Height;
						bitComponent.center = this.Center;
					}
				}
				else
				{
					SphereCollider bitComponent2 = this._target.Id.GetBitComponent<SphereCollider>();
					if (bitComponent2 == null)
					{
						PerkChangeColliderSize.Log.ErrorFormat("{0} does not have a SphereCollider. {1}", new object[]
						{
							this._target,
							this.Effect
						});
						return;
					}
					this._oldCenter = bitComponent2.center;
					this._oldRadius = bitComponent2.radius;
					bitComponent2.radius = this.Radius;
					bitComponent2.center = this.Center;
				}
			}
			else
			{
				BoxCollider bitComponent3 = this._target.Id.GetBitComponent<BoxCollider>();
				if (bitComponent3 == null)
				{
					PerkChangeColliderSize.Log.ErrorFormat("{0} does not have a BoxCollider. {1}", new object[]
					{
						this._target,
						this.Effect
					});
					return;
				}
				this._oldSize = bitComponent3.size;
				this._oldCenter = bitComponent3.center;
				bitComponent3.size = this.Size;
				bitComponent3.center = this.Center;
			}
		}

		public override void PerkDestroyed(DestroyEffect destroyEffect)
		{
			PerkChangeColliderSize.ColliderType colliderShape = this.ColliderShape;
			if (colliderShape != PerkChangeColliderSize.ColliderType.BoxCollider)
			{
				if (colliderShape != PerkChangeColliderSize.ColliderType.SphereCollider)
				{
					if (colliderShape == PerkChangeColliderSize.ColliderType.CapsuleCollider)
					{
						CapsuleCollider bitComponent = this._target.Id.GetBitComponent<CapsuleCollider>();
						if (bitComponent == null)
						{
							PerkChangeColliderSize.Log.ErrorFormat("{0} does not have a CapsuleCollider. {1}", new object[]
							{
								this._target,
								this.Effect
							});
							return;
						}
						bitComponent.radius = this._oldRadius;
						bitComponent.height = this._oldHeight;
						bitComponent.center = this._oldCenter;
					}
				}
				else
				{
					SphereCollider bitComponent2 = this._target.Id.GetBitComponent<SphereCollider>();
					if (bitComponent2 == null)
					{
						PerkChangeColliderSize.Log.ErrorFormat("{0} does not have a SphereCollider. {1}", new object[]
						{
							this._target,
							this.Effect
						});
						return;
					}
					bitComponent2.radius = this._oldRadius;
					bitComponent2.center = this._oldCenter;
				}
			}
			else
			{
				BoxCollider bitComponent3 = this._target.Id.GetBitComponent<BoxCollider>();
				if (bitComponent3 == null)
				{
					PerkChangeColliderSize.Log.ErrorFormat("{0} does not have a BoxCollider. {1}", new object[]
					{
						this._target,
						this.Effect
					});
					return;
				}
				bitComponent3.size = this._oldSize;
				bitComponent3.center = this._oldCenter;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkChangeColliderSize));

		public BasePerk.PerkTarget Target;

		public PerkChangeColliderSize.ColliderType ColliderShape;

		public Vector3 Center;

		public Vector3 Size;

		public float Radius;

		public float Height;

		private Vector3 _oldCenter;

		private Vector3 _oldSize;

		private float _oldRadius;

		private float _oldHeight;

		private CombatObject _target;

		public enum ColliderType
		{
			BoxCollider,
			SphereCollider,
			CapsuleCollider
		}
	}
}
