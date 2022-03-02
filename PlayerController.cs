using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.MuteSystem;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Options.Presenting;
using HeavyMetalMachines.Respawn;
using Hoplon.GadgetScript;
using Hoplon.Input;
using Hoplon.Input.Business;
using Hoplon.Math;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines
{
	public class PlayerController : GameHubBehaviour, IObjectSpawnListener, IPlayerController, ICurrentPlayerController
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ServerListenToReverseUse;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CancelActionListener ListenToCancelAction;

		private GameGui CachedGameGui
		{
			get
			{
				if (null == this._cachedGameGui)
				{
					SceneStateData currentSceneStateData = GameHubBehaviour.Hub.State.GetCurrentSceneStateData();
					if (null == currentSceneStateData)
					{
						return null;
					}
					this._cachedGameGui = (currentSceneStateData.StateGuiController as GameGui);
				}
				return this._cachedGameGui;
			}
		}

		public bool MovingCar
		{
			get
			{
				return this.Inputs.Dir != Vector2.zero;
			}
		}

		public bool AcceleratingForward
		{
			get
			{
				return (this.Inputs.IsControllerActive() && this.Inputs.Dir != Vector2.zero) || (this.Inputs.IsFollowMouseActive() && this.Inputs.Up);
			}
		}

		public Vector3 MousePosition
		{
			get
			{
				return this.Inputs.MousePos;
			}
		}

		private void Awake()
		{
			if (!this.Combat)
			{
				this.Combat = base.Id.GetComponent<CombatObject>();
			}
			if (!this.CarInput)
			{
				this.CarInput = base.Id.GetBitComponent<CarInput>();
			}
			if (!this.Gadgetdata)
			{
				this.Gadgetdata = base.Id.GetBitComponent<GadgetData>();
			}
			this.DrivingStylesUsed = new bool[Enum.GetValues(typeof(CarInput.DrivingStyleKind)).Length];
		}

		private void OnEnable()
		{
			if (!GameHubBehaviour.Hub || (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest()) || (this._startRan && !this._isCurrentPlayer))
			{
				Debug.Log(string.Concat(new object[]
				{
					"Disabling PlayerController ",
					GameHubBehaviour.Hub.Net.IsServer(),
					" ",
					this._startRan && !this._isCurrentPlayer
				}));
				base.enabled = false;
				return;
			}
		}

		private void OnDestroy()
		{
			this.ListenToCancelAction = null;
			this.ListenToGadgetInputPressed = null;
			if (GameHubBehaviour.Hub.Net.IsServer() && !this.Combat.Player.IsBot)
			{
				this.Combat.Player.ServerListenToPlayerDisconnected -= this.ServerListenToPlayerDisconnected;
				this.Combat.Player.ServerListenToPlayerReconnected -= this.ServerListenToPlayerReconnected;
			}
		}

		public void ServerPlayerCarFactoryInit()
		{
			if (!GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			if (!this.Combat.Player.IsBot)
			{
				this.Combat.Player.ServerListenToPlayerDisconnected += this.ServerListenToPlayerDisconnected;
				this.Combat.Player.ServerListenToPlayerReconnected += this.ServerListenToPlayerReconnected;
			}
		}

		private void ServerClearInterfaceBooleans()
		{
			this.ShopInterfaceOpen = false;
			this.HudTabInterfaceOpen = false;
		}

		private void ServerListenToPlayerDisconnected()
		{
			this.ServerClearInterfaceBooleans();
		}

		public void ActivateBotController()
		{
			PlayerController.Log.DebugFormat("ActivateBotController: {0}", new object[]
			{
				base.name
			});
			this.CarHub.AIAgent.GoalManager.BotAIController.Spawned = this.Combat.IsAlive();
			this.EnableBotControllerComponents(true);
			this.CarHub.AIAgent.GoalManager.Initialize();
			this.CarHub.AIAgent.GoalManager.UpdateBotOnlyTeamGoalCap();
			this.CarHub.AIAgent.GoalManager.UpdateAllBotOnlyTeamGoalCap();
			this.CarHub.AIAgent.GoalManager.ReThinkState();
			IHMMGadgetContext gadgetContext = this.CarHub.combatObject.GetGadgetContext(3);
			if (gadgetContext != null)
			{
				IParameter<bool> uiparameter = gadgetContext.GetUIParameter<bool>("IsAttached");
				if (uiparameter != null && uiparameter.GetValue(gadgetContext))
				{
					this.CarHub.combatObject.CustomGadget2.Pressed = true;
				}
			}
			GameHubBehaviour.Hub.Server.SpreadInfo();
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.BotControllerActivated,
				Killer = this.Combat.Id.ObjId
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		public void DeactivateBotController()
		{
			PlayerController.Log.DebugFormat("DeactivateBotController: {0}", new object[]
			{
				base.name
			});
			this.EnableBotControllerComponents(false);
			this.CarHub.AIAgent.GoalManager.UpdateBotOnlyTeamGoalCap();
			this.CarHub.AIAgent.GoalManager.UpdateAllBotOnlyTeamGoalCap();
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.BotControllerDeactivated,
				Killer = this.Combat.Id.ObjId
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		public void EnableBotControllerComponents(bool enable)
		{
			this.CarHub.AIAgent.PathFind.enabled = enable;
			this.CarHub.AIAgent.GoalManager.BotAIController.SetEnabled(enable);
			this.CarHub.Player.IsBotControlled = enable;
			if (!this.CarHub.Player.IsBot)
			{
				PlayerStats bitComponent = this.CarHub.Player.CharacterInstance.GetBitComponent<PlayerStats>();
				if (enable)
				{
					bitComponent.BotControlledChronometer.Start();
				}
				else
				{
					bitComponent.BotControlledChronometer.Stop();
				}
			}
			this._playersDispatcher.UpdatePlayer(base.Id.ObjId);
		}

		private void ServerListenToPlayerReconnected()
		{
			this.ServerClearInterfaceBooleans();
		}

		private void Start()
		{
			this._startRan = true;
			if (GameHubBehaviour.Hub && (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.IsTest()))
			{
				this._isCurrentPlayer = (GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId == this.Combat.Player.PlayerCarId);
				base.enabled = this._isCurrentPlayer;
			}
		}

		public void OnObjectSpawned(SpawnEvent msg)
		{
		}

		public void OnObjectUnspawned(UnspawnEvent msg)
		{
			this.Inputs.Clear();
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.UpdateInput();
			}
		}

		private void Update()
		{
		}

		private void LateUpdate()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			if (!GameHubBehaviour.Hub.Net.isTest && GameHubBehaviour.Hub.Net.IsServer() && this.CarHub.Player.IsBotControlled)
			{
				this.Inputs.DrivingStyle = CarInput.DrivingStyleKind.Bot;
				return;
			}
			InputDevice inputDevice = this._getActiveDevicePoller.GetActiveDevice();
			if (inputDevice != 2 && inputDevice != 1)
			{
				if (inputDevice == 3)
				{
					this.Inputs.DrivingStyle = CarInput.DrivingStyleKind.Controller;
				}
			}
			else
			{
				inputDevice = 1;
				this.Inputs.DrivingStyle = CarInput.DrivingStyleKind.FollowMouse;
			}
			this._isOptionsVisibilityChangedInFrame = (this._isOptionsVisible != this._optionsPresenter.Visible);
			this._isOptionsVisible = this._optionsPresenter.Visible;
			bool flag = GameHubBehaviour.Hub.GuiScripts.DriverHelper.IsWindowVisible();
			this._isHelpVisibilityChangedInFrame = (this._isHelpVisible != flag);
			this._isHelpVisible = flag;
			this._isActiveDeviceChangedInFrame = (inputDevice != this._lastActiveDevice);
			this._lastActiveDevice = inputDevice;
			this.Inputs.InverseReverse = GameHubBehaviour.Hub.Options.Game.InverseReverseControl;
			switch (this.Inputs.DrivingStyle)
			{
			case CarInput.DrivingStyleKind.Simulator:
			case CarInput.DrivingStyleKind.FollowMouse:
				if (this._gameCamera.SkyViewFollowMouse != SkyViewFollowMode.Mouse)
				{
					this._gameCamera.SkyViewFollowMouse = SkyViewFollowMode.Mouse;
				}
				break;
			case CarInput.DrivingStyleKind.Controller:
				if (this._gameCamera.SkyViewFollowMouse != SkyViewFollowMode.JoyAxis)
				{
					this._gameCamera.SkyViewFollowMouse = SkyViewFollowMode.JoyAxis;
				}
				break;
			default:
				if (this.Combat.IsPlayer && !this.Combat.IsBot)
				{
					PlayerController.Log.DebugFormat("Player {0} is using an unsupported drivingStyle {1}", new object[]
					{
						base.gameObject.name,
						this.Inputs.DrivingStyle
					});
				}
				break;
			}
			this.Feed();
			if (GameHubBehaviour.Hub.Net.isTest || GameHubBehaviour.Hub.Net.IsClient())
			{
				this.CarInput.Input(this.Inputs);
			}
			this._isActiveDeviceChangedInFrame = false;
		}

		private void Feed()
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			bool value = false;
			bool flag9 = false;
			bool flag10 = false;
			this._backupInputs.Copy(this.Inputs);
			this.Inputs.Up = false;
			this.Inputs.Down = false;
			this.Inputs.Left = false;
			this.Inputs.Right = false;
			this.Inputs.Respawn = false;
			this.Inputs.Dir = Vector2.zero;
			this.Inputs.Speed = 0f;
			this.Inputs.ReverseGear = false;
			bool flag11 = GameHubBehaviour.Hub.Net.IsTest() || (!this._optionsPresenter.Visible && !GameHubBehaviour.Hub.GuiScripts.Loading.IsLoading && !this.ShopInterfaceOpen && !this.HudChatOpen && !this._muteSystemPresenter.Visible && !this._isOptionsVisibilityChangedInFrame && !this._isHelpVisibilityChangedInFrame);
			bool flag12 = GameHubBehaviour.Hub.Net.IsTest() || (flag11 && !this.HudTabInterfaceOpen && !this.InputChatInterfaceOpen && !GameHubBehaviour.Hub.GuiScripts.DriverHelper.IsWindowVisible() && !GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.Visible && !PauseController.Instance.IsGamePaused);
			bool flag13 = false;
			if (this._gameCamera.SkyViewFollowMouse == SkyViewFollowMode.Mouse)
			{
				Vector2 mousePosition = this._inputActionPoller.GetMousePosition();
				Ray ray = this._gameCameraEngine.UnityCamera.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y));
				Vector3 vector;
				if (UnityUtils.RayCastGroundPlane(ray, out vector, 0f))
				{
					this.Inputs.MouseDir = vector - base.transform.position;
					this.Inputs.MouseDir.y = 0f;
					flag13 = (this.Inputs.MouseDir.sqrMagnitude > 5f);
					this.Inputs.MouseDir.Normalize();
					this.Inputs.MousePos = vector;
				}
			}
			this.Inputs.ToggleChat = (this._inputActionPoller.GetButton(11) || this._inputActionPoller.GetButton(10) || this._inputActionPoller.GetButton(12));
			this.Inputs.TogglePause = this._inputActionPoller.GetButtonDown(20);
			if (flag11 && !PlayerController.LockedInputs)
			{
				bool button = this._inputActionPoller.GetButton(23);
				this.Inputs.Respawn = (flag12 && this.GetRespawnButtonDown() && !this._isActiveDeviceChangedInFrame);
				if (!this.InputChatInterfaceOpen)
				{
					CarInput.DrivingStyleKind drivingStyle = this.Inputs.DrivingStyle;
					if (drivingStyle != CarInput.DrivingStyleKind.Controller)
					{
						if (drivingStyle != CarInput.DrivingStyleKind.Simulator)
						{
							if (drivingStyle == CarInput.DrivingStyleKind.FollowMouse)
							{
								this.Inputs.Up = (this._inputActionPoller.GetButton(4) && this.TutorialInputUpButtonIsActive);
								this.Inputs.Down = (this._inputActionPoller.GetButton(3) && this.TutorialInputDownButtonIsActive && ControlOptions.IsControlActionUnlocked(3));
								this.Inputs.Dir = ((!flag13) ? Vector2.zero : this.Inputs.MouseDir.ToVector2XZ());
							}
						}
						else
						{
							this.Inputs.Up = (this._inputActionPoller.GetButton(4) && this.TutorialInputUpButtonIsActive);
							this.Inputs.Down = (this._inputActionPoller.GetButton(3) && this.TutorialInputDownButtonIsActive);
							this.Inputs.Left = (this._inputActionPoller.GetButton(24) && this.TutorialInputLeftButtonIsActive);
							this.Inputs.Right = (this._inputActionPoller.GetButton(25) && this.TutorialInputRightButtonIsActive);
							this.Inputs.Dir.x = (float)((!(this.Inputs.Left ^ this.Inputs.Right)) ? 0 : ((!this.Inputs.Left) ? 1 : -1));
							this.Inputs.Dir.y = (float)((!(this.Inputs.Up ^ this.Inputs.Down)) ? 0 : ((!this.Inputs.Up) ? -1 : 1));
						}
					}
					else
					{
						Vector2 compositeAxis = this._inputActionPoller.GetCompositeAxis(1, 2);
						this.Inputs.Dir = new Vector2(compositeAxis.x, compositeAxis.y);
						this.Inputs.Speed = Mathf.Clamp01(this.Inputs.Dir.magnitude);
						this.Inputs.Up = (this.Inputs.Speed > 0f && !this.Inputs.ReverseGear);
						this.Inputs.Down = (this.Inputs.Speed > 0f && this.Inputs.ReverseGear);
					}
					if (!this.ShopInterfaceOpen && !this.HudTabInterfaceOpen)
					{
						flag7 = (this._inputActionPoller.GetButton(9) && ControlOptions.IsControlActionUnlocked(9));
						flag = (!button && this._inputActionPoller.GetButton(5) && ControlOptions.IsControlActionUnlocked(5));
						flag2 = (this._inputActionPoller.GetButton(6) && ControlOptions.IsControlActionUnlocked(6));
						flag3 = (this._inputActionPoller.GetButton(7) && ControlOptions.IsControlActionUnlocked(7));
						flag6 = (this._inputActionPoller.GetButton(17) && ControlOptions.IsControlActionUnlocked(17));
						flag5 = (this._inputActionPoller.GetButton(8) && ControlOptions.IsControlActionUnlocked(8));
						flag4 = (this._inputActionPoller.GetButton(15) && ControlOptions.IsControlActionUnlocked(15));
						flag8 = (this._inputActionPoller.GetButton(16) && ControlOptions.IsControlActionUnlocked(16));
						flag9 = (this._inputActionPoller.GetButton(28) && ControlOptions.IsControlActionUnlocked(28));
						flag10 = (this._inputActionPoller.GetButton(55) && ControlOptions.IsControlActionUnlocked(55));
						this.UpdateInputEmoteMouse(flag9);
						this.CheckOpenQuickChatPresenter(flag10);
						value = this.isGridHighlightTriggered;
						this.isGridHighlightTriggered = false;
						if (this._gameCamera.SkyViewFollowMouse == SkyViewFollowMode.JoyAxis)
						{
							Vector2 compositeAxis2 = this._inputActionPoller.GetCompositeAxis(22, 21);
							bool flag14 = this.Combat && this.Combat.IsAlive();
							if (!flag14 && compositeAxis2.sqrMagnitude < 0.09f)
							{
								compositeAxis2 = this._inputActionPoller.GetCompositeAxis(1, 2);
							}
							Vector3 vector2 = (!flag14) ? GameHubBehaviour.Hub.BombManager.BombMovement.transform.position : base.transform.position;
							this.UpdateJoystickInputAim(new Vector2(compositeAxis2.x, compositeAxis2.y));
							float num = flag14 ? this.JoyMaxRange : this.JoyRespawnRange;
							Vector3 vector3 = this.CarInput.ConvertAim(this.Inputs.Aim * num).ToVector3XZ();
							this.Inputs.MouseDir = ((compositeAxis2.sqrMagnitude <= 0.09f) ? Vector3.zero : vector3.normalized);
							this.Inputs.MousePos = vector3 + vector2;
							this.Inputs.ReverseGear = (this._inputActionPoller.GetButton(3) && this.TutorialInputDownButtonIsActive && ControlOptions.IsControlActionUnlocked(3));
						}
					}
				}
			}
			if (!GameHubBehaviour.Hub.Net.isTest && PauseController.Instance.IsGamePaused)
			{
				if (flag || flag2 || flag3 || flag5 || flag6 || flag4 || flag7 || flag8 || flag9 || flag10)
				{
					if (!this._wasPausedAndSomeActiveInput)
					{
						PauseController.Instance.TriggerPauseNotification(new PauseController.PauseNotification
						{
							kind = PauseController.PauseNotificationKind.InputBlocked
						});
					}
					this._wasPausedAndSomeActiveInput = true;
				}
				else
				{
					this._wasPausedAndSomeActiveInput = false;
				}
			}
			else
			{
				this._wasPausedAndSomeActiveInput = false;
				this.FeedGadgetInput(flag, ref this.Inputs.G0.Pressed, ref this.Inputs.G0.Changed, GadgetSlot.CustomGadget0);
				this.FeedGadgetInput(flag2, ref this.Inputs.G1.Pressed, ref this.Inputs.G1.Changed, GadgetSlot.CustomGadget1);
				this.FeedGadgetInput(flag3, ref this.Inputs.G2.Pressed, ref this.Inputs.G2.Changed, GadgetSlot.CustomGadget2);
				this.FeedGadgetInput(flag5, ref this.Inputs.GG.Pressed, ref this.Inputs.GG.Changed, GadgetSlot.GenericGadget);
				this.FeedGadgetInput(flag4, ref this.Inputs.GBoost.Pressed, ref this.Inputs.GBoost.Changed, GadgetSlot.BoostGadget);
				this.FeedGadgetInput(flag7, ref this.Inputs.Bomb, ref this.Inputs.BombChanged, GadgetSlot.BombGadget);
				this.FeedGadgetInput(flag8, ref this.Inputs.GSponsor.Pressed, ref this.Inputs.BombChanged, GadgetSlot.SprayGadget);
				this.FeedGadgetInput(value, ref this.Inputs.GHightlight.Pressed, ref this.Inputs.GHightlight.Changed, GadgetSlot.GridHighlightGadget);
				this.FeedGadgetInput(flag9, ref this.Inputs.GEmoteMenu.Pressed, ref this.Inputs.GEmoteMenu.Changed, GadgetSlot.EmoteRadialGadget);
				this.FeedGadgetInput(flag10, ref this.Inputs.QuickChatMenu.Pressed, ref this.Inputs.QuickChatMenu.Changed, GadgetSlot.QuickChatMenu);
			}
			if (this.Inputs.DrivingStyle == CarInput.DrivingStyleKind.Controller)
			{
				this.Inputs.Dir = this.CarInput.FixDirection(this.Inputs.Dir);
			}
			this.Inputs.CheckMovementChanged(this._backupInputs);
		}

		private bool GetRespawnButtonDown()
		{
			return this._inputActionPoller.GetButtonDown(35) || this._getRespawnSecondaryKeyWasPressed.WasPressed();
		}

		private void UpdateJoystickInputAim(Vector2 aimVect)
		{
			bool flag = (double)aimVect.sqrMagnitude > 0.5;
			if (this._inputJoystickCursorLoad.LoadCursorMode() == JoystickCursorMode.ReturnToCenter || flag)
			{
				float sensitivityInfraValue = this._inputJoystickCursorLoad.GetSensitivityInfraValue();
				this.Inputs.Aim = Vector2.MoveTowards(this.Inputs.Aim, aimVect, sensitivityInfraValue * Time.deltaTime);
			}
		}

		private void UpdateInputEmoteMouse(bool isPressed)
		{
			if (isPressed && this.Combat.IsAlive())
			{
				this.CachedGameGui.ShowEmotesMenu();
			}
			else
			{
				this.CachedGameGui.HideEmotesMenu();
			}
		}

		private void CheckOpenQuickChatPresenter(bool quickChatMenuButtonPressed)
		{
			if (quickChatMenuButtonPressed)
			{
				this._cachedGameGui.ShowQuickChatMenu();
			}
			else
			{
				this._cachedGameGui.HideQuickChatMenu();
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event PlayerController.GadgetInputPressedListener ListenToGadgetInputPressed;

		private void FeedGadgetInput(bool value, ref bool gadget, ref bool gadgetChanged, GadgetSlot slot)
		{
			bool flag = gadgetChanged;
			gadgetChanged |= (gadget != value);
			if (!flag && gadgetChanged)
			{
				gadget = value;
			}
			if (gadget && this.ListenToGadgetInputPressed != null)
			{
				this.ListenToGadgetInputPressed(slot);
			}
		}

		public void ExecInput(PlayerController.InputMap input)
		{
			if (input.Down && !this.Inputs.Down && this.ServerListenToReverseUse != null)
			{
				this.ServerListenToReverseUse();
			}
			this.Inputs.Copy(input);
			this.UpdateInput();
		}

		private void LogDrivingStyle(CarInput.DrivingStyleKind style)
		{
			bool flag = this.DrivingStylesUsed[(int)style];
			this.DrivingStylesUsed[(int)style] = true;
			if (!flag)
			{
				MatchLogWriter.UserAction(0, this.Combat.Player.UserId, style.ToString());
			}
		}

		private void UpdateInput()
		{
			if (this.Combat.IsPlayer && GameHubBehaviour.Hub.Net.IsServer() && this.Inputs.HasPressedAnyAntiAfkInput())
			{
				this._afkManager.InputChanged(this.Combat.Player);
			}
			if (!GameHubBehaviour.Hub.Net.isTest && GameHubBehaviour.Hub.Net.IsServer() && this.CarHub.Player.IsBotControlled)
			{
				return;
			}
			this.LogDrivingStyle(this.Inputs.DrivingStyle);
			this.CarInput.Input(this.Inputs);
			for (int i = 0; i < this.Combat.CustomGadgets.Count; i++)
			{
				if (this.Combat.CustomGadgets[i] is CombatGadget)
				{
					((CombatGadget)this.Combat.CustomGadgets[i]).Pressed = this.Inputs.IsGadgetPressed(((CombatGadget)this.Combat.CustomGadgets[i]).Slot);
				}
			}
			bool flag = false;
			if (this.Combat.CustomGadget0 && !flag)
			{
				flag |= (this.Inputs.G0.Pressed && this.CheckGadgetCanBeUsed(this.Combat.CustomGadget0));
				this.Combat.CustomGadget0.Pressed = flag;
				this.Combat.CustomGadget0.Dir = this.Inputs.MouseDir;
				this.Combat.CustomGadget0.Target = this.Inputs.MousePos;
			}
			flag &= !this.Combat.HasGadgetContext(1);
			if (!flag && this.Combat.CustomGadget1)
			{
				flag |= (this.Inputs.G1.Pressed && this.CheckGadgetCanBeUsed(this.Combat.CustomGadget1) && !this.Combat.HasGadgetContext(2));
				this.Combat.CustomGadget1.Pressed = flag;
				this.Combat.CustomGadget1.Dir = this.Inputs.MouseDir;
				this.Combat.CustomGadget1.Target = this.Inputs.MousePos;
			}
			if (!flag && this.Combat.CustomGadget2)
			{
				flag |= (this.Inputs.G2.Pressed && this.CheckGadgetCanBeUsed(this.Combat.CustomGadget2) && !this.Combat.HasGadgetContext(3));
				this.Combat.CustomGadget2.Pressed = flag;
				this.Combat.CustomGadget2.Dir = this.Inputs.MouseDir;
				this.Combat.CustomGadget2.Target = this.Inputs.MousePos;
			}
			if (!flag && this.Combat.BoostGadget)
			{
				flag |= (this.Inputs.GBoost.Pressed && this.CheckGadgetCanBeUsed(this.Combat.BoostGadget) && !this.Combat.HasGadgetContext(4));
				this.Combat.BoostGadget.Pressed = flag;
				this.Combat.BoostGadget.Dir = this.Inputs.MouseDir;
				this.Combat.BoostGadget.Target = this.Inputs.MousePos;
			}
			if (this.ShouldUpdateBombInput(flag))
			{
				this.Combat.BombGadget.Pressed = this.Inputs.Bomb;
				this.Combat.BombGadget.Dir = this.Inputs.MouseDir;
				this.Combat.BombGadget.Target = this.Inputs.MousePos;
			}
			if (this.Combat.SprayGadget)
			{
				this.Combat.SprayGadget.Pressed = this.Inputs.GSponsor.Pressed;
				this.Combat.SprayGadget.Dir = this.Inputs.MouseDir;
				this.Combat.SprayGadget.Target = this.Inputs.MousePos;
			}
			if (this.Combat.GridHighlightGadget)
			{
				this.Combat.GridHighlightGadget.Pressed = this.Inputs.GHightlight.Pressed;
				this.Combat.GridHighlightGadget.Dir = this.Inputs.MouseDir;
				this.Combat.GridHighlightGadget.Target = this.Inputs.MousePos;
			}
			if (this.Combat.GenericGadget)
			{
				this.Combat.GenericGadget.Pressed = this.Inputs.GG.Pressed;
				this.Combat.GenericGadget.Dir = this.Inputs.MouseDir;
				this.Combat.GenericGadget.Target = this.Inputs.MousePos;
			}
			if (this.Combat.RespawnGadget)
			{
				this.Combat.RespawnGadget.Pressed = this.Inputs.Respawn;
				this.Combat.RespawnGadget.Dir = this.Inputs.MouseDir;
				this.Combat.RespawnGadget.Target = this.Inputs.MousePos;
			}
		}

		private bool ShouldUpdateBombInput(bool gadgetWasUsedThisFrame)
		{
			return !(this.Combat.BombGadget == null) && (!this.Inputs.Bomb || !GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this.Combat) || !gadgetWasUsedThisFrame);
		}

		public void TriggerGridHighlightGadget()
		{
			if (this.Combat.GridHighlightGadget)
			{
				this.isGridHighlightTriggered = true;
			}
		}

		private bool CheckGadgetCanBeUsed(GadgetBehaviour gadget)
		{
			GadgetState gadgetState = this.Combat.GadgetStates.GetGadgetState(gadget.Slot).GadgetState;
			bool flag = gadget.TestSecondClick();
			bool flag2 = this.Combat.Attributes.IsGadgetDisarmed(gadget.Slot, gadget.Nature);
			return (gadgetState == GadgetState.Ready || gadgetState == GadgetState.Toggled || flag) && !flag2;
		}

		public void ActionExecuted(GadgetBehaviour gadget)
		{
			if (gadget != this.Combat.CustomGadget0 && gadget != this.Combat.CustomGadget1 && gadget != this.Combat.CustomGadget2 && gadget != this.Combat.BombGadget && gadget != this.Combat.BoostGadget)
			{
				return;
			}
			if (this.ListenToCancelAction != null)
			{
				this.ListenToCancelAction(gadget);
			}
		}

		public void Reset()
		{
			this.Inputs.G0.Changed = false;
			this.Inputs.G1.Changed = false;
			this.Inputs.G2.Changed = false;
			this.Inputs.GG.Changed = false;
			this.Inputs.GBoost.Changed = false;
			this.Inputs.GSponsor.Changed = false;
			this.Inputs.GHightlight.Changed = false;
			this.Inputs.GEmoteMenu.Changed = false;
			this.Inputs.QuickChatMenu.Changed = false;
			this.Inputs.Bomb = false;
			this.Inputs.BombChanged = false;
		}

		public void AddGadgetCommand(GadgetSlot slot)
		{
			this.Inputs.AddPressCommand(slot);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PlayerController));

		[Inject]
		private IMatchPlayersDispatcher _playersDispatcher;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		[InjectOnClient]
		private IGameCamera _gameCamera;

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		[InjectOnServer]
		private IAFKManager _afkManager;

		public CarInput CarInput;

		public CombatObject Combat;

		public GadgetData Gadgetdata;

		public CarComponentHub CarHub;

		public static bool LockedInputs;

		public PlayerController.InputMap Inputs = new PlayerController.InputMap();

		[NonSerialized]
		private PlayerController.InputMap _backupInputs = new PlayerController.InputMap();

		public bool TutorialInputUpButtonIsActive = true;

		public bool TutorialInputDownButtonIsActive = true;

		public bool TutorialInputLeftButtonIsActive = true;

		public bool TutorialInputRightButtonIsActive = true;

		public bool TutorialInputDriftButtonIsActive = true;

		public Color AttackCursorAvailableColor;

		public Color AttackCursorUnavailableColor;

		public Color BasicAttackCursorAvailableColor;

		public Color BasicAttackCursorUnavailableColor;

		public float AttackCursorDelay;

		public bool InputChatInterfaceOpen;

		public bool ShopInterfaceOpen;

		public bool HudTabInterfaceOpen;

		public bool HudChatOpen;

		public string SelectedInstance;

		[InjectOnClient]
		private IControllerInputActionPoller _inputActionPoller;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _getActiveDevicePoller;

		[InjectOnClient]
		private IInputJoystickCursorLoad _inputJoystickCursorLoad;

		[InjectOnClient]
		private IOptionsPresenter _optionsPresenter;

		[InjectOnClient]
		private IMuteSystemPresenter _muteSystemPresenter;

		[InjectOnClient]
		private IGetRespawnSecondaryKeyWasPressed _getRespawnSecondaryKeyWasPressed;

		private bool _isActiveDeviceChangedInFrame;

		private InputDevice _lastActiveDevice;

		private bool _isOptionsVisibilityChangedInFrame;

		private bool _isOptionsVisible;

		private bool _isHelpVisibilityChangedInFrame;

		private bool _isHelpVisible;

		private GameGui _cachedGameGui;

		private bool _isCurrentPlayer;

		private bool _startRan;

		public float JoyRespawnRange = 110f;

		public float JoyMaxRange = 100f;

		private bool _wasPausedAndSomeActiveInput;

		public bool[] DrivingStylesUsed;

		private bool isGridHighlightTriggered;

		public interface IControllerCursor
		{
			bool Visible { set; }

			Vector3 CursorPos { set; }
		}

		[Serializable]
		public struct GadgetInput : IBitStreamSerializable
		{
			public void Clear()
			{
				this.Pressed = (this.Changed = false);
			}

			public void Copy(PlayerController.GadgetInput other)
			{
				this.Pressed = other.Pressed;
				this.Changed = other.Changed;
			}

			public void WriteToBitStream(BitStream bs)
			{
				bs.WriteBool(this.Pressed);
				bs.WriteBool(this.Changed);
			}

			public void ReadFromBitStream(BitStream bs)
			{
				this.Pressed = bs.ReadBool();
				this.Changed = bs.ReadBool();
			}

			public bool Pressed;

			public bool Changed;
		}

		[Serializable]
		public class InputMap : IBitStreamSerializable
		{
			public void AddPressCommand(GadgetSlot slot)
			{
				if (!this._gadgetSlotPressCommand.Contains(slot))
				{
					this._gadgetSlotPressCommand.Add(slot);
				}
			}

			public IEnumerator RemovePressCommand(GadgetSlot slot)
			{
				yield return new WaitForSeconds(1f);
				if (this._gadgetSlotPressCommand.Contains(slot))
				{
					this._gadgetSlotPressCommand.Remove(slot);
				}
				yield break;
			}

			public void Clear()
			{
				this.Speed = 0f;
				this.Dir = Vector2.zero;
				this.Aim = Vector2.zero;
				this.MouseDir = Vector3.zero;
				this.MousePos = Vector3.zero;
				this.MouseScreenPos = Vector2.zero;
				this.MovementChanged = (this.Up = (this.Down = (this.Left = (this.Right = false))));
				this.G0.Clear();
				this.G1.Clear();
				this.G2.Clear();
				this.GG.Clear();
				this.GSponsor.Clear();
				this.GHightlight.Clear();
				this.GEmoteMenu.Clear();
				this.QuickChatMenu.Clear();
				this.Bomb = (this.BombChanged = false);
				this.GBoost.Clear();
				this.Respawn = false;
				this.LiftMousePos = Vector3.zero;
				this.MinimapTargeted = false;
				this.ReverseGear = false;
				this.InverseReverse = false;
				this._gadgetSlotPressCommand.Clear();
			}

			public void Copy(PlayerController.InputMap other)
			{
				this.Speed = other.Speed;
				this.Dir = other.Dir;
				this.Aim = other.Aim;
				this.Up = other.Up;
				this.Down = other.Down;
				this.Left = other.Left;
				this.Right = other.Right;
				this.MouseDir = other.MouseDir;
				this.MousePos = other.MousePos;
				this.MouseScreenPos = other.MouseScreenPos;
				this.G0.Copy(other.G0);
				this.G1.Copy(other.G1);
				this.G2.Copy(other.G2);
				this.GG.Copy(other.GG);
				this.GSponsor.Copy(other.GSponsor);
				this.GHightlight.Copy(other.GHightlight);
				this.GEmoteMenu.Copy(other.GEmoteMenu);
				this.QuickChatMenu.Copy(other.QuickChatMenu);
				this.GBoost.Copy(other.GBoost);
				this.Respawn = other.Respawn;
				this.LiftMousePos = other.LiftMousePos;
				this.Bomb = other.Bomb;
				this.BombChanged = other.BombChanged;
				this.MinimapTargeted = other.MinimapTargeted;
				this.DrivingStyle = other.DrivingStyle;
				this.ReverseGear = other.ReverseGear;
				this.InverseReverse = other.InverseReverse;
				this.ToggleChat = other.ToggleChat;
				this.TogglePause = other.TogglePause;
				this._gadgetSlotPressCommand.Clear();
				for (int i = 0; i < other._gadgetSlotPressCommand.Count; i++)
				{
					this._gadgetSlotPressCommand.Add(other._gadgetSlotPressCommand[i]);
				}
			}

			public bool IsGadgetPressed(GadgetSlot slot)
			{
				switch (slot)
				{
				case GadgetSlot.GridHighlightGadget:
					return this.GHightlight.Pressed;
				default:
					switch (slot)
					{
					case GadgetSlot.CustomGadget0:
						return this.G0.Pressed;
					case GadgetSlot.CustomGadget1:
						return this.G1.Pressed;
					case GadgetSlot.CustomGadget2:
						return this.G2.Pressed;
					case GadgetSlot.BoostGadget:
						return this.GBoost.Pressed;
					case GadgetSlot.RespawnGadget:
						return this.Respawn;
					case GadgetSlot.BombGadget:
						return this.Bomb;
					}
					return false;
				case GadgetSlot.EmoteRadialGadget:
					return this.GEmoteMenu.Pressed;
				case GadgetSlot.EmoteGadget0:
				case GadgetSlot.EmoteGadget1:
				case GadgetSlot.EmoteGadget2:
				case GadgetSlot.EmoteGadget3:
				case GadgetSlot.EmoteGadget4:
				case GadgetSlot.QuickChat00:
				case GadgetSlot.QuickChat01:
				case GadgetSlot.QuickChat02:
				case GadgetSlot.QuickChat03:
				case GadgetSlot.QuickChat04:
				case GadgetSlot.QuickChat05:
				case GadgetSlot.QuickChat06:
				case GadgetSlot.QuickChat07:
					return this._gadgetSlotPressCommand.Contains(slot);
				case GadgetSlot.QuickChatMenu:
					return this.QuickChatMenu.Pressed;
				}
			}

			public void CheckMovementChanged(PlayerController.InputMap old)
			{
				CarInput.DrivingStyleKind drivingStyle = this.DrivingStyle;
				if (drivingStyle != CarInput.DrivingStyleKind.Simulator)
				{
					if (drivingStyle != CarInput.DrivingStyleKind.Controller && drivingStyle != CarInput.DrivingStyleKind.FollowMouse)
					{
					}
				}
				else
				{
					this.MovementChanged = (this.Up != old.Up || this.Down != old.Down || this.Right != old.Right || this.Left != old.Left);
				}
			}

			public bool HasPauseButtonPressed()
			{
				return this.TogglePause;
			}

			public bool HasPressedAnyAntiAfkInput()
			{
				return this.Up || this.Down || this.Left || this.Right || this.G0.Pressed || this.G1.Pressed || this.G2.Pressed || this.GG.Pressed || this.GBoost.Pressed || this.GSponsor.Pressed || this.ToggleChat || this.TogglePause || this.Bomb || this.GEmoteMenu.Pressed || this.QuickChatMenu.Changed;
			}

			public bool HasGadgetInputChanged()
			{
				return this.G0.Changed || this.G1.Changed || this.G2.Changed || this.GG.Changed || this.GSponsor.Changed || this.GBoost.Changed || this.GHightlight.Changed || this.GEmoteMenu.Changed || this.QuickChatMenu.Changed;
			}

			public bool IsControllerActive()
			{
				return this.DrivingStyle == CarInput.DrivingStyleKind.Controller;
			}

			public bool IsFollowMouseActive()
			{
				return this.DrivingStyle == CarInput.DrivingStyleKind.FollowMouse;
			}

			public void WriteToBitStream(BitStream bs)
			{
				bs.WriteTinyUFloat(this.Speed);
				bs.WriteTinyFloat(this.Dir.x);
				bs.WriteTinyFloat(this.Dir.y);
				bs.WriteTinyFloat(this.Aim.x);
				bs.WriteTinyFloat(this.Aim.y);
				bs.WriteBool(this.Up);
				bs.WriteBool(this.Down);
				bs.WriteBool(this.Left);
				bs.WriteBool(this.Right);
				bs.WriteVector3(this.MouseDir);
				bs.WriteVector3(this.MousePos);
				bs.WriteVector2(this.MouseScreenPos);
				this.G0.WriteToBitStream(bs);
				this.G1.WriteToBitStream(bs);
				this.G2.WriteToBitStream(bs);
				this.GG.WriteToBitStream(bs);
				this.GSponsor.WriteToBitStream(bs);
				this.GHightlight.WriteToBitStream(bs);
				this.GEmoteMenu.WriteToBitStream(bs);
				this.QuickChatMenu.WriteToBitStream(bs);
				this.GBoost.WriteToBitStream(bs);
				bs.WriteBool(this.Respawn);
				bs.WriteVector3(this.LiftMousePos);
				bs.WriteBool(this.Bomb);
				bs.WriteBool(this.BombChanged);
				bs.WriteBool(this.MinimapTargeted);
				bs.WriteBits(2, (int)this.DrivingStyle);
				bs.WriteBool(this.ReverseGear);
				bs.WriteBool(this.InverseReverse);
				bs.WriteBool(this.ToggleChat);
				bs.WriteBool(this.TogglePause);
				bs.WriteCompressedInt(this._gadgetSlotPressCommand.Count);
				for (int i = 0; i < this._gadgetSlotPressCommand.Count; i++)
				{
					bs.WriteCompressedInt((int)this._gadgetSlotPressCommand[i]);
				}
				this._gadgetSlotPressCommand.Clear();
			}

			public void ReadFromBitStream(BitStream bs)
			{
				this.Speed = bs.ReadTinyUFloat();
				this.Dir = new Vector2(bs.ReadTinyFloat(), bs.ReadTinyFloat());
				this.Aim = new Vector2(bs.ReadTinyFloat(), bs.ReadTinyFloat());
				this.Up = bs.ReadBool();
				this.Down = bs.ReadBool();
				this.Left = bs.ReadBool();
				this.Right = bs.ReadBool();
				this.MouseDir = bs.ReadVector3();
				this.MousePos = bs.ReadVector3();
				this.MouseScreenPos = bs.ReadVector2();
				this.G0.ReadFromBitStream(bs);
				this.G1.ReadFromBitStream(bs);
				this.G2.ReadFromBitStream(bs);
				this.GG.ReadFromBitStream(bs);
				this.GSponsor.ReadFromBitStream(bs);
				this.GHightlight.ReadFromBitStream(bs);
				this.GEmoteMenu.ReadFromBitStream(bs);
				this.QuickChatMenu.ReadFromBitStream(bs);
				this.GBoost.ReadFromBitStream(bs);
				this.Respawn = bs.ReadBool();
				this.LiftMousePos = bs.ReadVector3();
				this.Bomb = bs.ReadBool();
				this.BombChanged = bs.ReadBool();
				this.MinimapTargeted = bs.ReadBool();
				this.DrivingStyle = (CarInput.DrivingStyleKind)bs.ReadBits(2);
				this.ReverseGear = bs.ReadBool();
				this.InverseReverse = bs.ReadBool();
				this.ToggleChat = bs.ReadBool();
				this.TogglePause = bs.ReadBool();
				this._gadgetSlotPressCommand.Clear();
				int num = bs.ReadCompressedInt();
				for (int i = 0; i < num; i++)
				{
					this._gadgetSlotPressCommand.Add((GadgetSlot)bs.ReadCompressedInt());
				}
			}

			public override string ToString()
			{
				return string.Format("[Inputs {0}/{1}/{2}/{3}/{4}/{5}/{6}]", new object[]
				{
					this.Up,
					this.Down,
					this.Left,
					this.Right,
					this.G0.Pressed,
					this.G1.Pressed,
					this.G2.Pressed
				});
			}

			public float Speed;

			public Vector2 Dir;

			public Vector2 Aim;

			public Vector3 MouseDir;

			public Vector3 MousePos;

			public Vector2 MouseScreenPos;

			public bool Up;

			public bool Down;

			public bool Left;

			public bool Right;

			public PlayerController.GadgetInput G0;

			public PlayerController.GadgetInput G1;

			public PlayerController.GadgetInput G2;

			public PlayerController.GadgetInput GG;

			public PlayerController.GadgetInput GSponsor;

			public PlayerController.GadgetInput GBoost;

			public PlayerController.GadgetInput GHightlight;

			public PlayerController.GadgetInput GEmoteMenu;

			public PlayerController.GadgetInput QuickChatMenu;

			public bool Bomb;

			public bool BombChanged;

			public bool Respawn;

			public Vector3 LiftMousePos;

			public bool MinimapTargeted;

			public CarInput.DrivingStyleKind DrivingStyle;

			public bool ReverseGear;

			public bool InverseReverse;

			public bool ToggleChat;

			public bool TogglePause;

			public bool MovementChanged;

			private List<GadgetSlot> _gadgetSlotPressCommand = new List<GadgetSlot>();
		}

		public delegate void GadgetInputPressedListener(GadgetSlot slot);
	}
}
