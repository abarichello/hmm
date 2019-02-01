using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkListenToDamage : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._listening = this.Effect.GetTargetCombat(this.Target);
			if (this._listening == null)
			{
				PerkListenToDamage.Log.Warn("PerkListenToDamage - Impossible to get CombatObject from target");
				return;
			}
			if (!this.Effect.CheckHit(this._listening))
			{
				return;
			}
			this.AddListener(this._listening);
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			if (this._listening)
			{
				this.RemoveListener(this._listening);
			}
		}

		private void AddListener(CombatObject obj)
		{
			if (this.ListenToPre)
			{
				obj.ListenToPreDamageTaken += this.OnPreDamageTaken;
			}
			if (this.ListenToPos)
			{
				obj.ListenToPosDamageTaken += this.OnPosDamageTaken;
			}
			if (!this.KeepListeningOnDeath)
			{
				obj.ListenToObjectUnspawn += this.OnObjectDeath;
			}
		}

		private void RemoveListener(CombatObject obj)
		{
			if (this.ListenToPre)
			{
				obj.ListenToPreDamageTaken -= this.OnPreDamageTaken;
			}
			if (this.ListenToPos)
			{
				obj.ListenToPosDamageTaken -= this.OnPosDamageTaken;
			}
			if (!this.KeepListeningOnDeath)
			{
				obj.ListenToObjectUnspawn -= this.OnObjectDeath;
			}
		}

		private void OnObjectDeath(CombatObject obj, UnspawnEvent msg)
		{
			this._listening = null;
			this.RemoveListener(obj);
		}

		private void OnPreDamageTaken(CombatObject causer, CombatObject taker, ModifierData data, ref float amount, int eventId)
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
			Mural.Post(new DamageTakenCallback(taker, data, causer, eventId, this.Effect.EventId, amount, false), this.Effect);
			Mural.Post(new DamageTakenCallback(taker, data, causer, eventId, this.Effect.EventId, amount, false), this.Effect.Gadget);
		}

		private void OnPosDamageTaken(CombatObject causer, CombatObject taker, ModifierData data, float amount, int eventId)
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
			Mural.Post(new DamageTakenCallback(taker, data, causer, eventId, this.Effect.EventId, amount, true), this.Effect);
			Mural.Post(new DamageTakenCallback(taker, data, causer, eventId, this.Effect.EventId, amount, true), this.Effect.Gadget);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkListenToDamage));

		private CombatObject _listening;

		public BasePerk.PerkTarget Target;

		public EffectKind[] DamageKinds;

		public bool ListenToPre = true;

		public bool ListenToPos = true;

		public bool KeepListeningOnDeath;

		public bool IgnoreReactiveModifiers = true;
	}
}
