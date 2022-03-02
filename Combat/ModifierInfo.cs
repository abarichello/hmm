using System;
using HeavyMetalMachines.Combat.Modifier;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class ModifierInfo
	{
		public static bool operator ==(ModifierInfo one, ModifierInfo other)
		{
			if (object.ReferenceEquals(one, null))
			{
				return object.ReferenceEquals(other, null);
			}
			return !object.ReferenceEquals(other, null) && one.Equals(other);
		}

		public static bool operator !=(ModifierInfo one, ModifierInfo other)
		{
			return !(one == other);
		}

		protected bool Equals(ModifierInfo other)
		{
			return this.Attribute == other.Attribute && this.Effect == other.Effect && this.Status == other.Status && this.TargetGadget == other.TargetGadget && this.IsPercent.Equals(other.IsPercent) && this.Unstable.Equals(other.Unstable) && this.Tapered == other.Tapered && this.FriendlyFire.Equals(other.FriendlyFire) && this.NotForEnemies.Equals(other.NotForEnemies) && this.NotForWards.Equals(other.NotForWards) && this.NotForBuildings.Equals(other.NotForBuildings) && this.NotFurTurrets.Equals(other.NotFurTurrets) && this.NotForPlayers.Equals(other.NotForPlayers) && this.IsReactive.Equals(other.IsReactive) && this.UsePower.Equals(other.UsePower) && this.IsPurgeable.Equals(other.IsPurgeable) && this.IsDispellable.Equals(other.IsDispellable) && this.NotConvoluted.Equals(other.NotConvoluted) && object.Equals(this.Feedback, other.Feedback) && (this.Attribute != AttributeBuffKind.SupressTargetTag || object.Equals(this.TargetTag, other.TargetTag));
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (obj.GetType() == base.GetType() && this.Equals((ModifierInfo)obj)));
		}

		public override int GetHashCode()
		{
			int num = (int)this.Attribute;
			num = (num * 397 ^ (int)this.Effect);
			num = (num * 397 ^ (int)this.Status);
			num = (num * 397 ^ (int)this.TargetGadget);
			num = (num * 397 ^ this.IsPercent.GetHashCode());
			num = (num * 397 ^ this.Unstable.GetHashCode());
			num = (num * 397 ^ (int)this.Tapered);
			num = (num * 397 ^ this.FriendlyFire.GetHashCode());
			num = (num * 397 ^ this.NotForEnemies.GetHashCode());
			num = (num * 397 ^ this.NotForWards.GetHashCode());
			num = (num * 397 ^ this.NotForBuildings.GetHashCode());
			num = (num * 397 ^ this.NotFurTurrets.GetHashCode());
			num = (num * 397 ^ this.NotForPlayers.GetHashCode());
			num = (num * 397 ^ this.IsReactive.GetHashCode());
			num = (num * 397 ^ this.UsePower.GetHashCode());
			num = (num * 397 ^ this.IsPurgeable.GetHashCode());
			num = (num * 397 ^ this.IsDispellable.GetHashCode());
			num = (num * 397 ^ this.NotConvoluted.GetHashCode());
			return num * 397 ^ ((!(this.Feedback != null)) ? 0 : this.Feedback.GetHashCode());
		}

		public BaseModifier NewModifier;

		public AttributeBuffKind Attribute;

		public EffectKind Effect;

		public StatusKind Status;

		public TargetGadget TargetGadget;

		[Tooltip("LifeTime will cap the duration of modifier.")]
		public float LifeTime;

		[Tooltip("Configure delay between the ticks.")]
		public float TickDelta;

		[Tooltip("If 'AmounPerSecond' is marked, amount will be spread over lifeTime .")]
		public float Amount;

		public string AmountUpgrade;

		public string LifeTimeUpgrade;

		public string TickDeltaUpgrade;

		public string StatusUpgrade;

		[Tooltip("Mark this if the desire is apply damage per second")]
		public bool AmountPerSecond;

		public bool IsPercent;

		public bool ForceShowAsPercent;

		public bool Unstable;

		public TaperMode Tapered;

		public bool HitOwner;

		public bool FriendlyFire;

		public bool NotForEnemies;

		public bool HitBomb;

		public bool NotForWards;

		public bool NotForBuildings;

		public bool NotFurTurrets;

		public bool NotForPlayers;

		public bool HitBanished;

		public bool IsReactive;

		public bool UsePower;

		public bool IsPurgeable;

		public bool IsDispellable;

		public bool NotConvoluted;

		public bool IsDebuff;

		public bool IgnoreBarrier;

		public float Delay;

		public string Tag;

		public string TargetTag;

		public ModifierFeedbackInfo Feedback;

		public bool UseAmountCurveMultiplier;

		public AnimationCurve AmountCurveMultiplier = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		[HideInInspector]
		[NonSerialized]
		public CombatObject LifeStealProvider;
	}
}
