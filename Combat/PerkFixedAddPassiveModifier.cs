using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkFixedAddPassiveModifier : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		protected override void Awake()
		{
			base.Awake();
			if (this.Modifier.Info.Feedback != null)
			{
				GameHubBehaviour.Hub.Resources.PreCachePrefab(this.Modifier.Info.Feedback.Name, this.Modifier.Info.Feedback.EffectPreCacheCount);
			}
		}

		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.Modifier.SetInfo(this.Modifier.Info, this.Effect.Gadget.Info);
			CombatObject targetCombat = base.GetTargetCombat(this.Effect, this.Target);
			if (targetCombat)
			{
				this._combat = targetCombat.Controller;
			}
			if (this._combat)
			{
				this._combat.AddPassiveModifier(this.Modifier, this.Effect.Gadget.Combat, this.Effect.EventId);
			}
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this._combat)
			{
				this._combat.RemovePassiveModifier(this.Modifier, this.Effect.Gadget.Combat, this.Effect.EventId);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkFixedAddPassiveModifier));

		public BasePerk.PerkTarget Target;

		public ModifierData Modifier;

		private CombatController _combat;
	}
}
