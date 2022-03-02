using System;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BoostGadget : BasicCannon
	{
		public BoostGadgetInfo BoostInfo
		{
			get
			{
				return base.Info as BoostGadgetInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._movement = (CarMovement)this.Combat.Movement;
			if (this.BoostInfo.StagesLifetime.Length != this.BoostInfo.StagesModifiers.Length || this.BoostInfo.StagesLifetime.Length == 0)
			{
				BoostGadget.Log.Error("The number of StageModifiers should be equal to the number of StageLifetimes and not 0");
			}
			this._myGadgetPressedUpdater = (GadgetPressedUpdater)this._gadgetUpdater;
		}

		protected override int FireGadget()
		{
			this._currentStageIndex = 0;
			this._currentStageEffectId = -1;
			this.FireStageEffect();
			return base.FireGadget();
		}

		protected override void OnMyEffectDestroyed(DestroyEffectMessage evt)
		{
			if (evt.RemoveData.DestroyReason == BaseFX.EDestroyReason.Lifetime)
			{
				this._currentStageIndex++;
				this.FireStageEffect();
			}
			else if (evt.RemoveData.TargetEventId != this._currentStageEffectId && this._currentStageEffectId != -1)
			{
				base.Pressed = false;
			}
			if (evt.RemoveData.TargetEventId == this._currentBoostFailEffectId)
			{
				this._myGadgetPressedUpdater.PressedBlocked = false;
				this._currentBoostFailEffectId = -1;
			}
		}

		private void FireStageEffect()
		{
			if (this._currentStageEffectId != -1)
			{
				BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(this._currentStageEffectId);
				if (baseFx != null)
				{
					baseFx.TriggerDefaultDestroy(-1);
				}
			}
			if (this._currentStageIndex < this.BoostInfo.StagesLifetime.Length && this._currentBoostFailEffectId == -1)
			{
				this._currentStageEffectId = this.FireExtraGadget(this.BoostInfo.ExtraEffect, ModifierData.CreateData(this.BoostInfo.StagesModifiers[this._currentStageIndex].Modifiers, this.BoostInfo), new ModifierData[0], delegate(EffectEvent data)
				{
					data.LifeTime = this.BoostInfo.StagesLifetime[this._currentStageIndex];
				});
				base.ExistingFiredEffectsAdd(this._currentStageEffectId);
			}
		}

		protected override void GadgetUpdate()
		{
			base.GadgetUpdate();
			if (this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Jammed) && this._currentJammedEffectId == -1 && !GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this.Combat))
			{
				this._currentJammedEffectId = base.FireExtraGadget(this.BoostInfo.JammedEffect, new ModifierData[0], new ModifierData[0]);
			}
			else if (this._currentJammedEffectId != -1 && !this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Jammed))
			{
				BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(this._currentJammedEffectId);
				if (baseFx)
				{
					baseFx.TriggerDefaultDestroy(-1);
					this._currentJammedEffectId = -1;
				}
			}
			bool flag = this.CheckGadgetsUsed();
			bool flag2 = this._movement.IsDrifting || this._movement.SpeedZ <= this.BoostInfo.ZSpeedToStartBoostEffect || flag || !this.Combat.IsAlive();
			if (base.Pressed && flag2)
			{
				if (this._currentBoostFailEffectId == -1)
				{
					this._myGadgetPressedUpdater.PressedBlocked = true;
					this._currentBoostFailEffectId = base.FireExtraGadget(this.BoostInfo.DriftingEffect, new ModifierData[0], new ModifierData[0]);
				}
			}
			else if (this._currentBoostFailEffectId != -1 && (!flag2 || !base.Pressed))
			{
				this.DestroyFailEffect();
			}
		}

		private bool CheckGadgetsUsed()
		{
			return this.CheckGadgetUsed(GadgetSlot.BoostGadget) || this.CheckGadgetUsed(GadgetSlot.CustomGadget0) || this.CheckGadgetUsed(GadgetSlot.CustomGadget1) || this.CheckGadgetUsed(GadgetSlot.CustomGadget2);
		}

		private bool CheckGadgetUsed(GadgetSlot slot)
		{
			GadgetBehaviour gadgetBehaviour;
			switch (slot)
			{
			case GadgetSlot.CustomGadget0:
				gadgetBehaviour = this.Combat.CustomGadget0;
				break;
			case GadgetSlot.CustomGadget1:
				gadgetBehaviour = this.Combat.CustomGadget1;
				break;
			case GadgetSlot.CustomGadget2:
				gadgetBehaviour = this.Combat.CustomGadget2;
				break;
			case GadgetSlot.BoostGadget:
				gadgetBehaviour = this.Combat.BoostGadget;
				break;
			default:
				BoostGadget.Log.WarnFormat("GadgetSlot not implemented: GadgetSlot={0}", new object[]
				{
					slot
				});
				return false;
			}
			if (gadgetBehaviour == null)
			{
				BoostGadget.Log.WarnFormat("GadgetBehaviour not found: GadgetSlot={0} Character={1}", new object[]
				{
					slot,
					this.Combat.Player.GetCharacterBiName()
				});
				return false;
			}
			GadgetInfo info = gadgetBehaviour.Info;
			if (info == null)
			{
				BoostGadget.Log.WarnFormat("GadgetInfo not found: GadgetSlot={0} Character={1}", new object[]
				{
					slot,
					this.Combat.Player.GetCharacterBiName()
				});
				return false;
			}
			if (!info.TurnOffTrailGadget)
			{
				return false;
			}
			bool result = false;
			GadgetKind kind = gadgetBehaviour.Kind;
			if (kind != GadgetKind.Instant)
			{
				BoostGadget.Log.WarnFormat("GadgetKind not implemented: GadgetKind={0}", new object[]
				{
					gadgetBehaviour.Kind
				});
			}
			else
			{
				result = (gadgetBehaviour.Pressed && this.Combat.GadgetStates.GetGadgetState(slot).GadgetState == GadgetState.Ready);
			}
			return result;
		}

		private void DestroyFailEffect()
		{
			BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(this._currentBoostFailEffectId);
			if (baseFx)
			{
				baseFx.TriggerDefaultDestroy(-1);
			}
			this._myGadgetPressedUpdater.PressedBlocked = false;
			this._currentBoostFailEffectId = -1;
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			this.DestroyFailEffect();
			base.Pressed = false;
			this._currentStageIndex = 0;
			this._currentStageEffectId = -1;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BoostGadget));

		private int _currentStageIndex;

		private int _currentStageEffectId = -1;

		private int _currentBoostFailEffectId = -1;

		private int _currentJammedEffectId = -1;

		private CarMovement _movement;

		public GadgetPressedUpdater _myGadgetPressedUpdater;
	}
}
