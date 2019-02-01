using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(Collider))]
	public class PerkApplyModifiersOnHitColliderCast : BaseDamageablePerk
	{
		protected override void OnPerkInitialized()
		{
			this._hits = base.GetComponent<Rigidbody>().SweepTestAll(this.Effect.Data.Direction, this.Effect.Data.Range);
			float num = float.PositiveInfinity;
			if (this.StopOnSceneryHit)
			{
				for (int i = 0; i < this._hits.Length; i++)
				{
					if (this._hits[i].distance < num && this._hits[i].collider.gameObject.layer == 9)
					{
						num = this._hits[i].distance;
					}
				}
			}
			for (int j = 0; j < this._hits.Length; j++)
			{
				if (this._hits[j].distance <= num && this._hits[j].collider.gameObject.layer != 9)
				{
					CombatObject combat = CombatRef.GetCombat(this._hits[j].collider);
					if (this.Effect.CheckHit(combat))
					{
						base.ApplyDamage(combat, combat, false, this.Effect.Data.Direction, this.Effect.transform.position);
					}
				}
			}
		}

		[Header("Collider Cast")]
		public bool StopOnSceneryHit;

		private RaycastHit[] _hits;
	}
}
