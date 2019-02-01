using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class HPRegenBoost : BasicCannon, DamageTakenCallback.IDamageTakenCallbackListener
	{
		public HPRegenBoostInfo MyInfo
		{
			get
			{
				return base.Info as HPRegenBoostInfo;
			}
		}

		public override void SetInfo(GadgetInfo gadget)
		{
			base.SetInfo(gadget);
			HPRegenBoostInfo myInfo = this.MyInfo;
			this._upgDamage = ModifierData.CreateData(myInfo.Damage, myInfo);
			this._upgChargesMax = new Upgradeable(myInfo.ChargeMaxNumberUpgrade, (float)myInfo.ChargeMaxNumber, myInfo.UpgradesValues);
			this._upgChargeDuration = new Upgradeable(myInfo.ChargeDurationUpgrade, myInfo.ChargeDuration, myInfo.UpgradesValues);
			this._upgChargeMinInterval = new Upgradeable(myInfo.ChargeMinIntervalUpgrade, myInfo.ChargeMinInterval, myInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgDamage.SetLevel(upgradeName, level);
			this._upgChargesMax.SetLevel(upgradeName, level);
			this._upgChargeDuration.SetLevel(upgradeName, level);
			this._upgChargeMinInterval.SetLevel(upgradeName, level);
			this.UpdateModifiers();
		}

		public override void Activate()
		{
			base.Activate();
			this._activeCharges = new List<long>();
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
			this._currentEffectId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			if (this._updater.ShouldHalt())
			{
				return;
			}
			if (!base.Activated)
			{
				return;
			}
			if (this._activeCharges.Count == 0)
			{
				return;
			}
			if (this._activeCharges[0] > (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime())
			{
				return;
			}
			this._activeCharges.RemoveAt(0);
			if (this._activeCharges.Count > 0)
			{
				this.UpdateModifiers();
			}
			else
			{
				this.RemoveModifiers();
			}
		}

		public void OnDamageTakenCallback(DamageTakenCallback evt)
		{
			if (!evt.TakerCombatObject)
			{
				return;
			}
			if (evt.ListenerEffectId != this._currentEffectId)
			{
				return;
			}
			if (this._chargeHaltTimeMillis > (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime())
			{
				return;
			}
			if ((float)this._activeCharges.Count >= this._upgChargesMax.Get())
			{
				this._activeCharges.RemoveAt(0);
			}
			this._activeCharges.Add((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + (long)(this._upgChargeDuration.Get() * 1000f));
			this._chargeHaltTimeMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + (long)(this._upgChargeMinInterval * 1000f);
			this._chargeLastTimeMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.UpdateModifiers();
		}

		private void UpdateModifiers()
		{
			if (this._activeCharges == null || this._activeCharges.Count < 1)
			{
				return;
			}
			this.Combat.Controller.AddPassiveModifiers(ModifierData.CreateConvoluted(this._damage, (float)this._activeCharges.Count), this.Combat, this._currentEffectId);
			this.RemoveCurrentFeedback();
			if (this.MyInfo.Feedback2D != null)
			{
				float num = (float)(this._activeCharges[this._activeCharges.Count - 1] - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime());
				this._currentFeedbackId = this.Combat.Feedback.Add(this.MyInfo.Feedback2D, -1, this.Combat.Id.ObjId, (int)this._chargeLastTimeMillis, (int)((float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + num), this._activeCharges.Count, base.Slot);
			}
		}

		private void RemoveModifiers()
		{
			if (!base.Activated)
			{
				return;
			}
			this._activeCharges.Clear();
			this.Combat.Controller.RemovePassiveModifiers(ModifierData.CreateData(this.MyInfo.Damage, this.MyInfo), this.Combat, this._currentEffectId);
			this.RemoveCurrentFeedback();
		}

		private void RemoveCurrentFeedback()
		{
			if (this._currentFeedbackId != -1)
			{
				this.Combat.Feedback.Remove(this._currentFeedbackId);
				this._currentFeedbackId = -1;
			}
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.RemoveModifiers();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HPRegenBoost));

		private ModifierData[] _upgDamage;

		private Upgradeable _upgChargesMax;

		private Upgradeable _upgChargeDuration;

		private Upgradeable _upgChargeMinInterval;

		private long _chargeHaltTimeMillis;

		private long _chargeLastTimeMillis;

		private List<long> _activeCharges;

		private int _currentEffectId = -1;

		private int _currentFeedbackId = -1;

		private TimedUpdater _updater = new TimedUpdater
		{
			PeriodMillis = 100
		};
	}
}
