using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.HMM.GameStates.Game.ComponentContainer;
using Assets.Standard_Assets.Scripts.HMM.GameStates.Game.newPause.Api;
using FMod;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	[RemoteClass]
	[RequireComponent(typeof(Identifiable))]
	public class PauseController : GameHubBehaviour, IBitComponent
	{
		public static PauseController Instance { get; private set; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action<PauseController.PauseNotification> OnNotification;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event PauseController.InGamePauseStateChangedEvent OnInGamePauseStateChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event PauseController.InGameOPauseCountdownEvent OnInGameCountdownNotification;

		public PauseController.PauseState CurrentState { get; private set; }

		public bool IsGamePaused
		{
			get
			{
				return this.CurrentState == PauseController.PauseState.Paused || this.CurrentState == PauseController.PauseState.UnpauseCountDown;
			}
		}

		public bool IsOnPauseCountdown
		{
			get
			{
				return this.CurrentState == PauseController.PauseState.PauseCountDown;
			}
		}

		private void Awake()
		{
			PauseController.Instance = this;
			this.CurrentState = PauseController.PauseState.Unpaused;
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.OnListenToStateChanged;
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.OnListenToStateChanged;
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				if (ControlOptions.GetButtonDown(ControlAction.Pause))
				{
					IPauseComponent hmmComponent = HmmUnityLifeCicleAdapter.Instance.GetHmmComponent<IPauseComponent>();
					if (!hmmComponent.TooglePause())
					{
						this.DispatchReliable(new byte[0]).TogglePauseServer();
					}
				}
			}
			else
			{
				PauseController.PauseState currentState = this.CurrentState;
				if (currentState != PauseController.PauseState.PauseCountDown)
				{
					if (currentState == PauseController.PauseState.UnpauseCountDown)
					{
						if (this._lastToggleSynchTime + Mathf.RoundToInt(1000f * this.PauseSettingsData.PauseCountDownTime) <= GameHubBehaviour.Hub.GameTime.GetSynchTime())
						{
							this.UnpauseServerTime();
							this.ChangePauseState(PauseController.PauseState.Unpaused, GameHubBehaviour.Hub.Players.GetAnyByPlayerId(this._lastPlayerWhoToggled));
						}
					}
				}
				else if (this._lastToggleSynchTime + Mathf.RoundToInt(1000f * this.PauseSettingsData.PauseCountDownTime) <= GameHubBehaviour.Hub.GameTime.GetSynchTime())
				{
					if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.BombDelivery)
					{
						this.PauseServerTime();
						this.ChangePauseState(PauseController.PauseState.Paused, GameHubBehaviour.Hub.Players.GetAnyByPlayerId(this._lastPlayerWhoToggled));
					}
					else
					{
						this.ChangePauseState(PauseController.PauseState.Unpaused, null);
					}
				}
			}
		}

		[RemoteMethod]
		public void TogglePauseServer()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				throw new AccessViolationException("TogglePauseServer called on client");
			}
			if (this._pauseSettings == null)
			{
				PauseController.Log.Error("Pause settings is null");
				return;
			}
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			switch (this.CurrentState)
			{
			case PauseController.PauseState.Unpaused:
			case PauseController.PauseState.PauseCountDown:
			{
				PauseController.PauseNotification notification;
				if (this.CanPause(playerByAddress, out notification))
				{
					this._lastToggleSynchTime = GameHubBehaviour.Hub.GameTime.GetSynchTime();
					this._lastPlayerWhoToggled = playerByAddress.PlayerId;
					this.ChangePauseState(PauseController.PauseState.PauseCountDown, playerByAddress);
				}
				else
				{
					this.TriggerPauseNotification(notification);
					this.DispatchReliable(new byte[]
					{
						this.Sender
					}).TriggerPauseNotification((int)notification.kind, notification.delay);
				}
				break;
			}
			case PauseController.PauseState.Paused:
			case PauseController.PauseState.UnpauseCountDown:
			{
				PauseController.PauseNotification notification;
				if (this.CanUnpause(playerByAddress, out notification))
				{
					this._lastToggleSynchTime = GameHubBehaviour.Hub.GameTime.GetSynchTime();
					if (!playerByAddress.IsNarrator)
					{
						if (this._playersUnpauseSynchTime.ContainsKey(this._lastPlayerWhoToggled))
						{
							this._playersUnpauseSynchTime[this._lastPlayerWhoToggled] = this._lastToggleSynchTime;
						}
						else
						{
							this._playersUnpauseSynchTime.Add(this._lastPlayerWhoToggled, this._lastToggleSynchTime);
						}
					}
					this._lastPlayerWhoToggled = playerByAddress.PlayerId;
					this.ChangePauseState(PauseController.PauseState.UnpauseCountDown, playerByAddress);
				}
				else
				{
					this.TriggerPauseNotification(notification);
					this.DispatchReliable(new byte[]
					{
						this.Sender
					}).TriggerPauseNotification((int)notification.kind, notification.delay);
				}
				break;
			}
			}
		}

		private void PauseServerTime()
		{
			this._beforePauseTimeScale = Time.timeScale;
			GameHubBehaviour.Hub.GameTime.SetTimeScale(0f);
			GameHubBehaviour.Hub.GameTime.MatchTimer.Stop();
			PlaybackManager.Scoreboard.Send();
		}

		private void UnpauseServerTime()
		{
			GameHubBehaviour.Hub.GameTime.SetTimeScale(this._beforePauseTimeScale);
			GameHubBehaviour.Hub.GameTime.MatchTimer.Start();
			PlaybackManager.Scoreboard.Send();
		}

		private bool CanPause(PlayerData playerData, out PauseController.PauseNotification reason)
		{
			reason.delay = 0f;
			if (playerData == null)
			{
				reason.kind = PauseController.PauseNotificationKind.None;
				PauseController.Log.WarnFormat("[QAHMM-13308] PlayerData is null! Address = {0}", new object[]
				{
					this.Sender
				});
				return false;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreBoard.State.BombDelivery)
			{
				reason.kind = PauseController.PauseNotificationKind.InvalidGameState;
				return false;
			}
			PauseController.PauseState currentState = this.CurrentState;
			if (currentState == PauseController.PauseState.Paused)
			{
				reason.kind = PauseController.PauseNotificationKind.None;
				return false;
			}
			if (currentState == PauseController.PauseState.PauseCountDown)
			{
				reason.kind = PauseController.PauseNotificationKind.PauseCountdown;
				reason.delay = Mathf.Max(0f, this.PauseSettingsData.PauseCountDownTime - (float)(GameHubBehaviour.Hub.GameTime.GetSynchTime() - this._lastToggleSynchTime) / 1000f);
				return false;
			}
			if (currentState != PauseController.PauseState.UnpauseCountDown)
			{
				int num;
				if (!playerData.IsNarrator && this._playersUnpauseSynchTime.TryGetValue(playerData.PlayerId, out num))
				{
					int num2 = num + Mathf.RoundToInt(1000f * this.PauseSettingsData.CooldownToPauseAgain) - GameHubBehaviour.Hub.GameTime.GetSynchTime();
					if (num2 > 0)
					{
						reason.kind = PauseController.PauseNotificationKind.PlayerCooldown;
						reason.delay = (float)num2 / 1000f;
						return false;
					}
				}
				reason.kind = PauseController.PauseNotificationKind.None;
				reason.delay = 0f;
				return true;
			}
			reason.kind = PauseController.PauseNotificationKind.UnpauseCountdown;
			reason.delay = Mathf.Max(0f, this.PauseSettingsData.UnpauseCountDownTime - (float)(GameHubBehaviour.Hub.GameTime.GetSynchTime() - this._lastToggleSynchTime) / 1000f);
			return false;
		}

		private bool CanUnpause(PlayerData playerData, out PauseController.PauseNotification reason)
		{
			reason.delay = 0f;
			if (playerData == null)
			{
				reason.kind = PauseController.PauseNotificationKind.None;
				PauseController.Log.WarnFormat("[QAHMM-13308] PlayerData is null! Address = {0}", new object[]
				{
					this.Sender
				});
				return false;
			}
			PauseController.PauseState currentState = this.CurrentState;
			if (currentState == PauseController.PauseState.Unpaused)
			{
				reason.kind = PauseController.PauseNotificationKind.None;
				return false;
			}
			if (currentState == PauseController.PauseState.PauseCountDown)
			{
				reason.kind = PauseController.PauseNotificationKind.PauseCountdown;
				return false;
			}
			if (currentState == PauseController.PauseState.UnpauseCountDown)
			{
				reason.kind = PauseController.PauseNotificationKind.UnpauseCountdown;
				return false;
			}
			float num = (!GameHubBehaviour.Hub.Players.IsSomeoneDisconnected(TeamKind.Red)) ? 0f : 1000f;
			float num2 = (!GameHubBehaviour.Hub.Players.IsSomeoneDisconnected(TeamKind.Blue)) ? 0f : 1000f;
			float num3 = 0f;
			TeamKind team = playerData.Team;
			if (team != TeamKind.Blue)
			{
				if (team == TeamKind.Red)
				{
					num3 = Mathf.Max(num * this.PauseSettingsData.AlliedDelayToUnpause, num2 * this.PauseSettingsData.EnemyDelayToUnpause);
				}
			}
			else
			{
				num3 = Mathf.Max(num * this.PauseSettingsData.EnemyDelayToUnpause, num2 * this.PauseSettingsData.AlliedDelayToUnpause);
			}
			int synchTime = GameHubBehaviour.Hub.GameTime.GetSynchTime();
			if (this._lastToggleSynchTime + Mathf.RoundToInt(num3) > synchTime)
			{
				reason.kind = PauseController.PauseNotificationKind.UnPauseDelay;
				reason.delay = ((float)this._lastToggleSynchTime + num3 - (float)synchTime) / 1000f;
				return false;
			}
			reason.kind = PauseController.PauseNotificationKind.None;
			return true;
		}

		public void TriggerPauseNotification(PauseController.PauseNotification notification)
		{
			if (PauseController.OnNotification != null)
			{
				PauseController.OnNotification(notification);
			}
		}

		[RemoteMethod]
		private void TriggerPauseNotification(int kind, float delay)
		{
			this.TriggerPauseNotification(new PauseController.PauseNotification
			{
				kind = (PauseController.PauseNotificationKind)kind,
				delay = delay
			});
		}

		public void SendCurrentPauseData(byte playerAddress)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				throw new AccessViolationException("SendCurrentPauseData called by client");
			}
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(playerAddress);
			this.DispatchReliable(new byte[]
			{
				playerAddress
			}).ChangePauseStateOnClient((int)this.CurrentState, this._lastPlayerWhoToggled);
		}

		private void ChangePauseState(PauseController.PauseState newState, PlayerData playerData)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				throw new AccessViolationException("ChangePauseState called on client");
			}
			if (this.CurrentState == newState)
			{
				PauseController.Log.WarnFormat("Tried to switch to the same state: {0}", new object[]
				{
					newState
				});
				return;
			}
			long playerId = -1L;
			if (!(playerData == null))
			{
				if (playerData.IsNarrator)
				{
					playerId = playerData.PlayerId;
				}
				else if (playerData.CharacterInstance != null)
				{
					playerId = playerData.PlayerId;
				}
			}
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).ChangePauseStateOnClient((int)newState, playerId);
			PauseController.PauseState currentState = this.CurrentState;
			this.CurrentState = newState;
			if (PauseController.OnInGamePauseStateChanged != null)
			{
				PauseController.OnInGamePauseStateChanged(currentState, newState, playerData);
			}
		}

		public void InvokeInGameCountdownNotification(int countdown)
		{
			if (PauseController.OnInGameCountdownNotification != null)
			{
				PauseController.OnInGameCountdownNotification(countdown);
			}
		}

		[RemoteMethod]
		private void ChangePauseStateOnClient(int newState, long playerId)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				throw new AccessViolationException("ChangePauseStateOnClient called on server");
			}
			PauseController.PauseState currentState = this.CurrentState;
			if (currentState != PauseController.PauseState.PauseCountDown)
			{
				if (currentState == PauseController.PauseState.UnpauseCountDown)
				{
					HudInGamePause.Instance.ShowPauseState(false, 0f);
				}
			}
			else
			{
				HudPauseCountdown.Instance.HidePauseCountdown();
			}
			bool showAsBlue = true;
			PlayerData anyByPlayerId = GameHubBehaviour.Hub.Players.GetAnyByPlayerId(playerId);
			if (anyByPlayerId != null)
			{
				showAsBlue = (GameHubBehaviour.Hub.Players.CurrentPlayerTeam == anyByPlayerId.Team);
			}
			if (this.CurrentState == PauseController.PauseState.Unpaused && newState == 1)
			{
				HudPauseCountdown.Instance.ShowCountdownToPause(0f, showAsBlue, false);
			}
			switch (newState)
			{
			case 0:
				FMODAudioManager.Resume();
				HudInGamePause.Instance.ShowBorderOverlay(false);
				break;
			case 1:
				FMODAudioManager.Pause();
				HudInGamePause.Instance.ShowPauseState(true, 0f);
				break;
			case 2:
				HudPauseCountdown.Instance.ShowCountdownToPause(this.PauseSettingsData.PauseCountDownTime, showAsBlue, true);
				break;
			case 3:
				HudInGamePause.Instance.StartUnpauseCountdownTimer(this.PauseSettingsData.UnpauseCountDownTime);
				break;
			}
			PauseController.PauseState currentState2 = this.CurrentState;
			this.CurrentState = (PauseController.PauseState)newState;
			if (PauseController.OnInGamePauseStateChanged != null)
			{
				PauseController.OnInGamePauseStateChanged(currentState2, (PauseController.PauseState)newState, anyByPlayerId);
			}
		}

		private void OnListenToStateChanged(GameState newGameState)
		{
			if (newGameState.StateKind == GameState.GameStateKind.Game)
			{
				this.PauseSettingsData = this._pauseSettings.GetPauseData(GameHubBehaviour.Hub.Match.Kind);
			}
		}

		private int OID
		{
			get
			{
				if (!this._identifiable)
				{
					this._identifiable = base.GetComponent<Identifiable>();
				}
				return this._identifiable.ObjId;
			}
		}

		public byte Sender { get; set; }

		public IPauseControllerAsync Async()
		{
			return this.Async(0);
		}

		public IPauseControllerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PauseControllerAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPauseControllerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PauseControllerDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPauseControllerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PauseControllerDispatch(this.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		protected IFuture Delayed
		{
			get
			{
				return this._delayed;
			}
		}

		protected void Delay(IFuture future)
		{
			this._delayed = future;
		}

		public object Invoke(int classId, short methodId, object[] args)
		{
			if (classId != 1027)
			{
				throw new Exception("Hierarchy in RemoteClass is not allowed!!! " + classId);
			}
			this._delayed = null;
			if (methodId == 4)
			{
				this.TogglePauseServer();
				return null;
			}
			if (methodId == 10)
			{
				this.TriggerPauseNotification((int)args[0], (float)args[1]);
				return null;
			}
			if (methodId != 14)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.ChangePauseStateOnClient((int)args[0], (long)args[1]);
			return null;
		}

		[SerializeField]
		private PauseSettings _pauseSettings;

		public static readonly BitLogger Log = new BitLogger(typeof(PauseController));

		[NonSerialized]
		public PauseSettings.PauseData PauseSettingsData;

		private int _lastToggleSynchTime;

		private long _lastPlayerWhoToggled;

		private float _beforePauseTimeScale;

		private readonly Dictionary<long, int> _playersUnpauseSynchTime = new Dictionary<long, int>(8);

		public const int StaticClassId = 1027;

		private Identifiable _identifiable;

		[ThreadStatic]
		private PauseControllerAsync _async;

		[ThreadStatic]
		private PauseControllerDispatch _dispatch;

		private IFuture _delayed;

		public delegate void InGamePauseStateChangedEvent(PauseController.PauseState oldState, PauseController.PauseState newState, PlayerData playerData);

		public delegate void InGameOPauseCountdownEvent(int countdown);

		public enum PauseState
		{
			Unpaused,
			Paused,
			PauseCountDown,
			UnpauseCountDown
		}

		public enum PauseNotificationKind
		{
			None,
			InvalidGameState,
			UnPauseDelay,
			InputBlocked,
			PauseCountdown,
			UnpauseCountdown,
			PlayerCooldown
		}

		public struct PauseNotification
		{
			public PauseController.PauseNotificationKind kind;

			public float delay;
		}
	}
}
