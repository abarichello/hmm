using System;
using Assets.Standard_Assets.Scripts.HMM.SFX;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Utils;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class UIGadgetConstructor : HudWindow, ICleanupListener, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		public CombatData CombatData
		{
			get
			{
				return this._currentPlayerCombatData;
			}
			set
			{
				this._currentPlayerCombatData = value;
			}
		}

		private void Start()
		{
			GameHubBehaviour.Hub.Options.Controls.OnKeyChangedCallback += this.OnKeyChangedCallback;
			GameHubBehaviour.Hub.Options.Controls.OnResetDefaultCallback += this.OnKeyDefaultReset;
			GameHubBehaviour.Hub.Options.Controls.OnResetPrimaryDefaultCallback += this.OnKeyDefaultReset;
			GameHubBehaviour.Hub.Options.Controls.OnResetSecondaryDefaultCallback += this.OnKeyDefaultReset;
			GameHubBehaviour.Hub.GuiScripts.Esc.OnControlModeChangedCallback += this.OnControlModeChangedCallback;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			GameHubBehaviour.Hub.Options.Controls.OnKeyChangedCallback -= this.OnKeyChangedCallback;
			GameHubBehaviour.Hub.Options.Controls.OnResetDefaultCallback -= this.OnKeyDefaultReset;
			GameHubBehaviour.Hub.Options.Controls.OnResetPrimaryDefaultCallback -= this.OnKeyDefaultReset;
			GameHubBehaviour.Hub.Options.Controls.OnResetSecondaryDefaultCallback -= this.OnKeyDefaultReset;
			GameHubBehaviour.Hub.GuiScripts.Esc.OnControlModeChangedCallback -= this.OnControlModeChangedCallback;
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			if (this._currentPlayerCombatData == null)
			{
				base.ChangeWindowVisibility(false);
				return;
			}
			base.ChangeWindowVisibility(visible);
		}

		public void OnCleanup(CleanupMessage msg)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}

		public void OnPlayerBuildComplete(PlayerBuildComplete evt)
		{
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData == null || GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance == null || evt.Id != GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId)
			{
				return;
			}
			CombatData component = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetComponent<CombatData>();
			this.UlUiepStatusController.SetupOnCurrentPlayerBuildComplete(component);
			this.SetCombatDataAndPopulateUI(component);
		}

		public void SetCombatDataAndPopulateUI(CombatData targetPlayerCombatData)
		{
			if (this._currentPlayerCombatData == targetPlayerCombatData)
			{
				return;
			}
			this._currentPlayerCombatData = targetPlayerCombatData;
			if (this._currentPlayerCombatData == null)
			{
				base.SetWindowVisibility(false);
				return;
			}
			if (!GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				if (GameHubBehaviour.Hub.GuiScripts.LoadingVersus.IsLoading)
				{
					if (this.IsWindowVisible())
					{
						base.SetWindowVisibility(false);
					}
				}
				else if (!this.IsWindowVisible())
				{
					base.SetWindowVisibility(true);
				}
			}
			this.UlUiepStatusController.ResetGadgetStats();
			GadgetData gadgetData = this._currentPlayerCombatData.GadgetData;
			this.CustomGadget0.Setup(this._currentPlayerCombatData.Combat.Player, gadgetData, this._currentPlayerCombatData.Combat.CustomGadget0, GadgetSlot.CustomGadget0);
			this.CustomGadget1.Setup(this._currentPlayerCombatData.Combat.Player, gadgetData, this._currentPlayerCombatData.Combat.CustomGadget1, GadgetSlot.CustomGadget1);
			this.CustomGadget2.Setup(this._currentPlayerCombatData.Combat.Player, gadgetData, this._currentPlayerCombatData.Combat.CustomGadget2, GadgetSlot.CustomGadget2);
			this.NitroGadget.Setup(this._currentPlayerCombatData.Combat.Player, gadgetData, this._currentPlayerCombatData.Combat.BoostGadget, GadgetSlot.BoostGadget);
			this.UlUiepStatusController.UpdateCombatDataOnSpectatorMode(this._currentPlayerCombatData);
			this.BoostGadget.Setup(this._currentPlayerCombatData.Combat.Player, gadgetData, this._currentPlayerCombatData.Combat.PassiveGadget, GadgetSlot.PassiveGadget);
			this._sprayGadgetFeedback.Setup(this._currentPlayerCombatData.Combat.Player, gadgetData, this._currentPlayerCombatData.Combat.SprayGadget, GadgetSlot.SprayGadget);
			this.ReloadKeys();
		}

		private void ReloadKeys()
		{
			this.OnKeyChangedCallback(this.CustomGadget0.ControlAction);
			this.OnKeyChangedCallback(this.CustomGadget1.ControlAction);
			this.OnKeyChangedCallback(this.CustomGadget2.ControlAction);
			this.OnKeyChangedCallback(this.NitroGadget.ControlAction);
		}

		private void OnKeyDefaultReset()
		{
			this.ReloadKeys();
		}

		private void OnControlModeChangedCallback(CarInput.DrivingStyleKind drivingStyleKind)
		{
			this.ReloadKeys();
		}

		private void OnKeyChangedCallback(ControlAction controlAction)
		{
			KeyCode keyCode;
			if (ControlOptions.IsMouseInput(controlAction, out keyCode))
			{
				Sprite sprite = null;
				switch (keyCode)
				{
				case KeyCode.Mouse0:
					sprite = this.Mouse0Sprite;
					break;
				case KeyCode.Mouse1:
					sprite = this.Mouse1Sprite;
					break;
				case KeyCode.Mouse2:
					sprite = this.Mouse2Sprite;
					break;
				default:
					HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("UIGadgetContructor.OnKeyChangedCallback - Invalid mouse input [{0}]", keyCode), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
					break;
				}
				this.SetSpriteKey(controlAction, sprite);
			}
			else if (ControlOptions.IsUsingControllerJoystick(GameHubBehaviour.Hub))
			{
				Sprite joystickShortcutIcon = GameHubBehaviour.Hub.GuiScripts.JoystickShortcutIcons.GetJoystickShortcutIcon(ControlOptions.GetText(controlAction, ControlOptions.ControlActionInputType.Secondary));
				this.SetSpriteKey(controlAction, joystickShortcutIcon);
			}
			else
			{
				string textlocalized = ControlOptions.GetTextlocalized(controlAction, ControlOptions.ControlActionInputType.Primary);
				if (controlAction == this.CustomGadget0.ControlAction)
				{
					this.CustomGadget0.UpdateKey(textlocalized);
				}
				else if (controlAction == this.CustomGadget1.ControlAction)
				{
					this.CustomGadget1.UpdateKey(textlocalized);
				}
				else if (controlAction == this.CustomGadget2.ControlAction)
				{
					this.CustomGadget2.UpdateKey(textlocalized);
				}
				else if (controlAction == this.NitroGadget.ControlAction)
				{
					this.NitroGadget.UpdateKey(textlocalized);
				}
			}
		}

		private void SetSpriteKey(ControlAction controlAction, Sprite sprite)
		{
			if (controlAction == this.CustomGadget0.ControlAction)
			{
				this.CustomGadget0.UpdateKey(sprite);
			}
			else if (controlAction == this.CustomGadget1.ControlAction)
			{
				this.CustomGadget1.UpdateKey(sprite);
			}
			else if (controlAction == this.CustomGadget2.ControlAction)
			{
				this.CustomGadget2.UpdateKey(sprite);
			}
			else if (controlAction == this.NitroGadget.ControlAction)
			{
				this.NitroGadget.UpdateKey(sprite);
			}
		}

		public void OnVfxActivated(GadgetSlot gadgetSlot, float lifeTime)
		{
			if (gadgetSlot != GadgetSlot.CustomGadget0)
			{
				if (gadgetSlot != GadgetSlot.CustomGadget1)
				{
					if (gadgetSlot == GadgetSlot.CustomGadget2)
					{
						this.CustomGadget2.OnVfxActivated(lifeTime);
					}
				}
				else
				{
					this.CustomGadget1.OnVfxActivated(lifeTime);
				}
			}
			else
			{
				this.CustomGadget0.OnVfxActivated(lifeTime);
			}
		}

		public void OnVfxDeactivated(GadgetSlot gadgetSlot)
		{
			if (gadgetSlot != GadgetSlot.CustomGadget0)
			{
				if (gadgetSlot != GadgetSlot.CustomGadget1)
				{
					if (gadgetSlot == GadgetSlot.CustomGadget2)
					{
						this.CustomGadget2.OnVfxDeactivated();
					}
				}
				else
				{
					this.CustomGadget1.OnVfxDeactivated();
				}
			}
			else
			{
				this.CustomGadget0.OnVfxDeactivated();
			}
		}

		public static bool TryToGetUiGadgetConstructor(out UIGadgetConstructor uiGadgetConstructor)
		{
			uiGadgetConstructor = null;
			Game game = GameHubBehaviour.Hub.State.Current as Game;
			if (game == null)
			{
				UIGadgetConstructor.Log.Warn("Couldn't find game state.");
				return false;
			}
			GameGui stateGuiController = game.GetStateGuiController<GameGui>();
			if (stateGuiController == null)
			{
				UIGadgetConstructor.Log.Warn("Couldn't find gameGUI.");
				return false;
			}
			uiGadgetConstructor = stateGuiController.UIGadgetConstructor;
			return true;
		}

		public void ResetGadgetState()
		{
			this.CustomGadget0.ResetGadgetState();
			this.CustomGadget1.ResetGadgetState();
			this.CustomGadget2.ResetGadgetState();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(UIGadgetConstructor));

		public UIGadgetController CustomGadget0;

		public UIGadgetController CustomGadget1;

		public UIGadgetController CustomGadget2;

		public UIEPStatusController UlUiepStatusController;

		public UIGadgetController BoostGadget;

		public UIGadgetController NitroGadget;

		[SerializeField]
		private GadgetFeedback _sprayGadgetFeedback;

		public Sprite Mouse0Sprite;

		public Sprite Mouse1Sprite;

		public Sprite Mouse2Sprite;

		private CombatData _currentPlayerCombatData;
	}
}
