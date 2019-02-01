using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageLineOnTick : PerkDamageOnTick
	{
		protected override void DealDamage()
		{
			Vector3 position = this._target.transform.position;
			Vector3 originPosition = base.OriginPosition;
			originPosition.y = (position.y = 1f);
			Vector3 vector = position - originPosition;
			float magnitude = vector.magnitude;
			Vector3 normalized = vector.normalized;
			this.Effect.Gadget.GetHittingCombatsInLine(originPosition, this.LineWidth, normalized, magnitude, 1077058560, this.Effect.Data.EffectInfo, ref this.combatObjects);
			for (int i = 0; i < this.combatObjects.Count; i++)
			{
				CombatObject combatObject = this.combatObjects[i];
				combatObject.Controller.AddModifiers(this._modifiers, this.Effect.Gadget.Combat, this.Effect.EventId, normalized, originPosition, false);
			}
		}

		[Header("Damage by range won't work in this perk!!!")]
		public float LineWidth;

		private List<CombatObject> combatObjects = new List<CombatObject>(20);
	}
}
