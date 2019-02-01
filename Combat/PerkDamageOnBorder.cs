using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageOnBorder : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		private Vector3 MyPosition
		{
			get
			{
				return this._myTransform.position;
			}
		}

		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._modifiers = ((!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers);
			this._updater = new TimedUpdater((int)this.TickMillis, false, false);
			this._myTransform = base.transform;
			this.isSleeping = false;
		}

		private void FixedUpdate()
		{
			if (this.isSleeping)
			{
				return;
			}
			if (this._updater.ShouldHalt())
			{
				return;
			}
			List<Collider> list = new List<Collider>(Physics.OverlapSphere(this.MyPosition, this.OuterRange, 1077058560));
			for (int i = 0; i < list.Count; i++)
			{
				if (!this._affectedColliders.Contains(list[i]))
				{
					this.EnterCollider(list[i]);
				}
			}
			for (int j = 0; j < this._affectedColliders.Count; j++)
			{
				if (this._affectedColliders[j] && !list.Contains(this._affectedColliders[j]))
				{
					this.ExitCollider(this._affectedColliders[j]);
				}
			}
			this._affectedColliders = list;
			this.CheckBorder();
		}

		private void EnterCollider(Collider other)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			if (!combat || !this.Effect.CheckHit(combat))
			{
				return;
			}
			this._affectedCombats.Add(new PerkDamageOnBorder.AffectedCombat
			{
				Combat = combat,
				Collider = other
			});
			combat.ListenToObjectUnspawn += this.OnObjectDeath;
		}

		private void ExitCollider(Collider other)
		{
			CombatObject combatObject = CombatRef.GetCombat(other);
			if (!combatObject || !this.Effect.CheckHit(combatObject))
			{
				return;
			}
			PerkDamageOnBorder.AffectedCombat item = this._affectedCombats.Find((PerkDamageOnBorder.AffectedCombat x) => x.Combat == combatObject);
			this._affectedCombats.Remove(item);
			combatObject.ListenToObjectUnspawn -= this.OnObjectDeath;
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			for (int i = 0; i < this._affectedCombats.Count; i++)
			{
				this._affectedCombats[i].Combat.ListenToObjectUnspawn -= this.OnObjectDeath;
			}
			this._affectedCombats.Clear();
			this._affectedColliders.Clear();
			this.isSleeping = true;
		}

		private void OnObjectDeath(CombatObject obj, UnspawnEvent msg)
		{
			PerkDamageOnBorder.AffectedCombat affectedCombat = this._affectedCombats.Find((PerkDamageOnBorder.AffectedCombat x) => x.Combat == obj);
			this._affectedCombats.Remove(affectedCombat);
			affectedCombat.Combat.ListenToObjectUnspawn -= this.OnObjectDeath;
		}

		private void CheckBorder()
		{
			this.testVar = false;
			for (int i = 0; i < this._affectedCombats.Count; i++)
			{
				PerkDamageOnBorder.AffectedCombat affectedCombat = this._affectedCombats[i];
				float num = Vector3.SqrMagnitude(this.MyPosition - affectedCombat.Collider.bounds.min);
				float num2 = Vector3.SqrMagnitude(this.MyPosition - affectedCombat.Collider.bounds.max);
				float num3 = this.InnerRange * this.InnerRange;
				bool flag = num >= num3 || num2 >= num3;
				if (flag)
				{
					this.testVar = true;
					affectedCombat.Combat.Controller.AddModifiers(this._modifiers, this.Effect.Data.SourceGadget.Combat, this.Effect.EventId, false);
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDamageOnBorder));

		public long TickMillis;

		public bool UseExtraModifiers;

		public float InnerRange;

		public float OuterRange;

		private TimedUpdater _updater;

		private ModifierData[] _modifiers;

		private List<PerkDamageOnBorder.AffectedCombat> _affectedCombats = new List<PerkDamageOnBorder.AffectedCombat>();

		private List<Collider> _affectedColliders = new List<Collider>();

		private Transform _myTransform;

		private bool isSleeping;

		public bool testVar;

		[Serializable]
		public class AffectedCombat
		{
			public CombatObject Combat;

			public Collider Collider;
		}
	}
}
