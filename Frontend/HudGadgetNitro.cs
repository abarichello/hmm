using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using FMod;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Options;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudGadgetNitro : GameHubBehaviour
	{
		public void Start()
		{
			this.NitroProgressBar.value = 0f;
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback += this.OnCurrentPlayerCreated;
			this._isSpectating = SpectatorController.IsSpectating;
		}

		private void TryResetGadget()
		{
			if (this._combatObject != null)
			{
				this._combatObject.ListenToObjectSpawn -= this.OnCombatObjecSpawn;
				this._combatObject.ListenToObjectUnspawn -= this.OnCombatObjecUnspawn;
				this._combatObject.OnCooldownRepairReceived -= this.CombatObject_OnCooldownRepairReceived;
				this._combatObject = null;
			}
			this._gadgetData = null;
		}

		public void OnDestroy()
		{
			this.TryResetGadget();
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback -= this.OnCurrentPlayerCreated;
		}

		private void OnCurrentPlayerCreated(PlayerEvent playerEvent)
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			this.SetupCreatedPlayerCombatObject(GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<CombatObject>());
		}

		public void SetupCreatedPlayerCombatObject(CombatObject combatObject)
		{
			this.TryResetGadget();
			this._combatObject = combatObject;
			this._gadgetData = this._combatObject.Data.gameObject.GetComponent<GadgetData>();
			this._boostGadgetState = GadgetState.None;
			this._combatObject.ListenToObjectSpawn += this.OnCombatObjecSpawn;
			this._combatObject.ListenToObjectUnspawn += this.OnCombatObjecUnspawn;
			this._combatObject.OnCooldownRepairReceived += this.CombatObject_OnCooldownRepairReceived;
			this._isAlive = this._combatObject.IsAlive();
		}

		private void CombatObject_OnCooldownRepairReceived(float obj, int otherId)
		{
			this.glowAnimation.Play();
		}

		private void OnCombatObjecSpawn(CombatObject obj, SpawnEvent msg)
		{
			this._isAlive = true;
		}

		private void OnCombatObjecUnspawn(CombatObject obj, UnspawnEvent msg)
		{
			this._isAlive = false;
		}

		public void Update()
		{
			if (this._combatObject == null)
			{
				return;
			}
			GadgetData.GadgetStateObject gadgetState = this._gadgetData.GetGadgetState(GadgetSlot.BoostGadget);
			bool stateChanged = this._boostGadgetState != gadgetState.GadgetState;
			this._boostGadgetState = gadgetState.GadgetState;
			if (this._isKeyEnabled && !this._isSpectating && (!this._isAlive || this._boostGadgetState != GadgetState.Ready) && this.DeniedAudioFmodAsset != null && ControlOptions.GetButtonDown(ControlAction.GadgetBoost))
			{
				FMODAudioManager.PlayOneShotAt(this.DeniedAudioFmodAsset, Vector3.zero, 0);
			}
			StatusKind currentStatus = this._combatObject.Attributes.CurrentStatus;
			this._isDisabled = (!this._isKeyEnabled || currentStatus.HasFlag(StatusKind.Jammed));
			this.UpdateNitroUiState(gadgetState, stateChanged);
		}

		private void UpdateNitroUiState(GadgetData.GadgetStateObject gadgetStateObject, bool stateChanged)
		{
			bool flag = GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.BombDelivery;
			if (!this._isAlive || !flag)
			{
				this.cooldownGroupGO.SetActive(false);
				this.SetAnimatorNitroUiState(0);
				return;
			}
			if (this._isDisabled)
			{
				this.cooldownGroupGO.SetActive(false);
				this.SetAnimatorNitroUiState(1);
				return;
			}
			if (!ControlOptions.IsControlActionUnlocked(ControlAction.GadgetBoost))
			{
				this.cooldownGroupGO.SetActive(false);
				this.SetAnimatorNitroUiState(0);
				return;
			}
			GadgetState gadgetState = gadgetStateObject.GadgetState;
			if (gadgetState != GadgetState.Ready)
			{
				if (gadgetState != GadgetState.Toggled)
				{
					if (gadgetState == GadgetState.Cooldown)
					{
						if (stateChanged)
						{
							this.cooldownGroupGO.SetActive(true);
						}
						float num = (float)(gadgetStateObject.CoolDown - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime()) * 0.001f;
						num = Mathf.Max(0f, num);
						int num2 = (int)num;
						float num3 = num - (float)num2;
						int num4 = (int)(num3 * 10f);
						this._nitroCooldownLabelSeconds.text = num2.ToString();
						this._nitroCooldownLabelMillis.text = string.Format(".{0}", num4);
					}
				}
				else
				{
					this.SetAnimatorNitroUiState(3);
				}
			}
			else
			{
				this.cooldownGroupGO.SetActive(false);
				if (ControlOptions.GetButton(ControlAction.GadgetBoost))
				{
					if (!this._isSpectating)
					{
						this.SetAnimatorNitroUiState(3);
					}
				}
				else
				{
					this.SetAnimatorNitroUiState(2);
				}
			}
		}

		private void SetAnimatorNitroUiState(int state)
		{
			if (this.NitroAnimator && this.NitroAnimator.gameObject.activeInHierarchy && this.NitroAnimator.gameObject.activeSelf)
			{
				this.NitroAnimator.SetInteger("NitroUIState", state);
			}
		}

		public void UpdateKey(string keyText)
		{
			this._isKeyEnabled = (keyText != KeyCode.None.ToString());
			if (this.TryDisableShortcutKeysIfSpectator())
			{
				return;
			}
			this.KeyLabel.text = keyText;
			this.KeyLabel.gameObject.SetActive(true);
			this.KeySprite.gameObject.SetActive(false);
		}

		public void UpdateKey(Sprite keySprite)
		{
			this._isKeyEnabled = true;
			if (this.TryDisableShortcutKeysIfSpectator())
			{
				return;
			}
			this.KeySprite.sprite2D = keySprite;
			this.KeySprite.gameObject.SetActive(true);
			this.KeyLabel.gameObject.SetActive(false);
		}

		private bool TryDisableShortcutKeysIfSpectator()
		{
			if (!SpectatorController.IsSpectating)
			{
				return false;
			}
			this.KeyLabel.gameObject.SetActive(false);
			this.KeySprite.gameObject.SetActive(false);
			return true;
		}

		private const string NitroUiStateAnimatorField = "NitroUIState";

		public UIProgressBar NitroProgressBar;

		public Animator NitroAnimator;

		[SerializeField]
		private Animation glowAnimation;

		[SerializeField]
		private UILabel _nitroCooldownLabelSeconds;

		[SerializeField]
		private UILabel _nitroCooldownLabelMillis;

		[SerializeField]
		private GameObject cooldownGroupGO;

		[Header("[Controls]")]
		public ControlAction ControlAction;

		public UILabel KeyLabel;

		public UI2DSprite KeySprite;

		[Header("[Audio]")]
		public FMODAsset DeniedAudioFmodAsset;

		public FMODAsset ActivatedAudioFmodAsset;

		private GadgetState _boostGadgetState;

		private float _boostRemainingCooldownTimeMillis;

		private GadgetData _gadgetData;

		private CombatObject _combatObject;

		private bool _isAlive;

		private bool _isDisabled;

		private bool _isKeyEnabled;

		private bool _isSpectating;
	}
}
