using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkListenToDamageOnThirdParty : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			if (this.UseEventTarget)
			{
				this.ThirdParty = CombatRef.GetCombat(this.Effect.Target);
			}
			if (this.ThirdParty == null)
			{
				PerkListenToDamageOnThirdParty.Log.Warn("PerkListenToDamage - Impossible to get Controller from target");
				return;
			}
			if (this.CheckHit && !this.Effect.CheckHit(this.ThirdParty))
			{
				return;
			}
			this.AddListener(this.ThirdParty);
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (this.ThirdParty)
			{
				this.RemoveListener(this.ThirdParty);
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
			this.ThirdParty = null;
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

		private static readonly BitLogger Log = new BitLogger(typeof(PerkListenToDamageOnThirdParty));

		public CombatObject ThirdParty;

		public EffectKind[] DamageKinds;

		public bool UseEventTarget = true;

		public bool CheckHit = true;

		public bool ListenToPre = true;

		public bool ListenToPos = true;

		public bool KeepListeningOnDeath;

		public bool IgnoreReactiveModifiers = true;
	}
}
