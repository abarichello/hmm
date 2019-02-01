using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PlayerPingGUI : GameHubBehaviour
	{
		private void Start()
		{
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged += this.ListenToBombCarrierChanged;
			}
			this.SetButtonsState(this.ButtonsForEnemyState, false);
			this.SetButtonsState(this.ButtonsForMyTeamState, false);
			this.SetButtonsState(this.ButtonsForCurrentPlayerState, false);
			this._currentButtonsState = this.ButtonsForEnemyState;
			this.ChangeQuickChatState(PlayerPingGUI.BombCarrierState.None);
			this._degreesForEachButton = 360f / (float)this.ButtonsForEnemyState.Count;
			this._lastObjectIndex = -1;
		}

		private void OnDisable()
		{
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged -= this.ListenToBombCarrierChanged;
			}
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreBoard.State.BombDelivery || SpectatorController.IsSpectating || GameHubBehaviour.Hub.GuiScripts.Esc.IsWindowVisible() || GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.IsWindowVisible() || !this.IsPlayerAlive())
			{
				if (this._active)
				{
					this._active = false;
					this.CloseWindow();
				}
				return;
			}
			bool button = ControlOptions.GetButton(ControlAction.Ping);
			if (button && !this._active)
			{
				this._active = true;
				this.OpenWindow();
			}
			else if (!button && this._active)
			{
				if (UICamera.hoveredObject != null)
				{
					int currentTouchID = UICamera.currentTouchID;
					UICamera.currentTouchID = -1;
					UICamera.Notify(UICamera.hoveredObject, "OnClick", null);
					UICamera.currentTouchID = currentTouchID;
				}
				this._active = false;
				this.CloseWindow();
			}
			if (this._active)
			{
				UICamera.MouseOrTouch mouse = UICamera.GetMouse(0);
				if (mouse != null && Input.GetMouseButton(0) && UICamera.hoveredObject != mouse.current)
				{
					if (this.CancelButton == mouse.current)
					{
						this.SetSelectedObject(this.CancelButton);
					}
					else
					{
						for (int i = 0; i < this._currentButtonsState.Count; i++)
						{
							if (this._currentButtonsState[i].gameObject == mouse.current)
							{
								this.SetSelectedObject(mouse.current);
								break;
							}
						}
					}
				}
			}
			if (GameHubBehaviour.Hub.Input.CurrentController && GameHubBehaviour.Hub.Input.CurrentController.Inputs.DrivingStyle == CarInput.DrivingStyleKind.Controller && this._active)
			{
				this.OnJoyStickInput();
			}
		}

		private bool IsPlayerAlive()
		{
			if (this._combatObject == null)
			{
				PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
				if (currentPlayerData == null || currentPlayerData.IsNarrator || currentPlayerData.CharacterInstance == null)
				{
					return false;
				}
				this._combatObject = currentPlayerData.CharacterInstance.GetBitComponent<CombatObject>();
			}
			return this._combatObject.IsAlive();
		}

		private void OpenWindow()
		{
			if (!this.Panel.gameObject.activeSelf)
			{
				this.Panel.gameObject.SetActive(true);
				GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.OptionsCursor);
			}
			this.SetSelectedObject(null);
		}

		private void CloseWindow()
		{
			this.SetSelectedObject(null);
			if (this.Panel.gameObject.activeSelf)
			{
				this.Panel.gameObject.SetActive(false);
				GameHubBehaviour.Hub.CursorManager.Pop();
			}
		}

		public void GUICreatePing(int pingKind)
		{
			if (!this._active)
			{
				return;
			}
			this.CloseWindow();
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.Events.PlayerPing.Dispatch(new byte[0]).ServerCreatePing(pingKind);
			}
		}

		private void ListenToBombCarrierChanged(CombatObject carrier)
		{
			if (carrier == null)
			{
				this.ChangeQuickChatState(PlayerPingGUI.BombCarrierState.None);
			}
			else if (GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId == carrier.Id.ObjId)
			{
				this.ChangeQuickChatState(PlayerPingGUI.BombCarrierState.CurrentPlayer);
			}
			else if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == carrier.Team)
			{
				this.ChangeQuickChatState(PlayerPingGUI.BombCarrierState.MyTeam);
			}
			else
			{
				this.ChangeQuickChatState(PlayerPingGUI.BombCarrierState.EnemyTeam);
			}
		}

		private void ChangeQuickChatState(PlayerPingGUI.BombCarrierState state)
		{
			switch (state)
			{
			case PlayerPingGUI.BombCarrierState.None:
			case PlayerPingGUI.BombCarrierState.EnemyTeam:
				this.SetButtonsState(this._currentButtonsState, false);
				this._currentButtonsState = this.ButtonsForEnemyState;
				this.SetButtonsState(this._currentButtonsState, true);
				break;
			case PlayerPingGUI.BombCarrierState.CurrentPlayer:
				this.SetButtonsState(this._currentButtonsState, false);
				this._currentButtonsState = this.ButtonsForCurrentPlayerState;
				this.SetButtonsState(this._currentButtonsState, true);
				break;
			case PlayerPingGUI.BombCarrierState.MyTeam:
				this.SetButtonsState(this._currentButtonsState, false);
				this._currentButtonsState = this.ButtonsForMyTeamState;
				this.SetButtonsState(this._currentButtonsState, true);
				break;
			}
		}

		private void SetButtonsState(List<UIButton> buttons, bool pActive)
		{
			for (int i = 0; i < buttons.Count; i++)
			{
				UIButton uibutton = buttons[i];
				uibutton.transform.parent.gameObject.SetActive(pActive);
			}
		}

		private void OnJoyStickInput()
		{
			this._axis1 = Input.GetAxis("Joy1 Axis 4");
			this._axis2 = Input.GetAxis("Joy1 Axis 5");
			if (Math.Abs(this._axis1 - this._lastAxis1) < 0.01f && Math.Abs(this._axis2 - this._lastAxis2) < 0.01f)
			{
				return;
			}
			this._lastAxis1 = this._axis1;
			this._lastAxis2 = this._axis2;
			if (Math.Abs(this._axis1) < 0.01f && Math.Abs(this._axis2) < 0.01f)
			{
				this.SetSelectedObject(this.CancelButton);
				this._lastObjectIndex = -1;
				return;
			}
			float num = Mathf.Atan2(this._axis1, this._axis2);
			this._degrees = num * 57.29578f;
			int num2 = Mathf.Abs((int)((this._degrees - 180f) / this._degreesForEachButton));
			if (num2 == this._lastObjectIndex)
			{
				return;
			}
			this._lastObjectIndex = num2;
			this.SetSelectedObject(this._currentButtonsState[num2].gameObject);
		}

		private void SetSelectedObject(GameObject selectedButton)
		{
			UICamera.selectedObject = selectedButton;
			UICamera.hoveredObject = selectedButton;
			UICamera.controller.current = selectedButton;
		}

		public UIPanel Panel;

		public GameObject PlayerAskTheBomb;

		public GameObject PlayerWillDropTheBomb;

		public GameObject ProtectTheBomb;

		public GameObject GetTheBomb;

		public GameObject CancelButton;

		public List<UIButton> ButtonsForEnemyState = new List<UIButton>(10);

		public List<UIButton> ButtonsForMyTeamState = new List<UIButton>(10);

		public List<UIButton> ButtonsForCurrentPlayerState = new List<UIButton>(10);

		private List<UIButton> _currentButtonsState;

		private bool _active;

		private float _axis1;

		private float _axis2;

		private float _lastAxis1;

		private float _lastAxis2;

		private float _degrees;

		private float _degreesForEachButton;

		private int _lastObjectIndex = -1;

		private CombatObject _combatObject;

		public enum BombCarrierState
		{
			None,
			CurrentPlayer,
			MyTeam,
			EnemyTeam
		}
	}
}
