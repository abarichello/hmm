using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkLinkDamageOnCross : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._owner = this.Effect.Owner.transform;
			this._target = ((!this.Effect.Target) ? null : this.Effect.Target.transform);
			if (!this._target)
			{
				base.enabled = false;
				return;
			}
			this._ownerCombat = CombatRef.GetCombat(this._owner);
			this._targetCombat = CombatRef.GetCombat(this._target);
			this._modifiers = ((!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers);
			this.Cleanup();
			this.isSleeping = false;
		}

		private void Cleanup()
		{
			this._inside.Clear();
			this._newInside.Clear();
			this._listening.Clear();
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
			this.CalcLinkSphere();
			Collider[] collection = Physics.OverlapSphere(this._center, this._range, 1077058560);
			this._newInside.Clear();
			this._newInside.AddRange(collection);
			for (int i = 0; i < this._newInside.Count; i++)
			{
				if (!this._inside.Contains(this._newInside[i]))
				{
					this.Enter(this._newInside[i]);
				}
			}
			for (int j = 0; j < this._inside.Count; j++)
			{
				if (this._inside[j] && !this._newInside.Contains(this._inside[j]))
				{
					this.Exit(this._inside[j]);
				}
			}
			List<Collider> inside = this._inside;
			this._inside = this._newInside;
			this._newInside = inside;
			Plane plane = new Plane(this._owner.position, this._target.position, this._center + Vector3.up * 10f);
			this.CheckSides(plane);
		}

		private void CalcLinkSphere()
		{
			this._center = (this._owner.position + this._target.position) * 0.5f;
			this._range = Vector3.Distance(this._owner.position, this._target.position) * 0.5f;
		}

		private void Enter(Collider other)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			if (!combat || combat == this._ownerCombat || combat == this._targetCombat || !combat.Controller || !this.Effect.CheckHit(combat))
			{
				return;
			}
			this._listening.Add(new PerkLinkDamageOnCross.LinkedCombat
			{
				Combat = combat.Controller,
				PlaneSide = 0,
				Transf = combat.transform,
				Col = other
			});
			combat.ListenToObjectUnspawn += this.OnObjectDeath;
		}

		private void Exit(Collider other)
		{
			CombatObject combatObject = CombatRef.GetCombat(other);
			if (!combatObject || !combatObject.Controller)
			{
				return;
			}
			PerkLinkDamageOnCross.LinkedCombat linkedCombat = this._listening.Find((PerkLinkDamageOnCross.LinkedCombat x) => x.Combat == combatObject.Controller);
			if (linkedCombat != null)
			{
				this._listening.Remove(linkedCombat);
				combatObject.ListenToObjectUnspawn -= this.OnObjectDeath;
			}
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			for (int i = 0; i < this._listening.Count; i++)
			{
				CombatController combat = this._listening[i].Combat;
				if (combat)
				{
					combat.Combat.ListenToObjectUnspawn -= this.OnObjectDeath;
				}
			}
			this._listening.Clear();
			this._inside.Clear();
			this._newInside.Clear();
			this.isSleeping = true;
		}

		private void OnObjectDeath(CombatObject obj, UnspawnEvent msg)
		{
			PerkLinkDamageOnCross.LinkedCombat linkedCombat = this._listening.Find((PerkLinkDamageOnCross.LinkedCombat x) => x.Combat == obj.Controller);
			if (linkedCombat != null)
			{
				this._listening.Remove(linkedCombat);
				linkedCombat.Combat.Combat.ListenToObjectUnspawn -= this.OnObjectDeath;
			}
		}

		private void CheckSides(Plane plane)
		{
			for (int i = 0; i < this._listening.Count; i++)
			{
				PerkLinkDamageOnCross.LinkedCombat linkedCombat = this._listening[i];
				int planeSide = linkedCombat.PlaneSide;
				linkedCombat.PlaneSide = linkedCombat.Col.PlaneCast(plane);
				if (linkedCombat.PlaneSide == 0)
				{
					linkedCombat.PlaneSide = 2;
				}
				if (planeSide == 0)
				{
					planeSide = linkedCombat.PlaneSide;
				}
				if (linkedCombat.PlaneSide == 2 || linkedCombat.PlaneSide != planeSide)
				{
					linkedCombat.Combat.AddModifiers(this._modifiers, this.Effect.Data.SourceGadget.Combat, this.Effect.EventId, false);
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkLinkDamageOnCross));

		public bool UseExtraModifiers;

		private List<PerkLinkDamageOnCross.LinkedCombat> _listening = new List<PerkLinkDamageOnCross.LinkedCombat>();

		private List<Collider> _inside = new List<Collider>();

		private List<Collider> _newInside = new List<Collider>();

		private ModifierData[] _modifiers;

		private Transform _owner;

		private Transform _target;

		private CombatObject _ownerCombat;

		private CombatObject _targetCombat;

		private bool isSleeping;

		private Vector3 _center;

		private float _range;

		private TimedUpdater _updater = new TimedUpdater(100, false, false);

		[Serializable]
		public class LinkedCombat
		{
			public int PlaneSide;

			public CombatController Combat;

			public Transform Transf;

			public Collider Col;
		}
	}
}
