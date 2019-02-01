using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class MortarWarningWithStacks : MortarWarning, TriggerEnterCallback.ITriggerEnterCallbackListener
	{
		private MortarWarningWithStacksInfo MyInfo
		{
			get
			{
				return base.Info as MortarWarningWithStacksInfo;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			this._upgStackedModifiers = ModifierData.CreateData(this.MyInfo.StackModifiers, this.MyInfo);
			this._upgExplosionModifiers = ModifierData.CreateData(this.MyInfo.ExplosionModifiers, this.MyInfo);
			this._upgExplosionMaxChargedModifiers = ModifierData.CreateData(this.MyInfo.ExplosionMaxChargedModifiers, this.MyInfo);
			this._upgExplosionRange = new Upgradeable(this.MyInfo.ExplosionRangeUpgrade, this.MyInfo.ExplosionRange, this.MyInfo.UpgradesValues);
			this._stacks = new StackedModifier(new Func<FXInfo, EffectEvent>(base.GetEffectEvent), this.MyInfo.StackEffect, this.MyInfo.ExplosionEffect, this.Combat, this.MyInfo.StackFeedback, this.MyInfo.NextStackTickMillis);
			this._stacks.SetLevel(this._upgStackedModifiers, this._upgExplosionModifiers, this._upgExplosionMaxChargedModifiers, this.MyInfo.ExplosionMultipliers, this._upgExplosionRange, this.MyInfo.StackLifeTime, this.MyInfo.StackMaxCount);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgExplosionModifiers.SetLevel(upgradeName, level);
			this._upgExplosionMaxChargedModifiers.SetLevel(upgradeName, level);
			this._upgExplosionRange.SetLevel(upgradeName, level);
			this._stacks.SetLevel(this._upgStackedModifiers, this._upgExplosionModifiers, this._upgExplosionMaxChargedModifiers, this.MyInfo.ExplosionMultipliers, this._upgExplosionRange, this.MyInfo.StackLifeTime, this.MyInfo.StackMaxCount);
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			base.GadgetUpdate();
			this._stacks.RunGadgetFixedUpdate();
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
			this._stacks.OnTriggerEnterCallback(evt, this);
		}

		protected ModifierData[] _upgStackedModifiers;

		protected ModifierData[] _upgExplosionModifiers;

		protected ModifierData[] _upgExplosionMaxChargedModifiers;

		protected Upgradeable _upgExplosionRange;

		[SerializeField]
		private StackedModifier _stacks;
	}
}
