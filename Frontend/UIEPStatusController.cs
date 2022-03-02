using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using FMod;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class UIEPStatusController : UIBaseStatus
	{
		protected override void SetupListeners()
		{
			this._combatData.OnEPChanged += this.OnEpChanged;
			this._combatData.OnLevelChanged += this.OnLevelChanged;
			this._combatData.Combat.Attributes.OnStreamApplied += this.OnCombatAttributesStreamApplied;
			this.UpdateInterface();
			this.UpdateTheVOController();
		}

		public void UpdateCombatDataOnSpectatorMode(CombatData combatData)
		{
			if (!SpectatorController.IsSpectating)
			{
				return;
			}
			this.RemoveListeners();
			this._combatData = combatData;
			this.SetupListeners();
			this.UpdateTheVOController();
		}

		protected override void RemoveListeners()
		{
			if (this._combatData == null)
			{
				return;
			}
			this._combatData.OnEPChanged -= this.OnEpChanged;
			this._combatData.OnLevelChanged -= this.OnLevelChanged;
			this._combatData.Combat.Attributes.OnStreamApplied -= this.OnCombatAttributesStreamApplied;
		}

		private void OnLevelChanged()
		{
			this.UpdateInterface();
		}

		private void OnEpChanged(float hpValue)
		{
			this.UpdateInterface();
		}

		private void OnCombatAttributesStreamApplied()
		{
			this.UpdateInterface();
		}

		protected override void UpdateInterface()
		{
			int epmax = this._combatData.EPMax;
			float ep = this._combatData.EP;
			int activationCost = this._combatData.Combat.CustomGadget2.Info.ActivationCost;
			int epRequiredToActivate = this._combatData.Combat.CustomGadget2.Info.EpRequiredToActivate;
			this.statusBar.fillAmount = ep / (float)epmax;
			bool flag = ep >= (float)activationCost && (epRequiredToActivate <= 0 || ep >= (float)epRequiredToActivate);
			if (flag)
			{
				if (this._playSound)
				{
					this._playSound = false;
					this.PlayCooldownEndSound();
				}
				if (this._animateTransition)
				{
					this._animateTransition = false;
					this.SetUIGadgetAnimatorState(2);
				}
			}
			else
			{
				if (!this._animateTransition)
				{
					this.SetUIGadgetAnimatorState(1);
				}
				this._animateTransition = true;
				this._playSound = true;
			}
		}

		private void PlayCooldownEndSound()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				if (this.EnoughEpAudioFmodAsset == null)
				{
					return;
				}
				FMODAudioManager.PlayOneShotAt(this.EnoughEpAudioFmodAsset, Vector3.zero, 0);
				if (this._voiceOverController != null)
				{
					this._voiceOverController.PlayGadgetAvailableVoiceOver(GadgetSlot.CustomGadget2);
				}
			}
		}

		private void UpdateTheVOController()
		{
			this._voiceOverController = this._combatData.Combat.Player.CharacterInstance.GetBitComponent<VoiceOverController>();
		}

		private void SetUIGadgetAnimatorState(int state)
		{
			if (this.GadgetAnimator)
			{
				this.GadgetAnimator.SetInteger("GadgetUIState", state);
			}
		}

		public void ResetGadgetStats()
		{
			this._playSound = false;
		}

		private bool _animateTransition;

		private const string UiStateAnimatorField = "GadgetUIState";

		public Animator GadgetAnimator;

		[Header("[Audio]")]
		public AudioEventAsset EnoughEpAudioFmodAsset;

		private bool _playSound;

		private VoiceOverController _voiceOverController;
	}
}
