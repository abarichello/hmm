using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BaseDamageableAreaPerk : BaseDamageablePerk
	{
		protected override void OnPerkInitialized()
		{
		}

		protected virtual void GetHits(Vector3 position, ref List<BarrierUtils.CombatHit> cpoCombatObjects)
		{
			float radius = (this.Radius != 0f) ? this.Radius : this.Effect.Gadget.Radius;
			cpoCombatObjects.Clear();
			BarrierUtils.OverlapSphereRaycastFromCenter(position, radius, 1077054464, cpoCombatObjects);
		}

		protected virtual void DamageArea(Vector3 position)
		{
			this.GetHits(position, ref this.HittingCombatObjects);
			for (int i = 0; i < this.HittingCombatObjects.Count; i++)
			{
				BarrierUtils.CombatHit combatHit = this.HittingCombatObjects[i];
				CombatObject combat = combatHit.Combat;
				if (!this.DamagedPlayers.Contains(combat) && combat && combat.Controller)
				{
					if (this.Effect.CheckHit(combat))
					{
						Vector3 normalized = (combat.Transform.position - position).normalized;
						base.ApplyDamage(combat, combat, combatHit.Barrier, normalized, base.transform.position);
						this.DamagedPlayers.Add(combat);
					}
				}
			}
			if (!this.IsDamageCallbackEnabled)
			{
				return;
			}
			Mural.Post(new DamageAreaCallback(this.DamagedPlayers, position, this.Effect, this.TargetGadgetCallback), this.Effect.Gadget);
			this.DamagedPlayers.Clear();
		}

		public float Radius;

		public bool IsDamageCallbackEnabled;

		public GadgetSlot TargetGadgetCallback;

		protected List<BarrierUtils.CombatHit> HittingCombatObjects = new List<BarrierUtils.CombatHit>(20);

		protected readonly List<CombatObject> DamagedPlayers = new List<CombatObject>(20);
	}
}
