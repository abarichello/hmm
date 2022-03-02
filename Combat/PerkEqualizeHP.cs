using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkEqualizeHP : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._combatObjectA = base.GetTargetCombat(this.Effect, this.TargetA);
			this._combatObjectB = base.GetTargetCombat(this.Effect, this.TargetB);
			this._timedUpdater = new TimedUpdater(this.TickMillis, false, false);
			this._remainingTicks = (int)Math.Truncate((double)(this.Effect.Data.LifeTime * 1000f / (float)this.TickMillis));
			float num = this._combatObjectA.Data.HP / (float)this._combatObjectA.Data.HPMax;
			float num2 = this._combatObjectB.Data.HP / (float)this._combatObjectB.Data.HPMax;
			float num3 = Mathf.Max(num, num2);
			this._healValueA = Mathf.Max(((float)this._combatObjectA.Data.HPMax * num3 - this._combatObjectA.Data.HP) / (float)this._remainingTicks, 0f);
			this._healValueB = Mathf.Max(((float)this._combatObjectB.Data.HPMax * num3 - this._combatObjectB.Data.HP) / (float)this._remainingTicks, 0f);
		}

		private void FixedUpdate()
		{
			if (this._timedUpdater.ShouldHalt())
			{
				return;
			}
			if (this._remainingTicks == 0)
			{
				return;
			}
			this._remainingTicks--;
			ModifierData[] datas = ModifierData.CreateConvoluted(this.Effect.Data.Modifiers, this._healValueA);
			ModifierData[] datas2 = ModifierData.CreateConvoluted(this.Effect.Data.Modifiers, this._healValueB);
			this._combatObjectA.Controller.AddModifiers(datas, this.Effect.Gadget.Combat, this.Effect.EventId, false);
			this._combatObjectB.Controller.AddModifiers(datas2, this.Effect.Gadget.Combat, this.Effect.EventId, false);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkEqualizeHP));

		public BasePerk.PerkTarget TargetA;

		public BasePerk.PerkTarget TargetB;

		public int TickMillis;

		private CombatObject _combatObjectA;

		private CombatObject _combatObjectB;

		private TimedUpdater _timedUpdater;

		private int _remainingTicks;

		private float _healValueA;

		private float _healValueB;
	}
}
