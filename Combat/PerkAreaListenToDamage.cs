using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkAreaListenToDamage : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._trans = base.transform;
			this.isSleeping = false;
		}

		private void Update()
		{
			if (this.isSleeping || GameHubBehaviour.Hub.Net.IsClient() || this._updater.ShouldHalt())
			{
				return;
			}
			Collider[] collection = Physics.OverlapSphere(this._trans.position, this.Effect.Data.Range, 1077054464);
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
		}

		private void Enter(Collider other)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			if (!combat || !combat.Controller || !this.Effect.CheckHit(combat) || this._listening.Contains(combat))
			{
				return;
			}
			this._listening.Add(combat);
			this.AddListener(combat);
		}

		private void Exit(Collider other)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			if (!combat || !combat.Controller || !this._listening.Contains(combat))
			{
				return;
			}
			this._listening.Remove(combat);
			this.RemoveListener(combat);
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			for (int i = 0; i < this._listening.Count; i++)
			{
				if (this._listening[i])
				{
					this.RemoveListener(this._listening[i]);
				}
			}
			this._listening.Clear();
			this._inside.Clear();
			this._newInside.Clear();
			this.isSleeping = true;
		}

		private void AddListener(CombatObject obj)
		{
			obj.ListenToPosDamageTaken += this.OnDamageTaken;
			if (!this.KeepListeningOnDeath)
			{
				obj.ListenToObjectUnspawn += this.OnObjectDeath;
			}
		}

		private void RemoveListener(CombatObject obj)
		{
			obj.ListenToPosDamageTaken -= this.OnDamageTaken;
			if (!this.KeepListeningOnDeath)
			{
				obj.ListenToObjectUnspawn -= this.OnObjectDeath;
			}
		}

		private void OnObjectDeath(CombatObject obj, UnspawnEvent msg)
		{
			this._listening.Remove(obj);
			this.RemoveListener(obj);
		}

		private void OnDamageTaken(CombatObject causer, CombatObject taker, ModifierData data, float amount, int eventId)
		{
			if (eventId == this.Effect.EventId)
			{
				return;
			}
			if (this.IgnoreReactiveModifiers && data.IsReactive)
			{
				return;
			}
			if (!Array.Exists<EffectKind>(this.DamageKinds, (EffectKind x) => data.Info.Effect == x))
			{
				return;
			}
			if (this.BasicAttackOnly && causer.CustomGadget0.Info != data.GadgetInfo)
			{
				return;
			}
			DamageTakenCallback damageTakenCallback = new DamageTakenCallback(taker, data, causer, eventId, this.Effect.EventId, amount, true);
			Mural.Post(damageTakenCallback, this.Effect);
			Mural.Post(damageTakenCallback, this.Effect.Gadget);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkAreaListenToDamage));

		private List<CombatObject> _listening = new List<CombatObject>();

		private List<Collider> _inside = new List<Collider>();

		private List<Collider> _newInside = new List<Collider>();

		public EffectKind[] DamageKinds;

		public bool BasicAttackOnly;

		public bool KeepListeningOnDeath;

		public bool IgnoreReactiveModifiers = true;

		private new Transform _trans;

		private bool isSleeping;

		private TimedUpdater _updater = new TimedUpdater(100, false, false);
	}
}
