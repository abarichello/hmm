using System;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkModifyDamageAmountOnDamageEvent : PerkApplyModifiersOnDamageEvent
	{
		protected override void OnRefEvent(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventid)
		{
			if (!this.ShouldApplyModifier(mod))
			{
				return;
			}
			if (!this.ValidTargetLayerCondition(mod))
			{
				return;
			}
			if (!this.ItsAValidAttackKindCondition(mod))
			{
				return;
			}
			this.ChangeModifierAmount(ref amount);
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			int causer2 = (!causer) ? -1 : causer.Id.ObjId;
			taker.Feedback.Add(this.FeedbackInfo, eventid, causer2, playbackTime, (int)((float)playbackTime + this.FeedbackInfo.LifeTime * 1000f), 0, this.Effect.Gadget.Slot);
		}

		protected override void OnEvent(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			if (!this.ShouldApplyModifier(mod))
			{
				return;
			}
			if (!this.ValidTargetLayerCondition(mod))
			{
				return;
			}
			if (!this.ItsAValidAttackKindCondition(mod))
			{
				return;
			}
			base.ApplyDamage(this.TargetCombat, this.TargetCombat, false);
		}

		protected void ChangeModifierAmount(ref float amount)
		{
			amount *= this.DamageAmountModificationPct;
		}

		protected virtual bool ValidTargetLayerCondition(ModifierData mod)
		{
			return mod.DirectionSet && mod.PositionSet;
		}

		protected virtual bool ItsAValidAttackKindCondition(ModifierData mod)
		{
			PerkModifyDamageAmountOnDamageEvent.AttackKind targetAttackKind = this.TargetAttackKind;
			if (targetAttackKind != PerkModifyDamageAmountOnDamageEvent.AttackKind.FrontalAttack)
			{
				if (targetAttackKind != PerkModifyDamageAmountOnDamageEvent.AttackKind.None)
				{
				}
				return true;
			}
			Transform trans = base._trans;
			Vector3 position = mod.Position;
			return PhysicsUtils.IsInFront(trans.position, trans.forward, position) && PhysicsUtils.IsFacing(mod.Direction, trans.forward);
		}

		[Header("Modify Damage configuration")]
		public bool CheckLayer;

		public LayerManager.Layer TargetLayer;

		public PerkModifyDamageAmountOnDamageEvent.AttackKind TargetAttackKind;

		public float DamageAmountModificationPct;

		public ModifierFeedbackInfo FeedbackInfo;

		public enum AttackKind
		{
			None,
			FrontalAttack
		}
	}
}
