using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class SequencialCannon : MultipleEffectsCannon
	{
		private SequencialCannonInfo SequencialInfo
		{
			get
			{
				return base.Info as SequencialCannonInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._heatRate = 1f / ((float)this.SequencialInfo.ChargedTimeMillis / 1000f);
		}

		protected override void GadgetUpdate()
		{
			base.GadgetUpdate();
			if (this._warmupId == -1)
			{
				return;
			}
			this.CurrentHeat = Mathf.Clamp01(this.CurrentHeat + this._heatRate * Time.deltaTime);
			if (!base.Pressed)
			{
				this._buttonHasBeenReleased = true;
			}
			if (this._buttonHasBeenReleased && base.Pressed)
			{
				base.DestroyExistingFiredEffects();
				this._buttonHasBeenReleased = false;
			}
		}

		protected override int FireWarmup()
		{
			this._buttonHasBeenReleased = false;
			this._warmupId = base.FireWarmup();
			return this._warmupId;
		}

		protected override int FireGadget()
		{
			if (this.SequencialInfo.AllEffects.Length <= 0)
			{
				return -1;
			}
			FullEffect fullEffect = this.SequencialInfo.AllEffects[this._currentEffectIndex];
			int num = base.FireGadget(fullEffect.Effect, fullEffect.ModifiersData, GadgetBehaviour.DummyPosition(this.Combat, fullEffect.Effect));
			base.ExistingFiredEffectsAdd(num);
			return num;
		}

		protected override int FireExtraGadget()
		{
			if (this.SequencialInfo.AllExtraEffects.Length <= 0)
			{
				return -1;
			}
			FullEffect fullEffect = this.SequencialInfo.AllExtraEffects[this._currentEffectIndex];
			int num = base.FireExtraGadget(fullEffect.Effect, fullEffect.ExtraModifiersData, null);
			base.ExistingFiredEffectsAdd(num);
			return num;
		}

		protected override void OnMyEffectDestroyed(DestroyEffectMessage evt)
		{
			base.OnMyEffectDestroyed(evt);
			if (evt.RemoveData.TargetEventId == this._finalEffectId)
			{
				return;
			}
			BaseFX.EDestroyReason destroyReason = evt.RemoveData.DestroyReason;
			if (destroyReason != BaseFX.EDestroyReason.Lifetime)
			{
				if (destroyReason == BaseFX.EDestroyReason.Default || destroyReason == BaseFX.EDestroyReason.Gadget)
				{
					this._finalEffectId = this.FireExtraGadget();
					this._warmupId = -1;
					this._currentEffectIndex = -1;
					this.StartCooldown();
				}
			}
			else
			{
				this._currentEffectIndex++;
				if (this._currentEffectIndex >= this.SequencialInfo.AllEffects.Length)
				{
					this._finalEffectId = this.FireOnDeathEffect(evt.RemoveData.Origin, evt.RemoveData.TargetId);
					this._warmupId = -1;
					this._currentEffectIndex = -1;
					this.StartCooldown();
				}
				else
				{
					this.FireGadget();
				}
			}
		}

		private void StartCooldown()
		{
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SequencialCannon));

		private float _heatRate;

		private bool _buttonHasBeenReleased;

		private int _warmupId = -1;

		private int _currentEffectIndex = -1;

		private int _finalEffectId = -1;
	}
}
