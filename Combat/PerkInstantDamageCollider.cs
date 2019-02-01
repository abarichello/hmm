using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	internal class PerkInstantDamageCollider : BasePerk
	{
		protected override void Awake()
		{
			base.Awake();
			this._rigidbody = base.GetComponent<Rigidbody>();
		}

		public override void PerkInitialized()
		{
			PerkInstantDamageCollider.<PerkInitialized>c__AnonStorey0 <PerkInitialized>c__AnonStorey = new PerkInstantDamageCollider.<PerkInitialized>c__AnonStorey0();
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._rigidbody.position += PerkInstantDamageCollider.Translation;
			<PerkInitialized>c__AnonStorey.hits = this._rigidbody.SweepTestAll(Vector3.down, 100f, QueryTriggerInteraction.Collide);
			PerkInstantDamageCollider.CombatHits.Clear();
			int hitIndex;
			for (hitIndex = 0; hitIndex < <PerkInitialized>c__AnonStorey.hits.Length; hitIndex++)
			{
				CombatObject combat = CombatRef.GetCombat(<PerkInitialized>c__AnonStorey.hits[hitIndex].collider);
				bool barrier = BarrierUtils.IsBarrier(<PerkInitialized>c__AnonStorey.hits[hitIndex].collider);
				if (combat != null && combat.Controller && this.Effect.CheckHit(combat) && PerkInstantDamageCollider.CombatHits.FindIndex((BarrierUtils.CombatHit x) => x.Col == <PerkInitialized>c__AnonStorey.hits[hitIndex].collider) < 0)
				{
					PerkInstantDamageCollider.CombatHits.Add(new BarrierUtils.CombatHit
					{
						Combat = combat,
						Col = <PerkInitialized>c__AnonStorey.hits[hitIndex].collider,
						Barrier = barrier
					});
				}
			}
			BarrierUtils.FilterByRaycastFromPoint(base._trans.position, PerkInstantDamageCollider.CombatHits);
			ModifierData[] datas = (!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers;
			Vector3 origin = this.Effect.Data.Origin;
			for (int i = 0; i < PerkInstantDamageCollider.CombatHits.Count; i++)
			{
				CombatObject combat2 = PerkInstantDamageCollider.CombatHits[i].Combat;
				Vector3 normalized = (combat2.Transform.position - origin).normalized;
				combat2.Controller.AddModifiers(datas, this.Effect.Gadget.Combat, this.Effect.EventId, normalized, origin, PerkInstantDamageCollider.CombatHits[i].Barrier);
			}
		}

		public bool UseExtraModifiers;

		private Rigidbody _rigidbody;

		private const float TranslationDistance = 100f;

		private static readonly Vector3 Translation = new Vector3(0f, 100f, 0f);

		private static readonly List<BarrierUtils.CombatHit> CombatHits = new List<BarrierUtils.CombatHit>();
	}
}
