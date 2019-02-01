using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkUpgradeGadget : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			BasePerk.PerkTarget target = this.Target;
			if (target != BasePerk.PerkTarget.Owner)
			{
				if (target == BasePerk.PerkTarget.Target)
				{
					CombatObject combat = CombatRef.GetCombat(this.Effect.Target);
					this._gadget = ((!combat) ? null : combat.GetGadget(this.TargetGadget));
				}
			}
			else
			{
				this._gadget = this.Effect.Gadget.Combat.GetGadget(this.TargetGadget);
			}
			if (this._gadget)
			{
				this._initialUpgradeLevel = this._gadget.GetLevel(this.Upgrade);
				this._gadget.Upgrade(this.Upgrade);
			}
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			if (this._gadget && this._gadget.GetLevel(this.Upgrade) > this._initialUpgradeLevel)
			{
				this._gadget.Downgrade(this.Upgrade);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkUpgradeGadget));

		public BasePerk.PerkTarget Target;

		public GadgetSlot TargetGadget;

		public string Upgrade;

		private GadgetBehaviour _gadget;

		private int _initialUpgradeLevel;
	}
}
