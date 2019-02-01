using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkInstantBonusArea : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._combatObjects.Clear();
			this._combatObjectsNew.Clear();
			this._myTransform = base.transform;
			this._regenUpdater = new TimedUpdater((int)(this.Tick * 1000f), false, false);
			this._modifiers = ((!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers);
			if (this.Radius == 0f)
			{
				this.Radius = this.Effect.Data.Range;
			}
		}

		private void FixedUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (!this._regenUpdater.ShouldHalt())
			{
				this.ApplyBonus();
			}
		}

		private void ApplyBonus()
		{
			Collider[] array = Physics.OverlapSphere(this._myTransform.position, this.Radius, 1077058560);
			this._combatObjectsNew.Clear();
			foreach (Collider comp in array)
			{
				CombatObject combat = CombatRef.GetCombat(comp);
				if (!this._combatObjectsNew.Contains(combat))
				{
					bool flag = this.Effect.CheckHit(combat);
					if (flag && combat && combat.Controller)
					{
						this._combatObjectsNew.Add(combat);
					}
				}
			}
			for (int j = 0; j < this._combatObjectsNew.Count; j++)
			{
				CombatObject combatObject = this._combatObjectsNew[j];
				if (!this._combatObjects.Contains(combatObject))
				{
					combatObject.Controller.AddPassiveModifiers(this._modifiers, this.Effect.Gadget.Combat, this.Effect.EventId);
					this._combatObjects.Add(combatObject);
				}
			}
			for (int k = 0; k < this._combatObjects.Count; k++)
			{
				if (!this._combatObjectsNew.Contains(this._combatObjects[k]))
				{
					this._combatObjects[k].Controller.RemovePassiveModifiers(this._modifiers, this.Effect.Gadget.Combat, this.Effect.EventId);
					this._combatObjects.Remove(this._combatObjects[k]);
					k--;
				}
			}
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			for (int i = 0; i < this._combatObjects.Count; i++)
			{
				this._combatObjects[i].Controller.RemovePassiveModifiers(this._modifiers, this.Effect.Gadget.Combat, this.Effect.EventId);
			}
			this._combatObjects.Clear();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkInstantBonusArea));

		public float Tick;

		public bool UseExtraModifiers;

		public float Radius;

		private readonly List<CombatObject> _combatObjects = new List<CombatObject>();

		private readonly List<CombatObject> _combatObjectsNew = new List<CombatObject>();

		private Transform _myTransform;

		private TimedUpdater _regenUpdater;

		private ModifierData[] _modifiers;
	}
}
