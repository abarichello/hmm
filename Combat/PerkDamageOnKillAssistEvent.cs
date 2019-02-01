using System;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageOnKillAssistEvent : BaseDamageablePerk
	{
		protected override void OnPerkInitialized()
		{
			this._target = base.GetTargetCombat(this.Effect, this.Target);
			if (!this._target || !this._target.IsAlive())
			{
				return;
			}
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.OnUnspawn;
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectUnspawn += this.OnUnspawn;
		}

		private void OnUnspawn(PlayerEvent data)
		{
			if ((!this.DamageOnKill || data.PossibleKiller != this._target.Id.ObjId) && (!this.DamageOnAssist || !data.Assists.Contains(this._target.Id.ObjId)))
			{
				return;
			}
			base.ApplyDamage(this.AdditionalTargetCombat, this._target, false);
		}

		public override void OnDestroyEffect(DestroyEffect evt)
		{
			base.OnDestroyEffect(evt);
			this.RemoveListeners();
		}

		private void RemoveListeners()
		{
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.OnUnspawn;
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectUnspawn -= this.OnUnspawn;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkDamageOnKillAssistEvent));

		public bool DamageOnKill;

		public bool DamageOnAssist;

		private CombatObject _target;
	}
}
