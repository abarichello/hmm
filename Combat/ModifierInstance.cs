using System;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class ModifierInstance
	{
		public ModifierInstance()
		{
		}

		public ModifierInstance(ModifierData data, CombatObject causer, CombatObject owner, int eventId, int startTime, float lifeTime, bool barrierHit)
		{
			this.SetupInstance(data, causer, owner, eventId, startTime, lifeTime, barrierHit);
		}

		public bool IsPurgeable
		{
			get
			{
				return this.Data.Info.IsPurgeable;
			}
		}

		public bool IsDispellable
		{
			get
			{
				return this.Data.Info.IsDispellable;
			}
		}

		public bool ShouldAccountDebuff()
		{
			return GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.BombDelivery && (this.Data.Info.IsDebuff && !this.NotBeingApplied && this.Causer != null && this.Causer.Stats != null) && this.Owner.Team != this.Causer.Team;
		}

		public void SetupInstance(ModifierData data, CombatObject causer, CombatObject owner, int eventId, int startTime, float lifeTime, bool barrierHit)
		{
			this.StartTime = startTime;
			this.LifeTime = lifeTime;
			this.NextTick = (int)(data.TickDelta * 1000f + (float)startTime);
			this.Data = data;
			this.Causer = causer;
			this.Owner = owner;
			this.EventId = eventId;
			this.CrowdControlApplied = 0f;
			this.BarrierHit = barrierHit;
			this._durationTime = this.StartTime + (int)(this.LifeTime * 1000f);
			this._lastDamageTime = startTime;
			if (owner)
			{
				this.OwnerForward = owner.transform.forward;
			}
			if (data.Info.Tapered != TaperMode.None)
			{
				this.MaxTapperedTick = (int)Math.Max(1.0, Math.Floor((double)(data.LifeTime / data.TickDelta)));
			}
			if (data.DirectionSet)
			{
				this._direction = data.Direction;
				if (this._direction != this._direction.normalized)
				{
					this._direction = this._direction.normalized;
					ModifierInstance.Log.WarnFormat("Direction not normalized!!! causer:{0} owner:{1} eventId:{2}", new object[]
					{
						causer,
						owner,
						eventId
					});
				}
			}
			if (!data.DirectionSet && (data.Status.HasFlag(StatusKind.Immobilized) || data.Info.Effect == EffectKind.Impulse))
			{
				if (causer != null && causer != owner)
				{
					this._direction = this.Owner.Transform.position - this.Causer.Transform.position;
				}
				else if (owner.Body)
				{
					this._direction = owner.Transform.forward;
					ModifierInstance.Log.DebugFormat("BodyForward:{0}", new object[]
					{
						this._direction
					});
				}
				else
				{
					this._direction = Vector3.zero;
				}
				this._direction.y = 0f;
				this._direction = this._direction.normalized;
				ModifierInstance.Log.DebugFormat("Final:{0}", new object[]
				{
					this._direction
				});
			}
			if (data.PositionSet)
			{
				this.Position = data.Position;
			}
			else if (causer != null && causer != owner)
			{
				this.Position = this.Owner.Transform.position;
			}
			else if (owner.Body)
			{
				this.Position = owner.Transform.position;
			}
			else
			{
				this.Position = Vector3.zero;
			}
		}

		public ModifierInfo Info
		{
			get
			{
				return this.Data.Info;
			}
		}

		public GadgetInfo Gadget
		{
			get
			{
				return this.Data.GadgetInfo;
			}
		}

		public float Amount
		{
			get
			{
				float num = this.Data.Amount;
				switch (this.Data.Info.Tapered)
				{
				case TaperMode.Tapered:
					if (this.TaperedTick == 0)
					{
						this.TaperedTick = 1;
					}
					this.TaperPercent = (float)this.TaperedTick / (float)this.MaxTapperedTick;
					num *= this.TaperPercent * (1f + ((!this.Data.Info.UsePower) ? 0f : this.Causer.Data.PowerPct));
					goto IL_13B;
				case TaperMode.InvertTapered:
					this.TaperPercent = (float)this.TaperedTick / (float)this.MaxTapperedTick;
					num *= (1f - this.TaperPercent) * (1f + ((!this.Data.Info.UsePower) ? 0f : this.Causer.Data.PowerPct));
					goto IL_13B;
				}
				num *= 1f + ((!this.Data.Info.UsePower) ? 0f : this.Causer.Data.PowerPct);
				IL_13B:
				if (this.Data.Info.AmountPerSecond)
				{
					num *= (float)this._deltaDamageTime / 1000f;
				}
				return num;
			}
		}

		public bool Tick(int matchTime)
		{
			if (matchTime < this.NextTick)
			{
				return false;
			}
			if (this.LifeTime > 0f && matchTime > this._durationTime)
			{
				this._deltaDamageTime = this._durationTime - this._lastDamageTime;
				this._lastDamageTime = this._durationTime;
			}
			else
			{
				this._deltaDamageTime = matchTime - this._lastDamageTime;
				this._lastDamageTime = matchTime;
			}
			this.NextTick = (int)(this.Data.TickDelta * 1000f) + matchTime;
			return true;
		}

		public Vector3 GetDirectionNomalized()
		{
			return this._direction;
		}

		public Vector3 GetDirection()
		{
			return this._direction * this.Amount;
		}

		public Vector3 GetDirection(float amountReduction)
		{
			return this._direction * (this.Amount - amountReduction);
		}

		public static Predicate<ModifierInstance> CheckSameModifier(ModifierInstance mod)
		{
			ModifierInstance._currentCheckMod = mod;
			if (ModifierInstance.<>f__mg$cache0 == null)
			{
				ModifierInstance.<>f__mg$cache0 = new Predicate<ModifierInstance>(ModifierInstance.InternalCheckSameModifier);
			}
			return ModifierInstance.<>f__mg$cache0;
		}

		private static bool InternalCheckSameModifier(ModifierInstance x)
		{
			return x.Data.Info == ModifierInstance._currentCheckMod.Data.Info && x.Data.GadgetInfo == ModifierInstance._currentCheckMod.Data.GadgetInfo && x.Owner.Team == ModifierInstance._currentCheckMod.Owner.Team && ModifierInstance._currentCheckMod.BarrierHit == x.BarrierHit;
		}

		public static Predicate<ModifierInstance> CheckSameModifier(ModifierData mod, CombatObject causer)
		{
			ModifierInstance._currentCheckModData = mod;
			ModifierInstance._currentCheckModDataCauser = causer;
			if (ModifierInstance.<>f__mg$cache1 == null)
			{
				ModifierInstance.<>f__mg$cache1 = new Predicate<ModifierInstance>(ModifierInstance.InternalCheckSameModifierData);
			}
			return ModifierInstance.<>f__mg$cache1;
		}

		private static bool InternalCheckSameModifierData(ModifierInstance x)
		{
			return x.Data.Info == ModifierInstance._currentCheckModData.Info && x.Data.GadgetInfo == ModifierInstance._currentCheckModData.GadgetInfo && (ModifierInstance._currentCheckModDataCauser == null || x.Causer.Team == ModifierInstance._currentCheckModDataCauser.Team);
		}

		public override string ToString()
		{
			return string.Format("Data: {0}, Causer: {1}, Owner: {2}, EventId: {3}, StartTime: {4}, LifeTime: {5}", new object[]
			{
				this.Data,
				this.Causer,
				this.Owner,
				this.EventId,
				this.StartTime,
				this.LifeTime
			});
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ModifierInstance));

		public ModifierData Data;

		public CombatObject Causer;

		public CombatObject Owner;

		public int EventId;

		public Vector3 OwnerForward;

		public int StartTime;

		public float LifeTime;

		public int NextTick;

		private int _deltaDamageTime;

		private int _lastDamageTime;

		public int TaperedTick;

		public int MaxTapperedTick;

		private float TaperPercent;

		public int FeedbackId = -1;

		private int _durationTime;

		public bool NotBeingApplied;

		public bool BarrierHit;

		public float CrowdControlApplied;

		private Vector3 _direction;

		public Vector3 Position;

		private static ModifierInstance _currentCheckMod;

		private static ModifierData _currentCheckModData;

		private static CombatObject _currentCheckModDataCauser;

		[CompilerGenerated]
		private static Predicate<ModifierInstance> <>f__mg$cache0;

		[CompilerGenerated]
		private static Predicate<ModifierInstance> <>f__mg$cache1;
	}
}
