using System;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using FMod;
using HeavyMetalMachines.AnimationHacks;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.Frontend
{
	public class UIGadgetController : GameHubBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event Action CustomParamsUpdate;

		private void AddCombatListeners()
		{
			if (this._gadget != null && this._gadget.Combat != null)
			{
				this._gadget.Combat.ListenToObjectSpawn += this.OnCombatObjecSpawn;
				this._gadget.Combat.ListenToObjectUnspawn += this.OnCombatObjecUnspawn;
			}
		}

		private void RemoveCombatListeners()
		{
			if (this._gadget != null && this._gadget.Combat != null)
			{
				this._gadget.Combat.ListenToObjectSpawn -= this.OnCombatObjecSpawn;
				this._gadget.Combat.ListenToObjectUnspawn -= this.OnCombatObjecUnspawn;
			}
		}

		public void Setup(PlayerData playerData, GadgetData gadgetData, GadgetBehaviour gadget, GadgetSlot slot)
		{
			this.RemoveCombatListeners();
			this._gadgetData = gadgetData;
			this._gadget = gadget;
			this._gadgetSlot = slot;
			this._gadgetNature = gadget.Nature;
			this._kind = gadget.Kind;
			this._combatData = gadget.Combat.Data;
			this._customGadget = (CombatGadget)gadget.Combat.GetGadgetContext((int)slot);
			if (SpectatorController.IsSpectating)
			{
				this._currentPlayerData = playerData;
				this.voiceOverController = null;
			}
			else
			{
				this._currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
				this.voiceOverController = playerData.CharacterInstance.GetBitComponent<VoiceOverController>();
			}
			bool flag = this._gadget.Slot == GadgetSlot.PassiveGadget;
			if (!flag || playerData.GetCharacterHasPassive())
			{
				this.GadgetSprite.SpriteName = HudUtils.GetGadgetIconName(this._currentPlayerData, this._gadgetSlot);
			}
			this.SetupOverheatComponents();
			this.AddCombatListeners();
			this._isAlive = gadget.Combat.IsAlive();
			this.TryToSetupCooldown();
			this.TryToSetupRage();
			this.TryToSetupBuffOnSpeed();
			this.TryToSetupCustomGadget();
			if (flag)
			{
				base.gameObject.SetActive(this.RageComponents.IsEnabled() || this.BuffOnSpeedComponents.IsEnabled());
			}
		}

		private void TryToSetupCustomGadget()
		{
			this.TryUpdateCustomGadgetCooldownTimer();
			int i = 0;
			while (i < this._customParameters.Length)
			{
				UICustomGadgetDataComponent uicustomGadgetDataComponent = this._customParameters[i];
				if (this._customGadget == null)
				{
					if (uicustomGadgetDataComponent.InitLegacy(this._gadget))
					{
						goto IL_59;
					}
				}
				else if (uicustomGadgetDataComponent.Init(this._customGadget))
				{
					goto IL_59;
				}
				IL_7D:
				i++;
				continue;
				IL_59:
				this.CustomParamsUpdate += uicustomGadgetDataComponent.Update;
				if (uicustomGadgetDataComponent.IsChargesParameter)
				{
					this._chargesParameter = uicustomGadgetDataComponent;
					goto IL_7D;
				}
				goto IL_7D;
			}
			for (int j = 0; j < this._customParametersCallback.Length; j++)
			{
				this._customParametersCallback[j].Init(this._combatData.Combat, true);
			}
			this.UpdateChargeComponentsColors(false, true);
		}

		private void OnCombatObjecSpawn(CombatObject combatObject, SpawnEvent msg)
		{
			this._isAlive = true;
		}

		private void OnCombatObjecUnspawn(CombatObject combatObject, UnspawnEvent msg)
		{
			this._isAlive = false;
			this.RageComponents.Reset();
			this._chargedAnimationQueue.Clear();
			GUIUtils.PlayAnimation(this.ChargingComponents.Animation, false, 1f, this.ChargingComponents.ChargeResetAnimationName);
		}

		private void SetupOverheatComponents()
		{
			if (this.OverheatComponents.GroupGameObject == null)
			{
				return;
			}
			if (this._kind != GadgetKind.Overheat)
			{
				this.OverheatComponents.GroupGameObject.SetActive(false);
				return;
			}
			this.OverheatComponents.GroupGameObject.SetActive(true);
			this.OverheatComponents.OverheatIcon.gameObject.SetActive(false);
			this.OverheatComponents.OverlayIcon.gameObject.SetActive(false);
			this.OverheatComponents.FlickAnimator.gameObject.SetActive(true);
			this.OverheatComponents.FlickAnimator.SetInteger(this.OverheatComponents.FlickAnimatorPropertyName, 0);
			this.OverheatComponents.OverheatAnimator.gameObject.SetActive(true);
			this.OverheatComponents.OverheatAnimator.SetInteger(this.OverheatComponents.FlickAnimatorPropertyName, 0);
		}

		public void OnDestroy()
		{
			this.RemoveCombatListeners();
			this._gadgetData = null;
			this._gadget = null;
			this._combatData = null;
		}

		private void CheckDisableSpriteState()
		{
			bool flag = this.IsEnabledSpriteState();
			if (flag)
			{
				this.CooldownInfo.SetSpritesEnabled(this.GadgetSprite, this.KeyLabelGroupGameObject.GetComponent<UI2DSprite>(), this.KeyLabel, this.BorderSprite);
			}
			else
			{
				this.CooldownInfo.SetSpritesDisabled(this.GadgetSprite, this.KeyLabelGroupGameObject.GetComponent<UI2DSprite>(), this.KeyLabel, this.BorderSprite);
			}
			this.UpdateChargeComponentsColors(flag, false);
		}

		private void UpdateChargeComponentsColors(bool isEnabled, bool forceUpdate = false)
		{
			bool atMaxCharges = false;
			if (this._chargesParameter != null)
			{
				atMaxCharges = (this._chargesParameter.Data >= this._chargesParameter.Max);
			}
			this._chargesComponents.UpdateColors(isEnabled, atMaxCharges, forceUpdate);
		}

		private bool IsEnabledSpriteState()
		{
			return !this._isDisabled && this._activated && this.IsProgressActive() && this.IsBombDeliveryOrTutorialWarmup() && this.HasCharges() && !this.IsInputBlocked() && !this.IsCoolingAfterOverheat();
		}

		private bool IsInputBlocked()
		{
			return !ControlOptions.IsControlActionUnlocked(this.InputAction);
		}

		private bool IsCoolingAfterOverheat()
		{
			return this._gadgetState == GadgetState.CoolingAfterOverheat;
		}

		private bool IsProgressActive()
		{
			return this.ProgressComponents.IsActive() || (!this._isCountingDown && this._hasEnoughEp);
		}

		private bool HasCharges()
		{
			return this._chargesParameter == null || this._chargesParameter.Max == 0 || this._chargesParameter.Data > 0;
		}

		private bool IsBombDeliveryOrTutorialWarmup()
		{
			bool flag = GameHubBehaviour.Hub.Match.LevelIsTutorial();
			bool flag2 = GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.BombDelivery;
			bool flag3 = GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.Warmup;
			return flag2 || (flag3 && flag);
		}

		public void Activate()
		{
			this._activated = true;
			this._isCountingDown = false;
		}

		public void DeActivate()
		{
			this._activated = false;
			this._isCountingDown = false;
		}

		private void ChangeState()
		{
			switch (this._gadgetState)
			{
			case GadgetState.Toggled:
				if (!this._activated)
				{
					this.Activate();
				}
				else if (this._lastGadgetState == GadgetState.Cooldown)
				{
					if (this.CooldownEndFX != null)
					{
						this.CooldownEndFX.Play(4);
					}
					GUIUtils.PlayAnimation(this.FireGadgetAnimation, true, 1f, string.Empty);
				}
				this.ToggleOn();
				break;
			case GadgetState.Cooldown:
				if (this._kind == GadgetKind.Charged)
				{
					this._chargedMaxHeat = false;
					this._chargedAnimationQueue.Clear();
					this._chargedAnimationQueue.Queue(this.ChargingComponents.Animation, this.ChargingComponents.ChargeLaunchAnimationName, null, 1f);
					this._chargedAnimationQueue.Queue(this.ChargingComponents.Animation, this.ChargingComponents.ChargeResetAnimationName, null, 1f);
				}
				this.StartCountingDown();
				this.ToggleOff();
				break;
			case GadgetState.NotActive:
				if (this._activated)
				{
					this.DeActivate();
				}
				break;
			case GadgetState.Ready:
				if (!this._activated)
				{
					this.Activate();
				}
				else if (this._lastGadgetState == GadgetState.Cooldown)
				{
					if (this.CooldownEndFX != null)
					{
						this.CooldownEndFX.Play(4);
					}
					this.CooldownInfo.SetSpritesEnabled(this.GadgetSprite, this.KeyLabelGroupGameObject.GetComponent<UI2DSprite>(), this.KeyLabel, this.BorderSprite);
					this.UpdateChargeComponentsColors(true, false);
					GUIUtils.PlayAnimation(this.FireGadgetAnimation, true, 1f, string.Empty);
				}
				if (this._kind == GadgetKind.Overheat && this.OverheatComponents.OverheatIcon.gameObject.activeSelf)
				{
					this.OverheatMakeReadyAfterCooling();
				}
				this.ToggleOff();
				break;
			case GadgetState.CoolingAfterOverheat:
				if (this._kind == GadgetKind.Overheat)
				{
					this.OverheatComponents.OverheatIcon.gameObject.SetActive(true);
					this.OverheatComponents.OverlayIcon.gameObject.SetActive(true);
					this.OverheatComponents.OverheatAnimator.SetInteger(this.OverheatComponents.OverheatAnimatorPropertyName, 2);
					this.OverheatComponents.FlickAnimator.SetInteger(this.OverheatComponents.FlickAnimatorPropertyName, 0);
					if (this.OverheatComponents.OverheatAudioFmodAsset != null)
					{
						FMODAudioManager.PlayOneShotAt(this.OverheatComponents.OverheatAudioFmodAsset, Vector3.zero, 0);
					}
				}
				break;
			}
			this._lastGadgetState = this._gadgetState;
			this.CheckDisableSpriteState();
		}

		private void UpdateGadgetState(GadgetData.GadgetStateObject gadgetStateObject)
		{
			if (this.TryUpdateCustomGadgetState())
			{
				return;
			}
			if (gadgetStateObject.GadgetState != this._lastGadgetState)
			{
				this._gadgetState = gadgetStateObject.GadgetState;
				this.ChangeState();
			}
			if (this._kind == GadgetKind.Switch && gadgetStateObject.Value != this._lastGadgetValue)
			{
				this.SwitchGadgetValue(gadgetStateObject.Value);
			}
		}

		private bool TryUpdateCustomGadgetState()
		{
			if (this._customGadget == null)
			{
				return false;
			}
			if (!this._customGadget.HasCooldownParameters())
			{
				return false;
			}
			this._gadgetState = ((this._customGadget.GetCooldownEndTime() <= GameHubBehaviour.Hub.GameTime.GetPlaybackTime()) ? GadgetState.Ready : GadgetState.Cooldown);
			if (this._gadgetState != this._lastGadgetState)
			{
				this.ChangeState();
			}
			return true;
		}

		public void Update()
		{
			if (this._gadgetData == null)
			{
				return;
			}
			GadgetData.GadgetStateObject gadgetState = this._gadgetData.GetGadgetState(this._gadgetSlot);
			this.UpdateGadgetState(gadgetState);
			this.PlayDisarmedAudio();
			bool flag = this._customGadget == null || this._combatData.EP >= (float)this._gadget.Info.EpRequiredToActivate;
			this._hasEnoughEp = (this._combatData.EP >= (float)this._gadget.ActivationCost && flag);
			this.ExecuteControlAction();
			this.CheckDisableSpriteState();
			this._timeMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this._deltaTimeMillis = this._timeMillis - this._lastTimeMillis;
			if (this._deltaTimeMillis == 0L)
			{
				return;
			}
			this.UpdateGadgetHeat(gadgetState);
			this.UpdateCooldownTimer(gadgetState);
			this.ChargingUpdater(gadgetState);
			this._chargedAnimationQueue.Update();
			this.ProgressUpdate();
			this.RageUpdate();
			this.BuffOnSpeedUpdate();
			if (this.CustomParamsUpdate != null)
			{
				this.CustomParamsUpdate();
			}
			this.UpdateCharges();
			this.UpdateCustomParameters();
			this._lastTimeMillis = this._timeMillis;
		}

		private void UpdateCustomParameters()
		{
			for (int i = 0; i < this._customParametersCallback.Length; i++)
			{
				this._customParametersCallback[i].CheckValueChangedAndTriggerCallback();
			}
		}

		private void UpdateCharges()
		{
			bool isEnabled = this.IsEnabledSpriteState();
			this.UpdateChargeComponentsColors(isEnabled, false);
		}

		private void ChargingUpdater(GadgetData.GadgetStateObject gadgetStateObject)
		{
			if (this._kind != GadgetKind.Charged || !this._combatData.IsAlive())
			{
				return;
			}
			GadgetState gadgetState = this._gadgetState;
			if (gadgetState == GadgetState.Ready)
			{
				if (this.ChargingComponents.ProgressBar.Value <= 0f && gadgetStateObject.Heat > 0f)
				{
					this._chargedAnimationQueue.Queue(this.ChargingComponents.Animation, this.ChargingComponents.ChargeStartAnimationName, null, 1f);
				}
				this.ChargingComponents.ProgressBar.Value = gadgetStateObject.Heat;
				if (gadgetStateObject.Heat >= 1f && !this._chargedMaxHeat)
				{
					this._chargedMaxHeat = true;
					this._chargedAnimationQueue.Queue(this.ChargingComponents.Animation, this.ChargingComponents.ChargeDoneAnimationName, null, 1f);
					this._chargedAnimationQueue.Queue(this.ChargingComponents.Animation, this.ChargingComponents.ChargeMaxAnimationName, null, 1f);
				}
			}
		}

		private void UpdateGadgetHeat(GadgetData.GadgetStateObject gadgetStateObject)
		{
			if (this._kind != GadgetKind.Overheat)
			{
				return;
			}
			float num = this.CalcHeat(gadgetStateObject);
			this.OverheatComponents.HeatProgressBar.value = num;
			Color[] heatPhaseColors = this.OverheatComponents.HeatPhaseColors;
			float num2 = 1f / (float)(heatPhaseColors.Length - 1);
			float num3 = num / num2;
			int num4 = Mathf.FloorToInt(num3);
			if (num4 >= heatPhaseColors.Length - 1)
			{
				this.OverheatComponents.HeatAnimatedColor.color = heatPhaseColors[heatPhaseColors.Length - 1];
			}
			else
			{
				float num5 = num3 - (float)num4;
				this.OverheatComponents.HeatAnimatedColor.color = Color.Lerp(heatPhaseColors[num4], heatPhaseColors[num4 + 1], num5);
			}
			switch (this._gadgetState)
			{
			case GadgetState.Toggled:
			case GadgetState.Cooldown:
			case GadgetState.Ready:
				this.OverheatComponents.FlickAnimator.SetInteger(this.OverheatComponents.FlickAnimatorPropertyName, (num >= this.OverheatComponents.ProgressBarFlickStart) ? 2 : 0);
				if (this.OverheatComponents.OverheatIcon.gameObject.activeSelf)
				{
					this.OverheatMakeReadyAfterCooling();
				}
				break;
			case GadgetState.CoolingAfterOverheat:
				if (this.OverheatComponents.DeniedAudioFmodAsset != null && !SpectatorController.IsSpectating && this._inputActionPoller.GetButtonDown(this.InputAction))
				{
					FMODAudioManager.PlayOneShotAt(this.OverheatComponents.DeniedAudioFmodAsset, Vector3.zero, 0);
				}
				break;
			}
		}

		private void UpdateCooldownTimer(GadgetData.GadgetStateObject gadgetStateObject)
		{
			if (this.TryUpdateCustomGadgetCooldownTimer())
			{
				return;
			}
			if (this._gadgetState == GadgetState.Cooldown)
			{
				this._isCountingDown = true;
				float cooldownTimeRemainingSeconds = (float)(gadgetStateObject.Cooldown - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime()) * 0.001f;
				this.UpdateCooldownUiComponents(cooldownTimeRemainingSeconds, this._gadget.Cooldown);
			}
			else if (this._isCountingDown)
			{
				this.DoCooldownEnded();
			}
		}

		private bool TryUpdateCustomGadgetCooldownTimer()
		{
			if (this._customGadget == null)
			{
				return false;
			}
			if (!this._customGadget.HasCooldownParameters())
			{
				return false;
			}
			int cooldownEndTime = this._customGadget.GetCooldownEndTime();
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (cooldownEndTime > playbackTime)
			{
				this._isCountingDown = true;
				float cooldownTimeRemainingSeconds = (float)(cooldownEndTime - GameHubBehaviour.Hub.GameTime.GetPlaybackTime()) * 0.001f;
				float cooldownTotalTime = this._customGadget.GetCooldownTotalTime();
				this.UpdateCooldownUiComponents(cooldownTimeRemainingSeconds, cooldownTotalTime);
			}
			else if (this._isCountingDown)
			{
				this.DoCooldownEnded();
			}
			return true;
		}

		private void UpdateCooldownUiComponents(float cooldownTimeRemainingSeconds, float totalCooldownTimeSeconds)
		{
			int num = (int)cooldownTimeRemainingSeconds;
			float num2 = cooldownTimeRemainingSeconds - (float)num;
			int num3 = (int)(num2 * 10f);
			this.CooldownLabelSeconds.text = num.ToString();
			this.CooldownLabelMillis.text = string.Format(".{0}", num3);
			if (this.OverlaySliderSprite != null)
			{
				float num4 = cooldownTimeRemainingSeconds / totalCooldownTimeSeconds;
				this.OverlaySliderSprite.fillAmount = 1f - num4;
			}
		}

		private void DoCooldownEnded()
		{
			this._isCountingDown = false;
			this.CooldownLabelSeconds.text = " ";
			this.CooldownLabelMillis.text = " ";
			this.PlayCooldownEndSound();
		}

		private float CalcHeat(GadgetData.GadgetStateObject gadgetStateObject)
		{
			if (this._isAlive)
			{
				return gadgetStateObject.Heat;
			}
			float num = (this._gadgetState != GadgetState.CoolingAfterOverheat) ? this._gadget.Info.OverheatCoolingRate : this._gadget.Info.OverheatUnblockRate;
			float num2 = this.OverheatComponents.HeatProgressBar.value - Time.deltaTime * num;
			if (num2 < 0f)
			{
				num2 = 0f;
				this.OverheatMakeReadyAfterCooling();
			}
			return num2;
		}

		private void ExecuteControlAction()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			if (!this._isCountingDown && !this._isDisabled && this._hasEnoughEp)
			{
				return;
			}
			if (!this._inputActionPoller.GetButtonDown(this.InputAction))
			{
				return;
			}
			if (this.ProgressComponents.IsActive())
			{
				return;
			}
			if (this.voiceOverController != null)
			{
				this.voiceOverController.PlayGadgetCooldown(this._gadgetSlot);
			}
			int synchTime = GameHubBehaviour.Hub.GameTime.GetSynchTime();
			if (this.audioCooldownTime < synchTime)
			{
				this.audioCooldownTime = synchTime + this.audioCooldownMillis;
				FMODAudioManager.PlayOneShotAt(this.CooldownAudioFmodAsset, Vector3.zero, 0);
			}
		}

		private void PlayDisarmedAudio()
		{
			if (this._gadgetSlot == GadgetSlot.OutOfCombatGadget)
			{
				return;
			}
			if (!this._isAlive || !this.IsGadgetEnabledByKey())
			{
				this._isDisabled = true;
				return;
			}
			if (!this._combatData.Combat.Attributes.IsGadgetDisarmed(this._gadgetSlot, this._gadgetNature))
			{
				if (this._isDisabled && this.UndisarmedAudioFmodAsset != null)
				{
					FMODAudioManager.PlayOneShotAt(this.UndisarmedAudioFmodAsset, Vector3.zero, 0);
				}
				this._isDisabled = false;
				return;
			}
			if (this._isDisabled)
			{
				return;
			}
			this._isDisabled = true;
			if (this.DisarmedAudioFmodAsset != null)
			{
				FMODAudioManager.PlayOneShotAt(this.DisarmedAudioFmodAsset, Vector3.zero, 0);
			}
			if (this.voiceOverController != null)
			{
				this.voiceOverController.PlayDisarmedVoiceOver();
			}
		}

		private void SwitchGadgetValue(int value)
		{
			this.ProgressComponents.MainGroup.SetActive(false);
			this._lastGadgetValue = value;
			if (value == 0)
			{
				this.GadgetSprite.SpriteName = HudUtils.GetGadgetIconName(this._currentPlayerData, this._gadgetSlot);
			}
			else
			{
				this.GadgetSprite.SpriteName = HudUtils.GetGadgetIconNameB(this._currentPlayerData, this._gadgetSlot);
			}
		}

		private void ToggleOn()
		{
			if (this._kind != GadgetKind.Toggle)
			{
				return;
			}
			this.ProgressComponents.MainGroup.SetActive(true);
			this.ProgressComponents.ProgressBar.value = 1f;
			this.GadgetSprite.SpriteName = HudUtils.GetGadgetIconNameB(this._currentPlayerData, this._gadgetSlot);
		}

		private void ToggleOff()
		{
			if (this._kind != GadgetKind.Toggle)
			{
				return;
			}
			this.ProgressComponents.MainGroup.SetActive(false);
			this.GadgetSprite.SpriteName = HudUtils.GetGadgetIconName(this._currentPlayerData, this._gadgetSlot);
		}

		public void UltimateCustomParameterToggle(UiParameterCallback parameter)
		{
			bool flag = (bool)parameter.CurrentValue;
			if (flag)
			{
				this.GadgetSprite.SpriteName = HudUtils.GetGadgetIconNameB(this._currentPlayerData, this._gadgetSlot);
			}
			else
			{
				this.GadgetSprite.SpriteName = HudUtils.GetGadgetIconName(this._currentPlayerData, this._gadgetSlot);
			}
		}

		public void UfoAttachedGlowToggle(UiParameterCallback parameter)
		{
			if (this._ufoAttachedFeedback == null)
			{
				return;
			}
			bool active = (bool)parameter.CurrentValue;
			this._ufoAttachedFeedback.SetActive(active);
		}

		private void OverheatMakeReadyAfterCooling()
		{
			this.OverheatComponents.OverheatAnimator.SetInteger(this.OverheatComponents.OverheatAnimatorPropertyName, 0);
			this.OverheatComponents.OverheatIcon.gameObject.SetActive(false);
			this.OverheatComponents.OverlayIcon.gameObject.SetActive(false);
			if (this.OverheatComponents.ReadyAfterCoolingAudioFmodAsset != null)
			{
				FMODAudioManager.PlayOneShotAt(this.OverheatComponents.ReadyAfterCoolingAudioFmodAsset, Vector3.zero, 0);
			}
		}

		public void StartCountingDown()
		{
			if (this._activated)
			{
				this.DeActivate();
			}
			this._activated = true;
			this._isCountingDown = true;
			this.CooldownLabelSeconds.text = " ";
			this.CooldownLabelMillis.text = " ";
			if (this._lastGadgetState != GadgetState.Cooldown)
			{
				this.CooldownInfo.SetSpritesDisabled(this.GadgetSprite, this.KeyLabelGroupGameObject.GetComponent<UI2DSprite>(), this.KeyLabel, this.BorderSprite);
				GUIUtils.PlayAnimation(this.FireGadgetAnimation, false, 1f, string.Empty);
			}
		}

		public void UpdateKey(string keyText)
		{
			this._gadgetHasKey = (keyText != 0.ToString());
			if (this.TryDisableShortcutKeysIfSpectator())
			{
				return;
			}
			this.KeyLabel.overflowMethod = UILabel.Overflow.ResizeFreely;
			this.KeyLabel.text = keyText;
			this.KeyLabel.TryUpdateText();
			if (this.KeyLabel.width >= this._keyLabelMaxWidth)
			{
				this.KeyLabel.overflowMethod = UILabel.Overflow.ShrinkContent;
				this.KeyLabel.width = this._keyLabelMaxWidth;
				this.KeyLabel.TryUpdateText();
			}
			this.KeyLabelGroupGameObject.SetActive(true);
			this.KeyLabelGroupGameObject.GetComponent<UI2DSprite>().UpdateAnchors();
			this.KeySprite.gameObject.SetActive(false);
		}

		public void UpdateKey(Sprite keySprite)
		{
			this._gadgetHasKey = true;
			if (this.TryDisableShortcutKeysIfSpectator())
			{
				return;
			}
			this.KeySprite.sprite2D = keySprite;
			this.KeySprite.gameObject.SetActive(true);
			this.KeyLabelGroupGameObject.SetActive(false);
		}

		private bool TryDisableShortcutKeysIfSpectator()
		{
			if (!SpectatorController.IsSpectating)
			{
				return false;
			}
			this.KeyLabelGroupGameObject.SetActive(false);
			this.KeySprite.gameObject.SetActive(false);
			return true;
		}

		private void PlayCooldownEndSound()
		{
			if (this.ActivatedAudioFmodAsset != null)
			{
				FMODAudioManager.PlayOneShotAt(this.ActivatedAudioFmodAsset, Vector3.zero, 0);
			}
			if (this._kind != GadgetKind.InstantWithCharges && this._gadgetSlot != GadgetSlot.CustomGadget2 && this.voiceOverController != null)
			{
				this.voiceOverController.PlayGadgetAvailableVoiceOver(this._gadgetSlot);
			}
		}

		private void TryToSetupCooldown()
		{
			this.ProgressComponents.Hide();
		}

		public void OnVfxActivated(float lifeTime)
		{
			this.ProgressComponents.Start(lifeTime);
			this.GadgetSprite.SpriteName = HudUtils.GetGadgetIconNameB(this._currentPlayerData, this._gadgetSlot);
		}

		public void OnVfxDeactivated()
		{
			this.ProgressComponents.Hide();
			this.GadgetSprite.SpriteName = HudUtils.GetGadgetIconName(this._currentPlayerData, this._gadgetSlot);
		}

		private bool TryToStartProgressFeedback()
		{
			if (!(this._gadget.Info is AlternatingCannonInfo) || this._gadget.LifeTime <= 0f)
			{
				return false;
			}
			this.ProgressComponents.Start(this._gadget.LifeTime);
			this.GadgetSprite.SpriteName = HudUtils.GetGadgetIconNameB(this._currentPlayerData, this._gadgetSlot);
			return true;
		}

		private void ProgressUpdate()
		{
			if (!this.ProgressComponents.IsActive())
			{
				return;
			}
			if (!this.ProgressComponents.Update())
			{
				this.GadgetSprite.SpriteName = HudUtils.GetGadgetIconName(this._currentPlayerData, this._gadgetSlot);
			}
		}

		private void TryToSetupRage()
		{
			this.RageComponents.Setup(this._gadget);
		}

		private void RageUpdate()
		{
			if (this.RageComponents.IsEnabled() && this._isAlive)
			{
				this.RageComponents.UpdateValue(this._gadgetData.GadgetJokeBarState.Value, this._gadgetData.GadgetJokeBarState.MaxValue);
			}
		}

		private void TryToSetupBuffOnSpeed()
		{
			this.BuffOnSpeedComponents.Setup(this._gadget);
		}

		private void BuffOnSpeedUpdate()
		{
			if (this.BuffOnSpeedComponents.IsEnabled())
			{
				this.BuffOnSpeedComponents.Update(this.GetJokeBarValue(), this._gadgetState);
			}
		}

		private float GetJokeBarValue()
		{
			float maxValue = this._gadgetData.GadgetJokeBarState.MaxValue;
			return (Math.Abs(maxValue) >= 0.001f) ? (this._gadgetData.GadgetJokeBarState.Value / maxValue) : 0f;
		}

		private bool IsGadgetEnabledByKey()
		{
			return this._gadgetSlot == GadgetSlot.PassiveGadget || this._gadgetHasKey;
		}

		public HMMUI2DDynamicSprite GadgetSprite;

		public ControllerInputActions InputAction;

		public UILabel CooldownLabelSeconds;

		public UILabel CooldownLabelMillis;

		public UI2DSprite OverlaySliderSprite;

		public UILabel KeyLabel;

		[SerializeField]
		private int _keyLabelMaxWidth = 85;

		public GameObject KeyLabelGroupGameObject;

		public UI2DSprite KeySprite;

		public UI2DSprite BorderSprite;

		private PlayerData _currentPlayerData;

		[Header("[Custom Parameters]")]
		[FormerlySerializedAs("CustomParameters")]
		[SerializeField]
		private UICustomGadgetDataComponent[] _customParameters;

		[SerializeField]
		private UiParameterCallback[] _customParametersCallback;

		[SerializeField]
		private GameObject _ufoAttachedFeedback;

		[Header("[Charges]")]
		[SerializeField]
		private GuiChargesComponent _chargesComponents;

		[Header("[Audio]")]
		[SerializeField]
		private int audioCooldownMillis;

		private int audioCooldownTime;

		public AudioEventAsset CooldownAudioFmodAsset;

		public AudioEventAsset DisarmedAudioFmodAsset;

		public AudioEventAsset UndisarmedAudioFmodAsset;

		public AudioEventAsset ActivatedAudioFmodAsset;

		private VoiceOverController voiceOverController;

		private bool _shownChargeGlow;

		private GadgetBehaviour _gadget;

		private CombatGadget _customGadget;

		private GadgetSlot _gadgetSlot;

		private GadgetNatureKind _gadgetNature;

		private UICustomGadgetDataComponent _chargesParameter;

		private CombatData _combatData;

		private bool _activated;

		private bool _isDisabled;

		private GadgetState _gadgetState;

		private GadgetState _lastGadgetState = GadgetState.None;

		private GadgetData _gadgetData;

		private int _lastGadgetValue;

		private bool _isCountingDown;

		public Animation CooldownEndFX;

		private GadgetKind _kind;

		private bool _hasEnoughEp = true;

		private long _lastTimeMillis;

		private long _timeMillis;

		private long _deltaTimeMillis;

		private bool _chargedMaxHeat;

		private readonly AnimationQueue _chargedAnimationQueue = new AnimationQueue();

		[InjectOnClient]
		private IControllerInputActionPoller _inputActionPoller;

		[Header("[Overheat]")]
		public UIGadgetController.GuiOverheatComponents OverheatComponents;

		[Header("[Charging Components]")]
		[SerializeField]
		public UIGadgetController.GuiChargingComponents ChargingComponents;

		[Header("[Alternating Progress info]")]
		[SerializeField]
		protected UIGadgetController.GuiProgressComponents ProgressComponents;

		[Header("[Rage]")]
		[SerializeField]
		protected UIGadgetController.GuiRageComponents RageComponents;

		[Header("[BuffOnSpeed]")]
		[SerializeField]
		protected UIGadgetController.GuiBuffOnSpeed BuffOnSpeedComponents;

		[Header("[Cooldown]")]
		[SerializeField]
		private UIGadgetController.GuiCooldownInfo CooldownInfo;

		[Header("[Fire Gadget Feedbacks]")]
		[SerializeField]
		private Animation FireGadgetAnimation;

		private bool _isAlive;

		private bool _gadgetHasKey;

		[Serializable]
		public struct GuiOverheatComponents
		{
			[Header("[Main Group]")]
			public GameObject GroupGameObject;

			[Header("[Icons]")]
			public UI2DSprite OverheatIcon;

			public UI2DSprite OverlayIcon;

			[Header("[Colors]")]
			public AnimatedColor HeatAnimatedColor;

			public Color[] HeatPhaseColors;

			[Header("[Progress Bar]")]
			public UIProgressBar HeatProgressBar;

			[Range(0f, 1f)]
			public float ProgressBarFlickStart;

			[Header("[Animators]")]
			public Animator FlickAnimator;

			public string FlickAnimatorPropertyName;

			public Animator OverheatAnimator;

			public string OverheatAnimatorPropertyName;

			[Header("[Audio]")]
			public AudioEventAsset OverheatAudioFmodAsset;

			public AudioEventAsset DeniedAudioFmodAsset;

			public AudioEventAsset ReadyAfterCoolingAudioFmodAsset;
		}

		[Serializable]
		public class GuiChargingComponents
		{
			public ProgressBarAnimationHack ProgressBar;

			public Animation Animation;

			public string ChargeStartAnimationName;

			public string ChargeDoneAnimationName;

			public string ChargeMaxAnimationName;

			public string ChargeLaunchAnimationName;

			public string ChargeResetAnimationName;
		}

		[Serializable]
		public class GuiProgressComponents
		{
			public void Hide()
			{
				if (this.MainGroup == null)
				{
					return;
				}
				this.MainGroup.SetActive(false);
				this.ProgressBar.value = 0f;
				this._timeInSec = 0f;
			}

			public void Start(float time)
			{
				this._maxTimeInSec = time;
				this._timeInSec = time;
				this.MainGroup.SetActive(true);
				this.ProgressBar.value = 0f;
			}

			public bool IsActive()
			{
				return this._timeInSec > 0f;
			}

			public bool Update()
			{
				if (this.MainGroup == null)
				{
					return false;
				}
				if (!this.IsActive())
				{
					return false;
				}
				this._timeInSec -= Time.deltaTime;
				this.ProgressBar.value = 1f - this._timeInSec / this._maxTimeInSec;
				if (this._timeInSec <= 0f)
				{
					this.Hide();
					return false;
				}
				return true;
			}

			public GameObject MainGroup;

			public UIProgressBar ProgressBar;

			private float _maxTimeInSec;

			private float _timeInSec;
		}

		[Serializable]
		public class GuiRageComponents
		{
			public void Setup(GadgetBehaviour gadgetBehaviour)
			{
				this._enabled = false;
				this._rageInfo = null;
				this._value = 0f;
				this._previousRageLevel = 0;
				this._RageLevelDecreaseValue = 0f;
				Rage rage = gadgetBehaviour.Combat.PassiveGadget as Rage;
				if (rage != null && gadgetBehaviour.Slot == GadgetSlot.PassiveGadget)
				{
					this._enabled = true;
					this._rageInfo = (RageInfo)rage.Info;
					if (this._rageInfo.RageValues.Length > 0)
					{
						this._RageLevelIncreaseValue = (float)this._rageInfo.RageValues[0];
					}
					else
					{
						this._RageLevelIncreaseValue = float.MaxValue;
					}
				}
				else
				{
					this._RageLevelIncreaseValue = float.MaxValue;
				}
				if (this.ProgressBar != null)
				{
					this.ProgressBar.value = 0f;
				}
			}

			public bool IsEnabled()
			{
				return this._enabled;
			}

			public void UpdateValue(float jokerBarValue, float jokerBarMaxValue)
			{
				float num = jokerBarValue / jokerBarMaxValue;
				if (jokerBarValue > this._RageLevelIncreaseValue)
				{
					this.RageLevelChanged(this._previousRageLevel + 1);
				}
				else if (jokerBarValue < this._RageLevelDecreaseValue)
				{
					this.RageLevelChanged(this._previousRageLevel - 1);
				}
				float num2 = num * this.MaxOffset + this.MinOffset;
				if (Math.Abs(num2 - this._value) < 0.001f)
				{
					return;
				}
				this._value = num2;
				this.ProgressBar.value = num2;
			}

			private void RageLevelChanged(int newRageLevel)
			{
				if (this._previousRageLevel < newRageLevel)
				{
					this._RageLevelDecreaseValue = (float)this._rageInfo.RageValues[this._previousRageLevel];
					if (newRageLevel >= this._rageInfo.RageValues.Length)
					{
						this._RageLevelIncreaseValue = float.MaxValue;
					}
					else
					{
						this._RageLevelIncreaseValue = (float)this._rageInfo.RageValues[newRageLevel];
					}
					if (this._previousRageLevel < this._increaseAudios.Length)
					{
						FMODAudioManager.PlayOneShotAt(this._increaseAudios[this._previousRageLevel], Vector3.zero, 0);
					}
				}
				else
				{
					this._RageLevelIncreaseValue = (float)this._rageInfo.RageValues[newRageLevel];
					if (newRageLevel == 0)
					{
						this._RageLevelDecreaseValue = 0f;
					}
					else
					{
						this._RageLevelDecreaseValue = (float)this._rageInfo.RageValues[newRageLevel - 1];
					}
					if (newRageLevel < this._decreaseAudios.Length)
					{
						FMODAudioManager.PlayOneShotAt(this._decreaseAudios[newRageLevel], Vector3.zero, 0);
					}
				}
				this._previousRageLevel = newRageLevel;
			}

			internal void Reset()
			{
				if (!this._enabled)
				{
					return;
				}
				this._value = 0f;
				this._previousRageLevel = 0;
				this._RageLevelDecreaseValue = 0f;
				if (this._rageInfo != null && this._rageInfo.RageValues.Length > 0)
				{
					this._RageLevelIncreaseValue = (float)this._rageInfo.RageValues[0];
				}
				else
				{
					this._RageLevelIncreaseValue = float.MaxValue;
				}
				if (this.ProgressBar != null)
				{
					this.ProgressBar.value = 0f;
				}
			}

			[Range(0f, 1f)]
			[SerializeField]
			private float MinOffset;

			[Range(0f, 1f)]
			[SerializeField]
			private float MaxOffset;

			public UIProgressBar ProgressBar;

			private bool _enabled;

			private RageInfo _rageInfo;

			private float _value;

			private int _previousRageLevel;

			private float _RageLevelIncreaseValue;

			private float _RageLevelDecreaseValue;

			[SerializeField]
			private AudioEventAsset[] _increaseAudios;

			[SerializeField]
			private AudioEventAsset[] _decreaseAudios;
		}

		[Serializable]
		public class GuiBuffOnSpeed
		{
			public void Setup(GadgetBehaviour gadgetBehaviour)
			{
				this._enabled = false;
				this._normalizedValue = 0f;
				if (this.SpeedBarGameObject != null)
				{
					this.SpeedBarGameObject.SetActive(false);
				}
				if (!this.Enabled)
				{
					return;
				}
				if (gadgetBehaviour.Combat.PassiveGadget is BuffOnSpeed && gadgetBehaviour.Slot == GadgetSlot.PassiveGadget)
				{
					this._enabled = true;
					this.ProgressBar.gameObject.SetActive(false);
					if (this.SpeedBarGameObject != null)
					{
						this.SpeedBarGameObject.SetActive(true);
					}
				}
				if (this.ProgressBar != null)
				{
					this.ProgressBar.value = 0f;
				}
				if (this.SpeedBarSprite != null)
				{
					this.SpeedBarSprite.fillAmount = 0f;
				}
			}

			public bool IsEnabled()
			{
				return this._enabled;
			}

			public void Update(float normalizedValue, GadgetState gadgetState)
			{
				if (Math.Abs(normalizedValue - this._normalizedValue) < 0.001f && this._gadgetState == gadgetState)
				{
					return;
				}
				this._normalizedValue = normalizedValue;
				this._gadgetState = gadgetState;
				this.ProgressBar.value = normalizedValue;
				this.SpeedBarSprite.fillAmount = normalizedValue;
				if (gadgetState != GadgetState.Ready)
				{
					this.SpeedBarSprite.sprite2D = this.IncreaseSprite;
					this.GlowInnerSprite.gameObject.SetActive(false);
					this.GlowOuterSprite.gameObject.SetActive(false);
					return;
				}
				this.SpeedBarSprite.sprite2D = this.DecaySprite;
				this.GlowInnerSprite.gameObject.SetActive(normalizedValue <= this.GlowDecay);
				this.GlowOuterSprite.gameObject.SetActive(normalizedValue <= this.GlowDecay);
			}

			public bool Enabled;

			public UIProgressBar ProgressBar;

			public GameObject SpeedBarGameObject;

			public UI2DSprite SpeedBarSprite;

			public Sprite IncreaseSprite;

			public Sprite DecaySprite;

			[Range(0f, 1f)]
			public float GlowDecay;

			public UI2DSprite GlowInnerSprite;

			public UI2DSprite GlowOuterSprite;

			private bool _enabled;

			private float _normalizedValue;

			private GadgetState _gadgetState;
		}

		[Serializable]
		private struct GuiCooldownInfo
		{
			public void SetSpritesEnabled(UI2DSprite iconSprite, UI2DSprite shortcutSprite, UILabel shortcutLabel, UI2DSprite borderSprite)
			{
				iconSprite.color = this.IconEnabledColor;
				shortcutSprite.color = this.ShortcutEnabledColor;
				shortcutLabel.color = this.ShortcutEnabledLabelColor;
				borderSprite.color = this.BorderEnabledColor;
			}

			public void SetSpritesDisabled(UI2DSprite iconSprite, UI2DSprite shortcutSprite, UILabel shortcutLabel, UI2DSprite borderSprite)
			{
				iconSprite.color = this.IconDisabledColor;
				shortcutSprite.color = this.ShortcutDisabledColor;
				shortcutLabel.color = this.ShortcutDisabledLabelColor;
				borderSprite.color = this.BorderDisabledColor;
			}

			[Header("[Enabled Colors]")]
			public Color BorderEnabledColor;

			public Color ShortcutEnabledColor;

			public Color ShortcutEnabledLabelColor;

			public Color IconEnabledColor;

			[Header("[Disabled States]")]
			public Color BorderDisabledColor;

			public Color ShortcutDisabledColor;

			public Color ShortcutDisabledLabelColor;

			public Color IconDisabledColor;

			[Header("[Animations]")]
			public Animation InAnimation;

			public Animation OutAnimation;
		}
	}
}
