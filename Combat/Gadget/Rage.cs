using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class Rage : BasicCannon
	{
		private RageInfo MyInfo
		{
			get
			{
				return base.Info as RageInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.Gadget0HitRatio = new Upgradeable(this.MyInfo.Gadget0HitRatioUpgrade, this.MyInfo.Gadget0HitRatio, this.MyInfo.UpgradesValues);
			this.Gadget1HitRatio = new Upgradeable(this.MyInfo.Gadget1HitRatioUpgrade, this.MyInfo.Gadget1HitRatio, this.MyInfo.UpgradesValues);
			this.Gadget2HitRatio = new Upgradeable(this.MyInfo.Gadget2HitRatioUpgrade, this.MyInfo.Gadget2HitRatio, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.Gadget0HitRatio.SetLevel(upgradeName, level);
			this.Gadget1HitRatio.SetLevel(upgradeName, level);
			this.Gadget2HitRatio.SetLevel(upgradeName, level);
		}

		private int CurrentRageLevel
		{
			set
			{
				if (this._currentRageLevel == value)
				{
					return;
				}
				if (this._currentRageLevel < value)
				{
					for (int i = 0; i < this.MyInfo.InvisibleUpgrades.Length; i++)
					{
						InvisibleUpgradeInfo invisibleUpgradeInfo = this.MyInfo.InvisibleUpgrades[i];
						base.Upgrade(invisibleUpgradeInfo.Name);
					}
				}
				else
				{
					for (int j = 0; j < this.MyInfo.InvisibleUpgrades.Length; j++)
					{
						InvisibleUpgradeInfo invisibleUpgradeInfo2 = this.MyInfo.InvisibleUpgrades[j];
						base.Downgrade(invisibleUpgradeInfo2.Name);
					}
				}
				this._currentRageLevel = value;
			}
		}

		private float CurrentRatio
		{
			get
			{
				return this._currentRatio;
			}
			set
			{
				this._currentRatio = Mathf.Max(value, 0f);
				this._currentPowerPct = this.MyInfo.PowerPctRageRatio * this._currentRatio;
				ModifierData[] datas = ModifierData.CreateConvoluted(this._damage, this._currentPowerPct);
				if (this._currentRatio == 0f)
				{
					this.Combat.Controller.RemovePassiveModifiers(datas, this.Combat, -1);
				}
				else
				{
					this.Combat.Controller.AddPassiveModifiers(datas, this.Combat, -1);
				}
				int num = 0;
				for (int i = 0; i < this.MyInfo.RageValues.Length; i++)
				{
					int num2 = this.MyInfo.RageValues[i];
					if (this._currentRatio < (float)num2)
					{
						break;
					}
					num++;
				}
				this.Combat.GadgetStates.SetJokerBarState(this._currentRatio, 100f);
				this.CurrentRageLevel = num;
			}
		}

		protected override void OnPreDamageCaused(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventid)
		{
			if ((this._lastEventIdDamageCaused == eventid && mod.TickDelta <= 0f) || !mod.Info.Effect.IsHPDamage())
			{
				return;
			}
			this._lastEventIdDamageCaused = eventid;
			float num = 0f;
			if (mod.GadgetInfo.GadgetId == this.Combat.CustomGadget0.Info.GadgetId)
			{
				num = this.Gadget0HitRatio;
			}
			else if (mod.GadgetInfo.GadgetId == this.Combat.CustomGadget1.Info.GadgetId)
			{
				num = this.Gadget1HitRatio;
			}
			else if (mod.GadgetInfo.GadgetId == this.Combat.CustomGadget2.Info.GadgetId)
			{
				num = this.Gadget2HitRatio;
			}
			this.CurrentRatio = Mathf.Min(this.CurrentRatio + num, 100f);
			this._rageDegenerationStoppedUntil = (int)((float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + this.Cooldown * 1000f);
		}

		protected override void RunBeforeUpdate()
		{
			if (this.CurrentRatio <= 0f || GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.GameTime.GetPlaybackTime() < this._rageDegenerationStoppedUntil)
			{
				return;
			}
			this.CurrentRatio -= this.MyInfo.DegenerationRatio * Time.deltaTime;
		}

		protected Upgradeable Gadget0HitRatio;

		protected Upgradeable Gadget1HitRatio;

		protected Upgradeable Gadget2HitRatio;

		private int _rageDegenerationStoppedUntil;

		private float _currentPowerPct;

		private int _currentRageLevel;

		private float _currentRatio;

		private int _lastEventIdDamageCaused = -1;
	}
}
