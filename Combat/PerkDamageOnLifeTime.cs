using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageOnLifeTime : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._deathTime = this.Effect.Data.DeathTime;
			this._damaged = false;
		}

		private void FixedUpdate()
		{
			long num = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (num < this._deathTime)
			{
				return;
			}
			if (!this._damaged)
			{
				this._damaged = true;
				CombatObject targetCombat = base.GetTargetCombat(this.Effect, this.Target);
				ModifierData[] datas = (!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers;
				targetCombat.Controller.AddModifiers(datas, this.Effect.Gadget.Combat, this.Effect.EventId, false);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDamageOnLifeTime));

		public BasePerk.PerkTarget Target;

		public bool UseExtraModifiers;

		private long _deathTime;

		private bool _damaged;
	}
}
