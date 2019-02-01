using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public abstract class BaseDamageablePerk : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this.Modifiers = base.GetModifiers(this.Source);
			this.TargetCombat = base.GetTargetCombat(this.Effect, this.Target);
			this.AdditionalTargetCombat = base.GetTargetCombat(this.Effect, this.AdditionalTargetOnApplyDamage);
			if (!this.TargetCombat && this.Target != BasePerk.PerkTarget.None)
			{
				base.enabled = false;
				return;
			}
			this._perkInitializedTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (this.Effect.Data.PreviousEventId != -1 && this.SaveSlot != DataHolderSlot.None)
			{
				this.DamagedCount = PerkDataHolder<int>.PopData(this.Effect.Data.PreviousEventId, this.SaveSlot);
				if (this.HitOnlyOnceWithinSameSlot)
				{
					this.Damaged = PerkDataHolder<List<int>>.PopData(this.Effect.Data.PreviousEventId, this.SaveSlot);
				}
				else
				{
					this.Damaged.Clear();
				}
			}
			else
			{
				this.DamagedCount = 0;
				this.Damaged.Clear();
			}
			if (this.ReadCustomVarAsIncreasePctByHit)
			{
				this.IncreasePctByHit = (float)this.Effect.Data.CustomVar / 255f;
			}
			this.OnPerkInitialized();
		}

		protected abstract void OnPerkInitialized();

		protected bool ApplyDamage(CombatObject damagedCombat, CombatObject hitOnlyOnceTarget, bool barrierHit)
		{
			return this.ApplyDamage(damagedCombat, hitOnlyOnceTarget, barrierHit, null, null, null);
		}

		protected bool ApplyDamage(CombatObject damagedCombat, CombatObject hitOnlyOnceTarget, bool barrierHit, ModifierData[] modifiers)
		{
			return this.ApplyDamage(damagedCombat, hitOnlyOnceTarget, barrierHit, null, null, modifiers);
		}

		protected bool ApplyDamage(CombatObject damagedCombat, CombatObject hitOnlyOnceTarget, bool barrierHit, Vector3 dir, Vector3 pos)
		{
			return this.ApplyDamage(damagedCombat, hitOnlyOnceTarget, barrierHit, new Vector3?(dir), new Vector3?(pos), null);
		}

		private bool ApplyDamage(CombatObject damagedCombat, CombatObject hitOnlyOnceTarget, bool barrierHit, Vector3? dir, Vector3? pos, ModifierData[] modifiers)
		{
			if (this.SingleTarget && this.DamagedCount > 0)
			{
				return false;
			}
			if (this.HitOnlyOnce && this.Damaged.Contains(hitOnlyOnceTarget.Id.ObjId))
			{
				return false;
			}
			if (GameHubBehaviour.Hub.GameTime.GetPlaybackTime() < this._perkInitializedTime + this.DelayToCheckHitMillis)
			{
				return false;
			}
			ModifierData[] datas = null;
			ModifierData[] array = modifiers ?? this.Modifiers;
			if (this.IncreasePctByHit != 0f)
			{
				float num = (float)this.DamagedCount * this.IncreasePctByHit;
				if (this.MaxPercentage != 0f)
				{
					num = Mathf.Min(this.MaxPercentage, num);
				}
				array = ModifierData.CreateConvoluted(array, this.StartingPercentage + num);
			}
			if (this.OwnerSpeedToAmount || this.TargetSpeedToAmount)
			{
				if (this.Effect == null)
				{
					BaseDamageablePerk.Log.ErrorFormat("Effect is null on ApplyDamage (DamagedCombat: {0})", new object[]
					{
						(!damagedCombat) ? string.Empty : damagedCombat.gameObject.name
					});
					return false;
				}
				if ((this.OwnerSpeedToAmount && this.Effect.Owner == null) || (this.TargetSpeedToAmount && this.Effect.Target == null))
				{
					BaseDamageablePerk.Log.ErrorFormat("Null Reference on ApplyDamage (Effect: {0}, Owner: {1}, Target: {2}, Gadget: {3}", new object[]
					{
						this.Effect.name,
						this.Effect.Owner,
						this.Effect.Target,
						(!(this.Effect.Gadget == null)) ? this.Effect.Gadget.name : string.Empty
					});
					return false;
				}
				Identifiable identifiable = (!this.OwnerSpeedToAmount) ? this.Effect.Target : this.Effect.Owner;
				CombatObject combat = CombatRef.GetCombat(identifiable.ObjId);
				if (combat == null)
				{
					BaseDamageablePerk.Log.ErrorFormat("combatRef is null on ApplyDamage ({0} {1}, Effect: {2}, Gadget: {3})", new object[]
					{
						(!this.OwnerSpeedToAmount) ? "Target:" : "Owner: ",
						identifiable.gameObject.name,
						this.Effect.name,
						this.Effect.Gadget.name
					});
					return false;
				}
				float time = combat.Movement.LastSpeed;
				if (this.RelativeSpeedToAmount && combat != damagedCombat)
				{
					time = (damagedCombat.Movement.LastVelocity - combat.Movement.LastVelocity).magnitude;
				}
				float baseAmount = this.speedToAmount.Evaluate(time);
				array = ModifierData.CreateConvoluted(array, baseAmount);
			}
			if (dir != null)
			{
				array.SetDirection(dir.Value);
			}
			if (pos != null)
			{
				array.SetPosition(pos.Value);
			}
			BaseDamageablePerk.UpdateCustomDirection(this.Effect, array, damagedCombat, this.CustomDirection);
			if (this.AdditionalTargetCombat)
			{
				datas = ModifierData.CopyData(array);
			}
			damagedCombat.Controller.AddModifiers(array, this.Effect.Gadget.Combat, this.Effect.EventId, barrierHit);
			if (this.AdditionalTargetCombat)
			{
				this.AdditionalTargetCombat.Controller.AddModifiers(datas, this.Effect.Gadget.Combat, this.Effect.EventId, barrierHit);
			}
			if (this.HitOnlyOnce || this.SingleTarget)
			{
				this.Damaged.Add(hitOnlyOnceTarget.Id.ObjId);
				this.DamagedCount++;
			}
			return true;
		}

		public virtual void OnDestroyEffect(DestroyEffect evt)
		{
			if (evt.RemoveData.WillCreateNextEvent && this.SaveSlot != DataHolderSlot.None && base.enabled)
			{
				PerkDataHolder<int>.PushData(evt.RemoveData.TargetEventId, this.SaveSlot, this.DamagedCount);
				if (this.HitOnlyOnceWithinSameSlot)
				{
					PerkDataHolder<List<int>>.PushData(evt.RemoveData.TargetEventId, this.SaveSlot, this.Damaged);
				}
			}
		}

		public static void UpdateCustomDirection(BaseFX effect, ModifierData[] mods, CombatObject damagedCombat, BaseDamageablePerk.ECustomDirection customDirection, Vector3 dir, Vector3 pos)
		{
			mods.SetDirection(dir);
			mods.SetPosition(pos);
			BaseDamageablePerk.UpdateCustomDirection(effect, mods, damagedCombat, customDirection);
		}

		private static void UpdateCustomDirection(BaseFX effect, ModifierData[] mods, CombatObject damagedCombat, BaseDamageablePerk.ECustomDirection customDirection)
		{
			switch (customDirection)
			{
			case BaseDamageablePerk.ECustomDirection.None:
				break;
			case BaseDamageablePerk.ECustomDirection.SplitLeftRight:
			{
				Plane plane = new Plane(effect.transform.right, effect.transform.position);
				if (plane.GetSide(damagedCombat.transform.position))
				{
					mods.SetDirection(effect.transform.right);
				}
				else
				{
					mods.SetDirection(-effect.transform.right);
				}
				break;
			}
			case BaseDamageablePerk.ECustomDirection.Forward:
				mods.SetDirection(effect.transform.forward);
				break;
			case BaseDamageablePerk.ECustomDirection.Backward:
				mods.SetDirection(-effect.transform.forward);
				break;
			case BaseDamageablePerk.ECustomDirection.EffectEventDirection:
				mods.SetDirection(effect.Data.Direction);
				break;
			case BaseDamageablePerk.ECustomDirection.OwnerForward:
				mods.SetDirection(effect.Owner.transform.forward);
				break;
			default:
				BaseDamageablePerk.Log.ErrorFormat("Trying to use invalid CustomDirection! Effect:{0}", new object[]
				{
					effect.name
				});
				break;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BaseDamageablePerk));

		public DataHolderSlot SaveSlot;

		public BasePerk.PerkTarget Target;

		[Header("This perk uses ApplyDamage?")]
		public BasePerk.PerkTarget AdditionalTargetOnApplyDamage = BasePerk.PerkTarget.None;

		public bool SingleTarget;

		public bool HitOnlyOnce;

		public bool HitOnlyOnceWithinSameSlot = true;

		[Header("If read from customVar, will only work between 0.0 and 1.0")]
		public bool ReadCustomVarAsIncreasePctByHit;

		public float IncreasePctByHit;

		public float StartingPercentage = 1f;

		public float MaxPercentage;

		public BasePerk.DamageSource Source = BasePerk.DamageSource.EventModifiers;

		public BaseDamageablePerk.ECustomDirection CustomDirection;

		public bool OwnerSpeedToAmount;

		public bool TargetSpeedToAmount;

		public bool RelativeSpeedToAmount;

		public AnimationCurve speedToAmount;

		protected int _perkInitializedTime;

		public int DelayToCheckHitMillis;

		protected List<int> Damaged = new List<int>(5);

		protected int DamagedCount;

		protected ModifierData[] Modifiers;

		protected CombatObject TargetCombat;

		protected CombatObject AdditionalTargetCombat;

		public enum ECustomDirection
		{
			None,
			SplitLeftRight,
			Forward,
			Backward,
			EffectEventDirection,
			OwnerForward
		}
	}
}
