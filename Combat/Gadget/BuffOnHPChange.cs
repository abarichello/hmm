using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BuffOnHPChange : GadgetBehaviour
	{
		public BuffOnHPChangeInfo ReactionInfo
		{
			get
			{
				return base.Info as BuffOnHPChangeInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._originalModifiers = ModifierData.CreateData(this.ReactionInfo.Damage, this.ReactionInfo);
			this._minHP = new Upgradeable(this.ReactionInfo.MinHPUpgrade, this.ReactionInfo.MinHPpct, this.ReactionInfo.UpgradesValues);
			this._maxHP = new Upgradeable(this.ReactionInfo.MaxHPUpgrade, this.ReactionInfo.MaxHPpct, this.ReactionInfo.UpgradesValues);
			this._reverse = this.ReactionInfo.Reverse;
			this.Combat.Data.OnHPChanged += this.OnHPChanged;
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._minHP.SetLevel(upgradeName, level);
			this._maxHP.SetLevel(upgradeName, level);
		}

		public void OnHPChanged(float currentHP)
		{
			float num = currentHP / (float)this.Combat.Data.HPMax;
			float num2 = this._maxHP - this._minHP;
			float num3 = Mathf.Min(num2, (!this._reverse) ? Mathf.Max(0f, this._maxHP - num) : Mathf.Max(0f, num - this._minHP));
			if (num2 <= 0f)
			{
				BuffOnHPChange.Log.Error("Min HP is not lower than Max HP. Invalid values!");
				return;
			}
			this._modifiers = ModifierData.RemoveAmountPercent(this._originalModifiers, num3 / num2);
			if (this._modifiers != null)
			{
				this.Combat.Controller.RemovePassiveModifiers(this._modifiers, this.Combat, -1);
			}
			this.Combat.Controller.AddPassiveModifiers(this._modifiers, this.Combat, -1);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BuffOnHPChange));

		protected ModifierData[] _originalModifiers;

		protected ModifierData[] _modifiers;

		protected Upgradeable _minHP;

		protected Upgradeable _maxHP;

		protected bool _reverse;
	}
}
