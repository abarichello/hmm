﻿using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkApplyModifiersOnDamageEvent : BaseDamageablePerk
	{
		protected override void OnPerkInitialized()
		{
			this.RegisterListeners();
		}

		protected virtual void RegisterListeners()
		{
			switch (this.TargetEvent)
			{
			case EventKind.OnPreDamageCaused:
				this.TargetCombat.ListenToPreDamageCaused += this.OnRefEvent;
				break;
			case EventKind.OnPosDamageCaused:
				this.TargetCombat.ListenToPosDamageCaused += this.OnEvent;
				break;
			case EventKind.OnPreDamageTaken:
				this.TargetCombat.ListenToPreDamageTaken += this.OnRefEvent;
				break;
			case EventKind.OnPosDamageTaken:
				this.TargetCombat.ListenToPosDamageTaken += this.OnEvent;
				break;
			}
		}

		protected virtual void OnRefEvent(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventid)
		{
			if (!this.ShouldApplyModifier(mod))
			{
				return;
			}
			base.ApplyDamage(this.TargetCombat, this.TargetCombat, false);
		}

		protected virtual void OnEvent(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			if (!this.ShouldApplyModifier(mod))
			{
				return;
			}
			base.ApplyDamage(this.TargetCombat, this.TargetCombat, false);
		}

		protected virtual bool ShouldApplyModifier(ModifierData mod)
		{
			return this.CheckGadgetConditions(mod) && mod.Info.Effect.IsHPDamage() && mod.Amount > 0f && (!this.ModTickDeltaMustBeGreaterThanZero || mod.Info.TickDelta <= 0f);
		}

		private bool CheckGadgetConditions(ModifierData mod)
		{
			if (!mod.GadgetInfo)
			{
				return false;
			}
			PerkApplyModifiersOnDamageEvent.GadgetOwner targetGadgetOwner = this.TargetGadgetOwner;
			if (targetGadgetOwner == PerkApplyModifiersOnDamageEvent.GadgetOwner.None)
			{
				return true;
			}
			if (targetGadgetOwner != PerkApplyModifiersOnDamageEvent.GadgetOwner.AnyOtherGadget)
			{
				return targetGadgetOwner != PerkApplyModifiersOnDamageEvent.GadgetOwner.MyGadget || mod.GadgetInfo.GadgetId == this.Effect.Gadget.Info.GadgetId;
			}
			return mod.GadgetInfo.GadgetId != this.Effect.Gadget.Info.GadgetId;
		}

		public override void PerkDestroyed(DestroyEffect destroyEffect)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.RemoveListeners();
		}

		public void OnDisable()
		{
			if (this.TargetCombat != null)
			{
				this.RemoveListeners();
			}
		}

		protected virtual void RemoveListeners()
		{
			switch (this.TargetEvent)
			{
			case EventKind.OnPreDamageCaused:
				this.TargetCombat.ListenToPreDamageCaused -= this.OnRefEvent;
				break;
			case EventKind.OnPosDamageCaused:
				this.TargetCombat.ListenToPosDamageCaused -= this.OnEvent;
				break;
			case EventKind.OnPreDamageTaken:
				this.TargetCombat.ListenToPreDamageTaken -= this.OnRefEvent;
				break;
			case EventKind.OnPosDamageTaken:
				this.TargetCombat.ListenToPosDamageTaken -= this.OnEvent;
				break;
			}
		}

		public EventKind TargetEvent;

		public PerkApplyModifiersOnDamageEvent.GadgetOwner TargetGadgetOwner;

		public bool ModTickDeltaMustBeGreaterThanZero;

		public enum GadgetOwner
		{
			None,
			MyGadget,
			AnyOtherGadget
		}
	}
}
