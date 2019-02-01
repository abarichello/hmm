using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkInstantDamage : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			ModifierData[] modifiers = base.GetModifiers(this.Source);
			Transform targetTransform = base.GetTargetTransform(this.Effect, this.Origin);
			CombatObject targetCombat = base.GetTargetCombat(this.Effect, this.Target);
			CombatObject targetCombat2 = base.GetTargetCombat(this.Effect, this.TargetToApplyDamage);
			if (!targetCombat2)
			{
				return;
			}
			ModifierData[] array = modifiers;
			if (this.DamageByRange && targetCombat)
			{
				float baseAmount = this.DamageToRange.Evaluate(Vector3.Distance(targetCombat.Transform.position, targetTransform.position));
				array = ModifierData.CreateConvoluted(modifiers, baseAmount);
			}
			Vector3 dir = targetCombat.transform.position - targetTransform.transform.position;
			BaseDamageablePerk.UpdateCustomDirection(this.Effect, array, targetCombat, this.CustomDirection, dir, targetTransform.transform.position);
			if (this.SetTargetToOwnerModDirection && targetCombat)
			{
				Vector3 normalized = (targetCombat.Transform.position - targetTransform.position).normalized;
				array.SetDirection(normalized);
			}
			targetCombat2.Controller.AddModifiers(array, this.Effect.Gadget.Combat, this.Effect.EventId, false);
		}

		public BasePerk.PerkTarget Origin;

		public BasePerk.PerkTarget Target;

		public BasePerk.PerkTarget TargetToApplyDamage;

		public bool SetTargetToOwnerModDirection;

		public bool DamageByRange;

		public AnimationCurve DamageToRange;

		public BasePerk.DamageSource Source;

		public BaseDamageablePerk.ECustomDirection CustomDirection;
	}
}
