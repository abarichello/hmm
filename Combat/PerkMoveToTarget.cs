using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkMoveToTarget : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			EffectEvent data = this.Effect.Data;
			this._moveSpeed = data.MoveSpeed;
			this._lastMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this._shouldDestroy = false;
			this._targetCombat = this.Effect.GetTargetCombat(this.Target);
			if (this._targetCombat && this._targetCombat.IsAlive())
			{
				this._targetTransform = this._targetCombat.transform;
				this._targetCombat.ListenToObjectUnspawn += this.OnObjectDeath;
			}
			else
			{
				this._targetTransform = null;
				this.TargetPosition = base._trans.position + base._trans.forward * this._moveSpeed * this.Effect.Data.LifeTime;
			}
		}

		protected Vector3 TargetPosition
		{
			get
			{
				if (this.Effect.Data.EffectInfo.ShotPosAndDir.Dummy != CDummy.DummyKind.None && this._targetCombat)
				{
					this.TargetPosition = GadgetBehaviour.DummyPosition(this._targetCombat, this.Effect.Data.EffectInfo);
				}
				else if (this._targetTransform)
				{
					this.TargetPosition = this._targetTransform.position;
				}
				return this._targetPosition;
			}
			set
			{
				this._targetPosition = value;
				this.Effect.Data.Target = value;
			}
		}

		private void OnObjectDeath(CombatObject obj, UnspawnEvent msg)
		{
			this._targetTransform = null;
			this._targetCombat = null;
			this.TargetPosition = base._trans.position + base._trans.forward * this._moveSpeed * (float)(this.Effect.Data.DeathTime - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime()) / 1000f;
			obj.ListenToObjectUnspawn -= this.OnObjectDeath;
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (this._targetCombat)
			{
				this._targetCombat.ListenToObjectUnspawn -= this.OnObjectDeath;
			}
			this._targetPosition = Vector3.zero;
			this._targetTransform = null;
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			this.UpdatePosition();
		}

		private void FixedUpdate()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			Vector3 position = base._trans.position;
			Vector3 targetPosition = this.TargetPosition;
			targetPosition.y = base.transform.position.y;
			float sqrMagnitude = (position - targetPosition).sqrMagnitude;
			Vector3 j = this.CalcNewPosition();
			if (PhysicsUtils.SphereIntersect(position, j, this.Effect.Data.SourceCombat.transform.position, 3f) || sqrMagnitude < this.DestroyOnRemainingDistance)
			{
				base._trans.position = this.Effect.Data.SourceCombat.transform.position;
				this._shouldDestroy = true;
			}
			if (this._shouldDestroy)
			{
				this.Effect.TriggerDestroy(-1, base._trans.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
				return;
			}
			this.UpdatePosition();
		}

		private Vector3 CalcNewPosition()
		{
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			float num = (float)((long)playbackTime - this._lastMillis) * 0.001f;
			Vector3 forward = base._trans.forward;
			Vector3 vector;
			vector..ctor(forward.x, 0f, forward.z);
			vector.Normalize();
			return base._trans.position + vector * this._moveSpeed * num;
		}

		public virtual void UpdatePosition()
		{
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			float num = (float)((long)playbackTime - this._lastMillis) * 0.001f;
			this._lastMillis = (long)playbackTime;
			Quaternion rotation = base._trans.rotation;
			base._trans.LookAt(this.TargetPosition);
			Vector3 forward = base._trans.forward;
			Vector3 vector;
			vector..ctor(forward.x, 0f, forward.z);
			vector.Normalize();
			base._trans.position += vector * this._moveSpeed * num;
			if (this.LockRotation)
			{
				base._trans.rotation = rotation;
			}
		}

		protected Transform _targetTransform;

		protected CombatObject _targetCombat;

		protected float _moveSpeed;

		protected long _lastMillis;

		public BasePerk.PerkTarget Target = BasePerk.PerkTarget.Target;

		public CDummy.DummyKind DummyKind;

		public string CustomDummyName;

		public bool LockRotation;

		private bool _shouldDestroy;

		public float DestroyOnRemainingDistance;

		private Vector3 _targetPosition;
	}
}
