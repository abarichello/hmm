using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkReturnDamage : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			if (this.Damaged != null)
			{
				this.Damaged.Clear();
			}
			this._modifiers = ((!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers);
			BasePerk.PerkTarget target = this.Target;
			if (target != BasePerk.PerkTarget.Owner)
			{
				if (target == BasePerk.PerkTarget.Target)
				{
					this._combat = CombatRef.GetCombat(this.Effect.Target);
				}
			}
			else
			{
				this._combat = this.Effect.Gadget.Combat;
			}
			if (!this._combat)
			{
				base.enabled = false;
				return;
			}
			PerkReturnDamage.ReturnType returnType = this.returnType;
			if (returnType != PerkReturnDamage.ReturnType.Pre)
			{
				if (returnType != PerkReturnDamage.ReturnType.Pos)
				{
					throw new ArgumentOutOfRangeException();
				}
				this._combat.ListenToPosDamageTaken += this.OnPosDamageTaken;
			}
			else
			{
				this._combat.ListenToPreDamageTaken += this.OnPreDamageTaken;
			}
			this._combat.ListenToObjectUnspawn += this.OnObjectUnspawn;
		}

		private void OnPreDamageTaken(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventid)
		{
			if (!this.CanReturnDamage(causer, mod))
			{
				return;
			}
			this.ApplyDamage(causer, amount);
		}

		private void OnPosDamageTaken(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			if (!this.CanReturnDamage(causer, mod))
			{
				return;
			}
			this.ApplyDamage(causer, amount);
		}

		private bool CanReturnDamage(CombatObject causer, ModifierData mod)
		{
			return this.Effect.CheckHit(causer) && !mod.IsReactive && mod.Info.Effect.IsHPDamage() && (!this.BasicAttackOnly || !(causer.CustomGadget0.Info != mod.GadgetInfo)) && (!this.SingleTarget || this.Damaged.Count <= 0) && (!this.HitOnlyOnce || !this.Damaged.Contains(causer.Id.ObjId));
		}

		private void ApplyDamage(CombatObject causer, float amount)
		{
			ModifierData[] array;
			if (this.UseConvolutedDamage)
			{
				ModifierData[] modifiers = this._modifiers;
				if (PerkReturnDamage.<>f__mg$cache0 == null)
				{
					PerkReturnDamage.<>f__mg$cache0 = new Func<ModifierData, bool>(ModifierDataExt.CanReturnDamage);
				}
				array = ModifierData.CreateConvolutedFiltering(modifiers, amount, PerkReturnDamage.<>f__mg$cache0);
			}
			else
			{
				array = ModifierData.CopyData(this._modifiers);
			}
			ModifierData[] datas = array;
			causer.Controller.AddModifiers(datas, this.Effect.Gadget.Combat, this.Effect.EventId, false);
		}

		private void OnObjectUnspawn(CombatObject obj, UnspawnEvent msg)
		{
			this.UnRegisterListeners();
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			this.UnRegisterListeners();
		}

		private void UnRegisterListeners()
		{
			if (!this._combat)
			{
				return;
			}
			PerkReturnDamage.ReturnType returnType = this.returnType;
			if (returnType != PerkReturnDamage.ReturnType.Pre)
			{
				if (returnType != PerkReturnDamage.ReturnType.Pos)
				{
					throw new ArgumentOutOfRangeException();
				}
				this._combat.ListenToPosDamageTaken -= this.OnPosDamageTaken;
			}
			else
			{
				this._combat.ListenToPreDamageTaken -= this.OnPreDamageTaken;
			}
			this._combat.ListenToObjectUnspawn -= this.OnObjectUnspawn;
			this._combat = null;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkReturnDamage));

		public BasePerk.PerkTarget Target;

		public bool BasicAttackOnly;

		public bool UseExtraModifiers;

		public bool IgnoreReactiveModifiers = true;

		public PerkReturnDamage.ReturnType returnType = PerkReturnDamage.ReturnType.Pos;

		[Header("If checked, will use amount as a percentage value. Else will use modifer as it is")]
		public bool UseConvolutedDamage = true;

		public bool SingleTarget;

		public bool HitOnlyOnce;

		private List<int> Damaged = new List<int>();

		private CombatObject _combat;

		private ModifierData[] _modifiers;

		[CompilerGenerated]
		private static Func<ModifierData, bool> <>f__mg$cache0;

		public enum ReturnType
		{
			Pre,
			Pos
		}
	}
}
