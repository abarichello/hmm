using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageAreaOnLifetime : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._damaged = false;
			this._deathTime = this.Effect.Data.DeathTime;
		}

		private void FixedUpdate()
		{
			long num = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (num < this._deathTime)
			{
				return;
			}
			if (!this._damaged)
			{
				this._damaged = true;
				this.DamageArea();
			}
		}

		private void DamageArea()
		{
			Vector3 vector;
			if (this.UseTargetPosition)
			{
				vector = this.Effect.Data.Target;
			}
			else
			{
				vector = base.transform.position;
			}
			Collider[] array = Physics.OverlapSphere(vector, this.Effect.Data.Range, 1077058560);
			ModifierData[] datas = (!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers;
			foreach (Collider comp in array)
			{
				CombatObject combat = CombatRef.GetCombat(comp);
				if (!PerkDamageAreaOnLifetime._objects.Contains(combat))
				{
					PerkDamageAreaOnLifetime._objects.Add(combat);
					bool flag = this.Effect.CheckHit(combat);
					if (flag && combat && combat.Controller)
					{
						Vector3 normalized = (combat.Transform.position - vector).normalized;
						combat.Controller.AddModifiers(datas, this.Effect.Gadget.Combat, this.Effect.EventId, normalized, vector, false);
					}
				}
			}
			PerkDamageAreaOnLifetime._objects.Clear();
		}

		public bool UseTargetPosition;

		public bool UseExtraModifiers;

		private long _deathTime;

		private bool _damaged;

		private static readonly HashSet<CombatObject> _objects = new HashSet<CombatObject>();
	}
}
