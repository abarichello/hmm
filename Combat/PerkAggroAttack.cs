using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkAggroAttack : PerkAggro
	{
		public override void PerkInitialized()
		{
			base.PerkInitialized();
			if (GameHubBehaviour.Hub)
			{
				this._cooldown = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
		}

		protected override void FixedUpdateServer()
		{
			base.FixedUpdateServer();
			long num = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (this._cooldown > num)
			{
				return;
			}
			if (!this._wantsToAttack)
			{
				this._cooldown = num;
				return;
			}
			long num2 = num - this._cooldown;
			this._cooldown = (long)(this.Cooldown * 1000f) + num - num2;
			Mural.Post(new AggroAttackCallback(this.Effect, base.AggroTarget), this.Effect.Gadget);
		}

		public new static readonly BitLogger Log = new BitLogger(typeof(PerkAggroAttack));

		public float Cooldown;

		private long _cooldown;
	}
}
