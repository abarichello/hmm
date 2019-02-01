using System;
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
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Swordfish.Logs;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class PlayerController : GameHubBehaviour, IObjectSpawnListener, IPlayerController
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ServerListenToReverseUse;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CancelActionListener ListenToCancelAction;

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
				UnityEngine.Debug.Log(string.Concat(new object[]
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
			this.CarHub.botAIGoalManager.BotAIController.Spawned = this.Combat.IsAlive();
			this.EnableBotControllerComponents(true);
			this.CarHub.botAIGoalManager.Initialize();
			this.CarHub.botAIGadgetShop.InitValues(this.CarHub);
			this.CarHub.botAIGoalManager.UpdateBotOnlyTeamGoalCap();
			this.CarHub.botAIGoalManager.UpdateAllBotOnlyTeamGoalCap();
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
			this.EnableBotControllerComponents(false);
			this.CarHub.botAIGoalManager.UpdateBotOnlyTeamGoalCap();
			this.CarHub.botAIGoalManager.UpdateAllBotOnlyTeamGoalCap();
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.BotControllerDeactivated,
				Killer = this.Combat.Id.ObjId
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		public void EnableBotControllerComponents(bool enable)
		{
			this.CarHub.botAIPathFind.enabled = enable;
			this.CarHub.botAIGadgetShop.enabled = enable;
			this.CarHub.botAIGoalManager.BotAIController.enabled = enable;
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
			GameHubBehaviour.Hub.Players.UpdatePlayer(base.Id.ObjId);
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
			this.Inputs.DrivingStyle = (CarInput.DrivingStyleKind)GameHubBehaviour.Hub.Options.Game.MovementModeIndex;
			this.Inputs.InverseReverse = GameHubBehaviour.Hub.Options.Game.InverseReverseControl;
			switch (this.Inputs.DrivingStyle)
			{
			case CarInput.DrivingStyleKind.Simulator:
			case CarInput.DrivingStyleKind.FollowMouse:
				if (CarCamera.Singleton.SkyViewFollowMouse != CarCamera.SkyViewFollowMode.Mouse)
				{
					CarCamera.Singleton.SkyViewFollowMouse = CarCamera.SkyViewFollowMode.Mouse;
					if (this.ControllerCursor != null)
					{
						this.ControllerCursor.Visible = false;
					}
				}
				break;
			case CarInput.DrivingStyleKind.Controller:
				if (CarCamera.Singleton.SkyViewFollowMouse != CarCamera.SkyViewFollowMode.JoyAxis)
				{
					CarCamera.Singleton.SkyViewFollowMouse = CarCamera.SkyViewFollowMode.JoyAxis;
				}
				break;
			default:
				if (!this.Combat.IsPlayer || !this.Combat.IsBot)
				{
				}
				break;
			}
			this.Feed();
			if (GameHubBehaviour.Hub.Net.isTest || GameHubBehaviour.Hub.Net.IsClient())
			{
				this.CarInput.Input(this.Inputs, false);
			}
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
			bool value = false;
			bool value2 = false;
			this._backupInputs.Copy(this.Inputs);
			this.Inputs.Up = false;
			this.Inputs.Down = false;
			this.Inputs.Left = false;
			this.Inputs.Right = false;
			this.Inputs.Respawn = false;
			this.Inputs.Dir = Vector2.zero;
			this.Inputs.ReverseGear = false;
			bool flag8 = GameHubBehaviour.Hub.Net.IsTest() || (!GameHubBehaviour.Hub.GuiScripts.Esc.IsWindowVisible() && !GameHubBehaviour.Hub.GuiScripts.Esc.IsOptionsWindowVisible() && !GameHubBehaviour.Hub.GuiScripts.Loading.IsLoading && !this.ShopInterfaceOpen && !this.HudChatOpen);
			bool flag9 = GameHubBehaviour.Hub.Net.IsTest() || (flag8 && !this.HudTabInterfaceOpen && !this.InputChatInterfaceOpen && !GameHubBehaviour.Hub.GuiScripts.DriverHelper.IsWindowVisible() && !GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.Visible && !PauseController.Instance.IsGamePaused);
			bool flag10 = false;
			if (CarCamera.Singleton.SkyViewFollowMouse == CarCamera.SkyViewFollowMode.Mouse)
			{
				Ray ray = CarCamera.Singleton.Camera.ScreenPointToRay(Input.mousePosition);
				Vector3 vector;
				if (UnityUtils.RayCastGroundPlane(ray, out vector, 0f))
				{
					this.Inputs.MouseDir = vector - base.transform.position;
					this.Inputs.MouseDir.y = 0f;
					flag10 = (this.Inputs.MouseDir.sqrMagnitude > 5f);
					this.Inputs.MouseDir.Normalize();
					this.Inputs.MousePos = vector;
				}
			}
			this.Inputs.ToggleChat = (ControlOptions.GetButton(ControlAction.ChatAll) || ControlOptions.GetButton(ControlAction.ChatTeam) || ControlOptions.GetButton(ControlAction.ChatSend));
			this.Inputs.TogglePause = ControlOptions.GetButtonDown(ControlAction.Pause);
			if (flag8 && !PlayerController.LockedInputs)
			{
				bool button = ControlOptions.GetButton(ControlAction.Ping);
				this.Inputs.Respawn = (flag9 && Input.GetMouseButtonDown(0));
				if (!this.InputChatInterfaceOpen)
				{
					CarInput.DrivingStyleKind drivingStyle = this.Inputs.DrivingStyle;
					if (drivingStyle != CarInput.DrivingStyleKind.Controller)
					{
						if (drivingStyle != CarInput.DrivingStyleKind.Simulator)
						{
							if (drivingStyle == CarInput.DrivingStyleKind.FollowMouse)
							{
								this.Inputs.Up = (ControlOptions.GetButton(ControlAction.MovementForward) && this.TutorialInputUpButtonIsActive);
								this.Inputs.Down = (ControlOptions.GetButton(ControlAction.MovementBackward) && this.TutorialInputDownButtonIsActive);
								this.Inputs.Dir = ((!flag10) ? Vector2.zero : this.Inputs.MouseDir.ToVector2XZ());
							}
						}
						else
						{
							this.Inputs.Up = (ControlOptions.GetButton(ControlAction.MovementForward) && this.TutorialInputUpButtonIsActive);
							this.Inputs.Down = (ControlOptions.GetButton(ControlAction.MovementBackward) && this.TutorialInputDownButtonIsActive);
							this.Inputs.Left = (ControlOptions.GetButton(ControlAction.MovementLeft) && this.TutorialInputLeftButtonIsActive);
							this.Inputs.Right = (ControlOptions.GetButton(ControlAction.MovementRight) && this.TutorialInputRightButtonIsActive);
							this.Inputs.Dir.x = (float)((!(this.Inputs.Left ^ this.Inputs.Right)) ? 0 : ((!this.Inputs.Left) ? 1 : -1));
							this.Inputs.Dir.y = (float)((!(this.Inputs.Up ^ this.Inputs.Down)) ? 0 : ((!this.Inputs.Up) ? -1 : 1));
						}
					}
					else
					{
						this.Inputs.Dir = new Vector2(ControlOptions.GetAxis(ControlOptions.JoystickInput.Joy1Axis1), -ControlOptions.GetAxis(ControlOptions.JoystickInput.Jo1Axis2));
						this.Inputs.Speed = Mathf.Clamp01(this.Inputs.Dir.magnitude);
						this.Inputs.Up = (this.Inputs.Speed > 0f && !this.Inputs.ReverseGear);
						this.Inputs.Down = (this.Inputs.Speed > 0f && this.Inputs.ReverseGear);
					}
					if (!this.ShopInterfaceOpen && !this.HudTabInterfaceOpen)
					{
						flag7 = ControlOptions.GetButton(ControlAction.GadgetDropBomb);
						flag = (!button && ControlOptions.GetButton(ControlAction.GadgetBasic));
						flag2 = ControlOptions.GetButton(ControlAction.Gadget0);
						flag3 = ControlOptions.GetButton(ControlAction.Gadget1);
						flag6 = ControlOptions.GetButton(ControlAction.GadgetSponsor);
						flag5 = ControlOptions.GetButton(ControlAction.GadgetGeneric);
						flag4 = ControlOptions.GetButton(ControlAction.GadgetBoost);
						value = ControlOptions.GetButton(ControlAction.Spray);
						value2 = this.isGridHighlightTriggered;
						this.isGridHighlightTriggered = false;
						if (CarCamera.Singleton.SkyViewFollowMouse == CarCamera.SkyViewFollowMode.JoyAxis)
						{
							Vector2 target = new Vector2(Input.GetAxis("Joy1 Axis 4"), -Input.GetAxis("Joy1 Axis 5"));
							Vector2 aim = Vector2.MoveTowards(this.Inputs.Aim, target, this.JoySpeed * this.JoyMaxRange / this.JoyRange * Time.deltaTime);
							this.Inputs.Aim = aim;
							Vector3 vector2 = this.CarInput.ConvertAim(this.Inputs.Aim * this.JoyRange).ToVector3XZ();
							Vector3 position;
							if (this.Combat && this.Combat.IsAlive())
							{
								position = base.transform.position;
							}
							else
							{
								position = GameHubBehaviour.Hub.BombManager.BombMovement.transform.position;
							}
							this.Inputs.MouseDir = vector2.normalized;
							this.Inputs.MousePos = vector2 + position;
							if (this.ControllerCursor != null)
							{
								if (target.sqrMagnitude > 0.09f && this.Combat && !this.Combat.IsAlive())
								{
									this.ControllerCursor.Visible = true;
									this.ControllerCursor.CursorPos = position + vector2;
								}
								else
								{
									this.ControllerCursor.Visible = false;
								}
							}
							this.Inputs.ReverseGear = ControlOptions.GetButton(ControlAction.MovementBackward);
						}
					}
				}
			}
			if (!GameHubBehaviour.Hub.Net.isTest && PauseController.Instance.IsGamePaused)
			{
				if (flag || flag2 || flag3 || flag5 || flag6 || flag4 || flag7)
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
				this.FeedGadgetInput(value, ref this.Inputs.GSponsor.Pressed, ref this.Inputs.BombChanged, GadgetSlot.SprayGadget);
				this.FeedGadgetInput(value2, ref this.Inputs.GHightlight.Pressed, ref this.Inputs.GHightlight.Changed, GadgetSlot.GridHighlightGadget);
			}
			CarInput.DrivingStyleKind drivingStyle2 = this.Inputs.DrivingStyle;
			if (drivingStyle2 == CarInput.DrivingStyleKind.Controller)
			{
				this.Inputs.Dir = this.CarInput.FixDirection(this.Inputs.Dir);
			}
			this.Inputs.CheckMovementChanged(this._backupInputs);
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
				MatchLogWriter.PlayerAction(LogAction.ClientControllerChanged, base.Id.ObjId, style.ToString());
			}
		}

		private bool CheckCustomGadgetCanBeUsed(GadgetSlot slot)
		{
			CombatGadget combatGadget;
			return !this.Combat.CustomGadgets.TryGetValue(slot, out combatGadget) || combatGadget.CheckGadgetCanBePressed();
		}

		private void UpdateInput()
		{
			if (this.ShopInterfaceOpen || this.HudTabInterfaceOpen)
			{
				this.Inputs.Clear();
				this.CarInput.Input(this.Inputs, false);
				if (this.Combat.CustomGadget0)
				{
					this.Combat.CustomGadget0.Pressed = false;
				}
				if (this.Combat.CustomGadget1)
				{
					this.Combat.CustomGadget1.Pressed = false;
				}
				if (this.Combat.CustomGadget2)
				{
					this.Combat.CustomGadget2.Pressed = false;
				}
				if (this.Combat.BoostGadget)
				{
					this.Combat.BoostGadget.Pressed = false;
				}
				if (this.Combat.BombGadget)
				{
					this.Combat.BombGadget.Pressed = false;
				}
				if (this.Combat.SprayGadget)
				{
					this.Combat.SprayGadget.Pressed = false;
				}
				if (this.Combat.GridHighlightGadget)
				{
					this.Combat.GridHighlightGadget.Pressed = false;
				}
				return;
			}
			if (this.Combat.IsPlayer && GameHubBehaviour.Hub.Net.IsServer() && this.Inputs.HasPressedWeaponsDriveOrChat())
			{
				GameHubBehaviour.Hub.afkController.InputChanged(this.Combat.Player);
			}
			if (!GameHubBehaviour.Hub.Net.isTest && GameHubBehaviour.Hub.Net.IsServer() && this.CarHub.Player.IsBotControlled)
			{
				return;
			}
			StatusKind currentStatus = this.Combat.Attributes.CurrentStatus;
			bool immobilized = currentStatus.HasFlag(StatusKind.Immobilized);
			this.LogDrivingStyle(this.Inputs.DrivingStyle);
			this.CarInput.Input(this.Inputs, immobilized);
			bool flag = false;
			if (this.Combat.CustomGadget0 && !flag)
			{
				flag |= (this.Inputs.G0.Pressed && this.CheckGadgetCanBeUsed(this.Combat.CustomGadget0) && this.CheckCustomGadgetCanBeUsed(GadgetSlot.CustomGadget0));
				this.Combat.CustomGadget0.Pressed = flag;
				this.Combat.CustomGadget0.Dir = this.Inputs.MouseDir;
				this.Combat.CustomGadget0.Target = this.Inputs.MousePos;
				Vector3 normalized = (this.Combat.CustomGadget0.Target - this.Combat.Transform.position).normalized;
				this.Combat.CarInput.TurretAngle = Mathf.Atan2(normalized.x, normalized.z) * 57.29578f;
			}
			if (!flag && this.Combat.CustomGadget1)
			{
				flag |= (this.Inputs.G1.Pressed && this.CheckGadgetCanBeUsed(this.Combat.CustomGadget1) && this.CheckCustomGadgetCanBeUsed(GadgetSlot.CustomGadget1));
				this.Combat.CustomGadget1.Pressed = flag;
				this.Combat.CustomGadget1.Dir = this.Inputs.MouseDir;
				this.Combat.CustomGadget1.Target = this.Inputs.MousePos;
			}
			if (!flag && this.Combat.CustomGadget2)
			{
				flag |= (this.Inputs.G2.Pressed && this.CheckGadgetCanBeUsed(this.Combat.CustomGadget2) && this.CheckCustomGadgetCanBeUsed(GadgetSlot.CustomGadget2));
				this.Combat.CustomGadget2.Pressed = flag;
				this.Combat.CustomGadget2.Dir = this.Inputs.MouseDir;
				this.Combat.CustomGadget2.Target = this.Inputs.MousePos;
			}
			if (!flag && this.Combat.BoostGadget)
			{
				flag |= (this.Inputs.GBoost.Pressed && this.CheckGadgetCanBeUsed(this.Combat.BoostGadget) && this.CheckCustomGadgetCanBeUsed(GadgetSlot.BoostGadget));
				this.Combat.BoostGadget.Pressed = flag;
				this.Combat.BoostGadget.Dir = this.Inputs.MouseDir;
				this.Combat.BoostGadget.Target = this.Inputs.MousePos;
			}
			if (this.Combat.BombGadget && (!GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this.Combat) || !flag))
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
			bool pressed = (this.Inputs.DrivingStyle != CarInput.DrivingStyleKind.Controller && this.Inputs.Up) || (this.Inputs.DrivingStyle == CarInput.DrivingStyleKind.Controller && this.Inputs.G0.Pressed);
			if (this.Combat.RespawnGadget)
			{
				this.Combat.RespawnGadget.Pressed = pressed;
				this.Combat.RespawnGadget.Dir = this.Inputs.MouseDir;
				this.Combat.RespawnGadget.Target = this.Inputs.MousePos;
			}
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
			this.Inputs.Bomb = false;
			this.Inputs.BombChanged = false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PlayerController));

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

		public PlayerController.IControllerCursor ControllerCursor;

		public bool InputChatInterfaceOpen;

		public bool ShopInterfaceOpen;

		public bool HudTabInterfaceOpen;

		public bool HudChatOpen;

		public string SelectedInstance;

		private bool _isCurrentPlayer;

		private bool _startRan;

		public float JoyMaxRange = 120f;

		public float JoyRange = 60f;

		public float JoySpeed = 10f;

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

			public void WriteToBitStream(Pocketverse.BitStream bs)
			{
				bs.WriteBool(this.Pressed);
				bs.WriteBool(this.Changed);
			}

			public void ReadFromBitStream(Pocketverse.BitStream bs)
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
				this.Bomb = (this.BombChanged = false);
				this.GBoost.Clear();
				this.Respawn = false;
				this.LiftMousePos = Vector3.zero;
				this.MinimapTargeted = false;
				this.ReverseGear = false;
				this.InverseReverse = false;
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

			public bool HasPressedWeaponsDriveOrChat()
			{
				return this.Up || this.Down || this.Left || this.Right || this.G0.Pressed || this.G1.Pressed || this.G2.Pressed || this.GG.Pressed || this.GBoost.Pressed || this.GSponsor.Pressed || this.ToggleChat || this.TogglePause || this.Bomb;
			}

			public bool HasGadgetInputChanged()
			{
				return this.G0.Changed || this.G1.Changed || this.G2.Changed || this.GG.Changed || this.GSponsor.Changed || this.GBoost.Changed || this.GHightlight.Changed;
			}

			public bool IsControllerActive()
			{
				return this.DrivingStyle == CarInput.DrivingStyleKind.Controller;
			}

			public bool IsFollowMouseActive()
			{
				return this.DrivingStyle == CarInput.DrivingStyleKind.FollowMouse;
			}

			public void WriteToBitStream(Pocketverse.BitStream bs)
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
			}

			public void ReadFromBitStream(Pocketverse.BitStream bs)
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
		}

		public delegate void GadgetInputPressedListener(GadgetSlot slot);
	}
}
