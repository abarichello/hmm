using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkCallbackAreaOnLifeTime : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._deathTime = this.Effect.Data.DeathTime;
			this._myTransform = base.transform;
			this._checked = false;
		}

		private void FixedUpdate()
		{
			long num = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (num < this._deathTime)
			{
				return;
			}
			if (!this._checked)
			{
				this._checked = true;
				this.CheckArea();
			}
		}

		private void CheckArea()
		{
			Vector3 vector;
			if (this.UseTargetPosition)
			{
				vector = this.Effect.Data.Target;
			}
			else
			{
				vector = this._myTransform.position;
			}
			Collider[] array = Physics.OverlapSphere(vector, this.Effect.Data.Range, 1077054464);
			List<CombatObject> list = new List<CombatObject>();
			foreach (Collider comp in array)
			{
				CombatObject combat = CombatRef.GetCombat(comp);
				if (!list.Contains(combat))
				{
					bool flag = this.Effect.CheckHit(combat);
					if (flag && combat && combat.Controller)
					{
						list.Add(combat);
					}
				}
			}
			if (this.MaxHits == 0)
			{
				Mural.Post(new DamageAreaCallback(list, vector, this.Effect, this.TargetGadgetCallback), this.Effect.Gadget);
			}
			else
			{
				List<PerkCallbackAreaOnLifeTime.AreaHit> list2 = new List<PerkCallbackAreaOnLifeTime.AreaHit>();
				for (int j = 0; j < list.Count; j++)
				{
					CombatObject combatObject = list[j];
					list2.Add(new PerkCallbackAreaOnLifeTime.AreaHit
					{
						Combat = combatObject,
						SqrDistance = (vector - combatObject.Transform.position).sqrMagnitude
					});
				}
				list2.Sort((PerkCallbackAreaOnLifeTime.AreaHit p1, PerkCallbackAreaOnLifeTime.AreaHit p2) => (int)(p1.SqrDistance - p2.SqrDistance));
				list.Clear();
				for (int k = 0; k < this.MaxHits; k++)
				{
					list.Add(list2[k].Combat);
				}
				Mural.Post(new DamageAreaCallback(list, vector, this.Effect, this.TargetGadgetCallback), this.Effect.Gadget);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkCallbackAreaOnLifeTime));

		public bool UseTargetPosition;

		public int MaxHits;

		public GadgetSlot TargetGadgetCallback;

		private long _deathTime;

		private bool _checked;

		private Transform _myTransform;

		private class AreaHit
		{
			public CombatObject Combat;

			public float SqrDistance;
		}
	}
}
