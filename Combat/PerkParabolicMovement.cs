using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkParabolicMovement : BasePerk, DestroyEffect.IDestroyEffectListener, IPerkMovement
	{
		private float DeltaTime
		{
			get
			{
				return (float)(GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._startTime) * 0.001f;
			}
		}

		private Transform TargetTransform
		{
			get
			{
				return (!(this._targetCombat != null)) ? null : this._targetCombat.transform;
			}
		}

		private Vector3 TargetPosition
		{
			get
			{
				if (this.FollowTarget && this.TargetTransform)
				{
					if (this._targetCombat.IsAlive())
					{
						this._validTargetPosition = this.TargetTransform.position;
					}
				}
				else
				{
					this._validTargetPosition = this.Effect.Data.Target;
				}
				return this._validTargetPosition;
			}
		}

		public override void PerkInitialized()
		{
			if (this._initialized)
			{
				return;
			}
			this._initialized = true;
			this._shouldDestroy = false;
			this.CalcParabola();
			EffectEvent data = this.Effect.Data;
			bool flag = GameHubBehaviour.Hub.Net.IsServer();
			if (!flag && this.Effect.Data.FirstPackageSent)
			{
				this.UpdateAfterVisibilityChange();
				return;
			}
			this._startTime = ((!flag) ? GameHubBehaviour.Hub.GameTime.GetPlaybackTime() : data.EventTime);
			if (data.CustomVar > 0)
			{
				float num = (float)data.CustomVar / 255f;
				num *= data.LifeTime;
				num = (float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - num * 1000f;
				if (num > 0f)
				{
					this._startTime = (int)num;
				}
				this.Effect.Data.EventTime = this._startTime;
			}
			if (this.FollowTarget)
			{
				this._targetCombat = this.Effect.GetTargetCombat(this.Target);
				if (this._targetCombat && this._targetCombat.IsAlive())
				{
					this._targetCombat.ListenToObjectUnspawn += this.OnObjectDeath;
				}
			}
		}

		private void OnObjectDeath(CombatObject obj, UnspawnEvent msg)
		{
			this._targetCombat = null;
			obj.ListenToObjectUnspawn -= this.OnObjectDeath;
		}

		public override void PerkDestroyed(DestroyEffect destroyEffect)
		{
			this._initialized = false;
		}

		private void UpdateAfterVisibilityChange()
		{
			this._startTime = this.Effect.Data.EventTime;
			this.CalcParabola();
			this.UpdatePosition();
		}

		public Vector3 UpdatePosition()
		{
			if (!this._initialized)
			{
				this.PerkInitialized();
			}
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				this.PreUpdatePosition();
			}
			Vector3 position = base._trans.position;
			EffectEvent data = this.Effect.Data;
			float num = Mathf.Clamp(this.DeltaTime / data.LifeTime, 0f, 1f);
			Vector3 result = Vector3.Lerp(data.Origin, this.TargetPosition, num);
			if (this.useCurve)
			{
				result.y = this._zeroHeight + Mathf.Lerp(Mathf.Lerp(data.Origin.y - this._zeroHeight, this.TargetPosition.y - this._zeroHeight, num), data.EffectInfo.Height, this.animationCurve.Evaluate(num));
			}
			else
			{
				result.y = this._zeroHeight + (this._a * num * num + this._b * num + this._c);
			}
			if (float.IsNaN(result.x) || float.IsNaN(result.y) || float.IsNaN(result.z))
			{
			}
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				this.PosUpdatePostion(position);
			}
			return result;
		}

		private void PreUpdatePosition()
		{
			if (this._shouldDestroy)
			{
				this.Effect.TriggerDestroy(-1, base._trans.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
				return;
			}
		}

		private void PosUpdatePostion(Vector3 initialPosition)
		{
			if (!this.DestroyOnReach)
			{
				return;
			}
			Vector3 targetPosition = this.TargetPosition;
			targetPosition.y = base._trans.position.y;
			float sqrMagnitude = (initialPosition - targetPosition).sqrMagnitude;
			float sqrMagnitude2 = (base._trans.position - initialPosition).sqrMagnitude;
			if (sqrMagnitude <= this.DestroyOnRemainingDistance || sqrMagnitude2 >= sqrMagnitude)
			{
				this._shouldDestroy = true;
			}
		}

		private void CalcParabola()
		{
			EffectEvent data = this.Effect.Data;
			this._zeroHeight = Mathf.Max(data.Origin.y, this.TargetPosition.y);
			float num = data.Origin.y - this._zeroHeight;
			float num2 = this.TargetPosition.y - this._zeroHeight;
			this._c = num;
			float num3 = 1f / (4f * (num - data.EffectInfo.Height));
			float num4 = num - num2;
			float a = (-1f + Mathf.Sqrt(1f - 4f * num3 * num4)) / (2f * num3);
			float b = (-1f - Mathf.Sqrt(1f - 4f * num3 * num4)) / (2f * num3);
			this._b = Mathf.Max(a, b);
			this._a = num2 - num - this._b;
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			this._targetCombat = null;
		}

		private void OnDrawGizmos()
		{
			EffectEvent data = this.Effect.Data;
			if (data == null)
			{
				return;
			}
			Gizmos.DrawLine(data.Origin, data.Target);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkParabolicMovement));

		private float _moved;

		private float _a;

		private float _b;

		private float _c;

		private float _zeroHeight;

		private int _startTime;

		private bool _initialized;

		public bool useCurve;

		public AnimationCurve animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public bool FollowTarget;

		public BasePerk.PerkTarget Target = BasePerk.PerkTarget.Target;

		private bool _shouldDestroy;

		public bool DestroyOnReach;

		public float DestroyOnRemainingDistance;

		private CombatObject _targetCombat;

		private Vector3 _validTargetPosition;
	}
}
