using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkCallbackAreaOnEPConsumption : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._timedUpdater = new TimedUpdater(this.TimeMillis, false, false);
			this._transform = base.transform;
		}

		private void FixedUpdate()
		{
			if (this._timedUpdater.ShouldHalt())
			{
				return;
			}
			this.CheckArea();
		}

		public override void PerkDestroyed(DestroyEffectMessage destroyEffectMessage)
		{
			base.PerkDestroyed(destroyEffectMessage);
			for (int i = 0; i < this._combats.Count; i++)
			{
				this._combats[i].Controller.ListenToEP -= this.OnEPSpent;
			}
			this._combats.Clear();
			this._combatsNew.Clear();
		}

		private void CheckArea()
		{
			Collider[] array = Physics.OverlapSphere(this._transform.position, this.Effect.Data.Range, 1077054464);
			this._combatsNew.Clear();
			foreach (Collider comp in array)
			{
				CombatObject combat = CombatRef.GetCombat(comp);
				if (!this._combatsNew.Contains(combat))
				{
					bool flag = this.Effect.CheckHit(combat);
					if (flag && combat && combat.Controller)
					{
						this._combatsNew.Add(combat);
					}
				}
			}
			for (int j = 0; j < this._combatsNew.Count; j++)
			{
				CombatObject combatObject = this._combatsNew[j];
				if (!this._combats.Contains(combatObject))
				{
					combatObject.Controller.ListenToEP += this.OnEPSpent;
					this._combats.Add(combatObject);
				}
			}
			for (int k = 0; k < this._combats.Count; k++)
			{
				if (!this._combatsNew.Contains(this._combats[k]))
				{
					this._combats[k].Controller.ListenToEP -= this.OnEPSpent;
					this._combats.Remove(this._combats[k]);
					k--;
				}
			}
		}

		private void OnEPSpent(CombatObject combat, float amount)
		{
			Mural.Post(new EPComsuptionCallback(combat, (int)amount), this.Effect.Gadget);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkCallbackAreaOnEPConsumption));

		public int TimeMillis;

		private TimedUpdater _timedUpdater;

		private Transform _transform;

		private readonly List<CombatObject> _combats = new List<CombatObject>();

		private readonly List<CombatObject> _combatsNew = new List<CombatObject>();
	}
}
