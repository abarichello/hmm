using System;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class IndestructibleEffectCannon : BasicCannon
	{
		private IndestructibleEffectCannonInfo MyInfo
		{
			get
			{
				return base.Info as IndestructibleEffectCannonInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectDeath += this.PlayersOnListenToObjectDeath;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectDeath += this.PlayersOnListenToObjectDeath;
			CombatController.OnInstantModifierApplied += this.ControllerOnOnInstantModifierApplied;
			this.Combat.ListenToObjectUnspawn += this.CombatOnListenToObjectUnspawn;
		}

		private void CombatOnListenToObjectUnspawn(CombatObject combatObject, UnspawnEvent msg)
		{
			base.DestroyExistingFiredEffects();
			this.ClearUndeadEffect();
		}

		private void ControllerOnOnInstantModifierApplied(ModifierInstance mod, CombatObject causer, CombatObject target, float amount, int eventId)
		{
			if (mod.Info.Effect == EffectKind.HPGodDamage && target.Id.ObjId == this.Combat.Id.ObjId)
			{
				base.DestroyExistingFiredEffects();
			}
		}

		private void PlayersOnListenToObjectDeath(PlayerEvent data)
		{
			if (this._undeadEffectId != -1 || data.TargetId != this.Combat.Id.ObjId || data.Reason != SpawnReason.Death)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this.Combat.Id.ObjId))
			{
				this.Combat.BombGadget.Disable(BombGadget.DisableReason.LinkBroke);
			}
			this._undeadEffectId = base.FireExtraGadget(data.TargetId);
			GameHubBehaviour.Hub.Effects.ListenToDestroy(this._undeadEffectId, new EffectsManager.EffectDestroyed(this.ListenToDestroyCallback));
		}

		private void ListenToDestroyCallback(EffectRemoveEvent data)
		{
			this.ClearUndeadEffect();
			base.DestroyExistingFiredEffects();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectDeath -= this.PlayersOnListenToObjectDeath;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectDeath -= this.PlayersOnListenToObjectDeath;
			CombatController.OnInstantModifierApplied -= this.ControllerOnOnInstantModifierApplied;
			this.Combat.ListenToObjectUnspawn -= this.CombatOnListenToObjectUnspawn;
			this.ClearUndeadEffect();
		}

		private void ClearUndeadEffect()
		{
			if (this._undeadEffectId != -1)
			{
				GameHubBehaviour.Hub.Effects.UnlistenToDestroy(this._undeadEffectId, new EffectsManager.EffectDestroyed(this.ListenToDestroyCallback));
			}
			this._undeadEffectId = -1;
		}

		public override void OnObjectSpawned(SpawnEvent evt)
		{
			base.OnObjectSpawned(evt);
			this.ClearUndeadEffect();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(IndestructibleEffectCannon));

		private int _undeadEffectId = -1;
	}
}
