using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkListenToAssistedKill : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._listening = this.Effect.Gadget.Combat;
			this._listening.ListenToPosDamageCaused += this.OnDamageCaused;
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._listening.ListenToPosDamageCaused -= this.OnDamageCaused;
		}

		private void OnDamageCaused(CombatObject causer, CombatObject taker, ModifierData data, float intAmount, int eventId)
		{
			if (!this.Effect.CheckHit(taker))
			{
				return;
			}
			if (this.IgnoreReactiveModifiers && data.IsReactive)
			{
				return;
			}
			PerkListenToAssistedKill.DamageTaker damageTaker = this._damaged.Find((PerkListenToAssistedKill.DamageTaker x) => x.Taker == taker);
			if (damageTaker == null)
			{
				damageTaker = new PerkListenToAssistedKill.DamageTaker
				{
					Taker = taker
				};
				taker.ListenToObjectUnspawn += this.OnObjectDeath;
				this._damaged.Add(damageTaker);
			}
			damageTaker.LastDamage = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		private void OnObjectDeath(CombatObject obj, UnspawnEvent unspawnEvent)
		{
			long num = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			int num2 = this._damaged.FindIndex((PerkListenToAssistedKill.DamageTaker x) => x.Taker == obj);
			if (num2 < 0)
			{
				return;
			}
			PerkListenToAssistedKill.DamageTaker damageTaker = this._damaged[num2];
			this._damaged.RemoveAt(num2);
			damageTaker.Taker.ListenToObjectUnspawn -= this.OnObjectDeath;
			long num3 = num - damageTaker.LastDamage;
			if ((float)num3 > this.AssistTimeout * 1000f)
			{
				return;
			}
			KillCallback killCallback = new KillCallback(obj);
			Mural.Post(killCallback, this.Effect);
			Mural.Post(killCallback, this.Effect.Gadget);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkListenToAssistedKill));

		public float AssistTimeout;

		public bool IgnoreReactiveModifiers = true;

		private CombatObject _listening;

		private List<PerkListenToAssistedKill.DamageTaker> _damaged = new List<PerkListenToAssistedKill.DamageTaker>();

		[Serializable]
		public class DamageTaker
		{
			public CombatObject Taker;

			public long LastDamage;
		}
	}
}
