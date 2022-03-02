using System;
using Assets.Standard_Assets.Scripts.HMM.SFX;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input;
using Hoplon.Input.Business;
using Pocketverse;
using Pocketverse.MuralContext;
using UniRx;
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
			this._inputBindNotifierDisposable = ObservableExtensions.Subscribe<int>(Observable.Do<int>(this._inputBindNotifier.ObserveBind(), delegate(int actionId)
			{
				this.OnKeyChangedCallback(actionId);
			}));
			this._inputBindResetDefaultNotifierDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._inputBindNotifier.ObserveResetDefault(), delegate(Unit _)
			{
				this.ReloadKeys();
			}));
			this._inputActiveDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(this._inputActiveDeviceChangeNotifier.ObserveActiveDeviceChange(), delegate(InputDevice activeDevice)
			{
				this.ReloadKeys();
			}));
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			if (this._inputBindNotifierDisposable != null)
			{
				this._inputBindNotifierDisposable.Dispose();
				this._inputBindNotifierDisposable = null;
			}
			if (this._inputBindResetDefaultNotifierDisposable != null)
			{
				this._inputBindResetDefaultNotifierDisposable.Dispose();
				this._inputBindResetDefaultNotifierDisposable = null;
			}
			if (this._inputActiveDeviceChangeNotifierDisposable != null)
			{
				this._inputActiveDeviceChangeNotifierDisposable.Dispose();
				this._inputActiveDeviceChangeNotifierDisposable = null;
			}
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
			Object.DestroyImmediate(base.gameObject);
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
			this.OnKeyChangedCallback(5);
			this.OnKeyChangedCallback(6);
			this.OnKeyChangedCallback(15);
			this.OnKeyChangedCallback(7);
		}

		private void OnKeyChangedCallback(ControllerInputActions controlAction)
		{
			if (!this.IsValidActionId(controlAction))
			{
				return;
			}
			ISprite sprite;
			string keyText;
			if (this._inputTranslation.TryToGetInputActionActiveDeviceAssetOrFallbackToTranslation(controlAction, ref sprite, ref keyText))
			{
				Sprite sprite2 = (sprite as UnitySprite).GetSprite();
				this.SetSpriteKey(controlAction, sprite2);
			}
			else
			{
				this.SetStringKey(controlAction, keyText);
			}
		}

		private bool IsValidActionId(ControllerInputActions controlAction)
		{
			return controlAction == 5 || controlAction == 6 || controlAction == 7 || controlAction == 15;
		}

		private void SetStringKey(ControllerInputActions controlAction, string keyText)
		{
			if (controlAction == 5)
			{
				this.CustomGadget0.UpdateKey(keyText);
			}
			else if (controlAction == 6)
			{
				this.CustomGadget1.UpdateKey(keyText);
			}
			else if (controlAction == 7)
			{
				this.CustomGadget2.UpdateKey(keyText);
			}
			else if (controlAction == 15)
			{
				this.NitroGadget.UpdateKey(keyText);
			}
		}

		private void SetSpriteKey(ControllerInputActions controlAction, Sprite sprite)
		{
			if (controlAction == 5)
			{
				this.CustomGadget0.UpdateKey(sprite);
			}
			else if (controlAction == 6)
			{
				this.CustomGadget1.UpdateKey(sprite);
			}
			else if (controlAction == 7)
			{
				this.CustomGadget2.UpdateKey(sprite);
			}
			else if (controlAction == 15)
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

		private static readonly BitLogger Log = new BitLogger(typeof(UIGadgetConstructor));

		public UIGadgetController CustomGadget0;

		public UIGadgetController CustomGadget1;

		public UIGadgetController CustomGadget2;

		public UIEPStatusController UlUiepStatusController;

		public UIGadgetController BoostGadget;

		public UIGadgetController NitroGadget;

		[SerializeField]
		private GadgetFeedback _sprayGadgetFeedback;

		private CombatData _currentPlayerCombatData;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[InjectOnClient]
		private IInputBindNotifier _inputBindNotifier;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		private IDisposable _inputBindNotifierDisposable;

		private IDisposable _inputBindResetDefaultNotifierDisposable;

		private IDisposable _inputActiveDeviceChangeNotifierDisposable;
	}
}
