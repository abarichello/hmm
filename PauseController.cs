using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.HMM.GameStates.Game.ComponentContainer;
using Assets.Standard_Assets.Scripts.HMM.GameStates.Game.newPause.Api;
using FMod;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using Pocketverse;
using UnityEngine;
using Zenject;

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

		public PlayerData GetLastPlayerWhoToggled()
		{
			return GameHubBehaviour.Hub.Players.GetAnyByPlayerId(this._lastPlayerWhoToggled);
		}

		public PauseController.TeamPauseData GetPauseDataOnServer(TeamKind kind)
		{
			if (kind == TeamKind.Red)
			{
				return this._redPauseData;
			}
			return this._bluePauseData;
		}

		public PauseController.TeamPauseData GetPauseDataOnClient(TeamKind togglingTeam)
		{
			PauseController.TeamPauseData result;
			if (togglingTeam == TeamKind.Red)
			{
				result = this._redPauseData;
			}
			else
			{
				result = this._bluePauseData;
			}
			return result;
		}

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
			this._redPauseData = new PauseController.TeamPauseData();
			this._bluePauseData = new PauseController.TeamPauseData();
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
				if (this._inputActionPoller.GetButtonDown(20))
				{
					PauseController.Log.Debug("Trying to toggle pause feature");
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
							PlayerData anyByPlayerId = GameHubBehaviour.Hub.Players.GetAnyByPlayerId(this._lastPlayerWhoToggled);
							this.ChangePauseState(PauseController.PauseState.Unpaused, anyByPlayerId);
						}
					}
				}
				else if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.BombDelivery)
				{
					if (this._lastToggleSynchTime + Mathf.RoundToInt(1000f * this.PauseSettingsData.PauseCountDownTime) <= GameHubBehaviour.Hub.GameTime.GetSynchTime())
					{
						this.PauseServerTime();
						PlayerData anyByPlayerId2 = GameHubBehaviour.Hub.Players.GetAnyByPlayerId(this._lastPlayerWhoToggled);
						PauseController.TeamPauseData teamPauseData = this.GetPauseDataOnServer(anyByPlayerId2.Team);
						if (anyByPlayerId2.Team == TeamKind.Red)
						{
							teamPauseData = this._redPauseData;
						}
						else
						{
							teamPauseData = this._bluePauseData;
						}
						teamPauseData.TimeoutMillis = GameHubBehaviour.Hub.Clock.GetSynchTime() + teamPauseData.TimeRemainingMillis;
						this.ChangePauseState(PauseController.PauseState.Paused, anyByPlayerId2);
					}
				}
				else
				{
					this.ChangePauseState(PauseController.PauseState.Unpaused, GameHubBehaviour.Hub.Players.GetAnyByPlayerId(this._lastPlayerWhoToggled));
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
					this.DecreaseRemainingActivations(playerByAddress);
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
						if (this.PauseSettingsData.ShouldUseTimePool)
						{
							PlayerData anyByPlayerId = GameHubBehaviour.Hub.Players.GetAnyByPlayerId(this._lastPlayerWhoToggled);
							PauseController.TeamPauseData pauseDataOnServer = this.GetPauseDataOnServer(anyByPlayerId.Team);
							pauseDataOnServer.TimeRemainingMillis = pauseDataOnServer.TimeoutMillis - GameHubBehaviour.Hub.Clock.GetSynchTime();
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
			this._scoreboardDispatcher.Send();
		}

		private void UnpauseServerTime()
		{
			GameHubBehaviour.Hub.GameTime.SetTimeScale(this._beforePauseTimeScale);
			GameHubBehaviour.Hub.GameTime.MatchTimer.Start();
			this._scoreboardDispatcher.Send();
		}

		private bool CanPause(PlayerData playerData, out PauseController.PauseNotification reason)
		{
			reason.kind = PauseController.PauseNotificationKind.None;
			reason.delay = 0f;
			if (playerData == null)
			{
				PauseController.Log.WarnFormat("[QAHMM-13308] PlayerData is null! Address = {0}", new object[]
				{
					this.Sender
				});
				return false;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreboardState.BombDelivery)
			{
				reason.kind = PauseController.PauseNotificationKind.InvalidGameState;
				PauseController.Log.DebugFormat("Cannot pause game because bombState should be a valid state. CurrentState is: {0}", new object[]
				{
					GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState
				});
				return false;
			}
			PauseController.PauseState currentState = this.CurrentState;
			if (currentState == PauseController.PauseState.Paused)
			{
				reason.kind = PauseController.PauseNotificationKind.None;
				PauseController.Log.Debug("Cannot pause game because it's already paused.");
				return false;
			}
			if (currentState == PauseController.PauseState.PauseCountDown)
			{
				reason.kind = PauseController.PauseNotificationKind.PauseCountdown;
				reason.delay = Mathf.Max(0f, this.PauseSettingsData.PauseCountDownTime - (float)(GameHubBehaviour.Hub.GameTime.GetSynchTime() - this._lastToggleSynchTime) / 1000f);
				PauseController.Log.Debug("Cannot pause game because it's in countdown.");
				return false;
			}
			if (currentState == PauseController.PauseState.UnpauseCountDown)
			{
				reason.kind = PauseController.PauseNotificationKind.UnpauseCountdown;
				reason.delay = Mathf.Max(0f, this.PauseSettingsData.UnpauseCountDownTime - (float)(GameHubBehaviour.Hub.GameTime.GetSynchTime() - this._lastToggleSynchTime) / 1000f);
				PauseController.Log.Debug("Cannot pause game because it's in countdown.");
				return false;
			}
			if (playerData.IsNarrator)
			{
				reason.kind = PauseController.PauseNotificationKind.PlayerIsNarrator;
				PauseController.Log.DebugFormat("Cannot pause game because player is narrator. PlayerId:{0}", new object[]
				{
					playerData.PlayerId
				});
				return false;
			}
			int num;
			if (this._playersUnpauseSynchTime.TryGetValue(playerData.PlayerId, out num))
			{
				int num2 = num + Mathf.RoundToInt(1000f * this.PauseSettingsData.CooldownToPauseAgain) - GameHubBehaviour.Hub.GameTime.GetSynchTime();
				if (num2 > 0)
				{
					reason.kind = PauseController.PauseNotificationKind.PlayerCooldown;
					reason.delay = (float)num2 / 1000f;
					PauseController.Log.DebugFormat("Cannot pause game because player is still in cooldown. PlayerId:{0}", new object[]
					{
						playerData.PlayerId
					});
					return false;
				}
			}
			return this.CheckTeamTimeRemaining(playerData, ref reason) && this.CheckTeamActivationRemaining(playerData, ref reason);
		}

		private bool CheckTeamActivationRemaining(PlayerData playerData, ref PauseController.PauseNotification reason)
		{
			int activationRemaining;
			if (playerData.Team == TeamKind.Red)
			{
				activationRemaining = this._redPauseData.ActivationRemaining;
			}
			else
			{
				activationRemaining = this._bluePauseData.ActivationRemaining;
			}
			if (this.PauseSettingsData.ShouldCheckActivationLimit && activationRemaining <= 0)
			{
				reason.kind = PauseController.PauseNotificationKind.TeamOutOfActivation;
				reason.delay = 1f;
				PauseController.Log.DebugFormat("Cannot pause game because team don't have remaining activations. PlayerId:{0} Team {1}", new object[]
				{
					playerData.PlayerId,
					playerData.Team
				});
				return false;
			}
			return true;
		}

		private void DecreaseRemainingActivations(PlayerData playerData)
		{
			if (!this.PauseSettingsData.ShouldCheckActivationLimit)
			{
				return;
			}
			if (playerData.Team == TeamKind.Red)
			{
				this._redPauseData.ActivationRemaining = this._redPauseData.ActivationRemaining - 1;
			}
			else
			{
				this._bluePauseData.ActivationRemaining = this._bluePauseData.ActivationRemaining - 1;
			}
		}

		private bool CheckTeamTimeRemaining(PlayerData playerData, ref PauseController.PauseNotification reason)
		{
			PauseController.TeamPauseData pauseDataOnServer = this.GetPauseDataOnServer(playerData.Team);
			if (this.PauseSettingsData.ShouldUseTimePool && pauseDataOnServer.TimeRemainingMillis <= 0)
			{
				reason.kind = PauseController.PauseNotificationKind.TeamOutOfTime;
				reason.delay = 1f;
				PauseController.Log.DebugFormat("Cannot pause game because team don't have remaining time pool. PlayerId:{0} Team {1} TimeoutMillis {2} GetSynchTime {3}", new object[]
				{
					playerData.PlayerId,
					playerData.Team,
					pauseDataOnServer.TimeoutMillis,
					GameHubBehaviour.Hub.Clock.GetSynchTime()
				});
				return false;
			}
			return true;
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
			if (playerData.IsNarrator)
			{
				reason.kind = PauseController.PauseNotificationKind.PlayerIsNarrator;
				PauseController.Log.DebugFormat("Cannot pause game because player is narrator. PlayerId:{0}", new object[]
				{
					playerData.PlayerId
				});
				return false;
			}
			PauseController.PauseState currentState = this.CurrentState;
			if (currentState == PauseController.PauseState.Unpaused)
			{
				reason.kind = PauseController.PauseNotificationKind.None;
				PauseController.Log.Debug("Cannot unpause game because it's already unpaused.");
				return false;
			}
			if (currentState == PauseController.PauseState.PauseCountDown)
			{
				reason.kind = PauseController.PauseNotificationKind.PauseCountdown;
				PauseController.Log.Debug("Cannot unpause game because it's in countdown.");
				return false;
			}
			if (currentState == PauseController.PauseState.UnpauseCountDown)
			{
				reason.kind = PauseController.PauseNotificationKind.UnpauseCountdown;
				PauseController.Log.Debug("Cannot unpause game because it's in countdown.");
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
				PauseController.Log.DebugFormat("Cannot unpause game because team {0}, still need to wait {1} seconds to unpause", new object[]
				{
					playerData.Team,
					reason.delay
				});
				return false;
			}
			reason.kind = PauseController.PauseNotificationKind.None;
			return true;
		}

		public void TriggerPauseNotification(PauseController.PauseNotification notification)
		{
			PauseController.Log.DebugFormat("Pause server Trigger. Kind:{0}", new object[]
			{
				notification.kind
			});
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
			PauseController.TeamPauseData pauseDataOnServer = this.GetPauseDataOnServer(playerByAddress.Team);
			this.DispatchReliable(new byte[]
			{
				playerAddress
			}).ChangePauseStateOnClient((int)this.CurrentState, this._lastPlayerWhoToggled, pauseDataOnServer.TimeRemainingMillis, pauseDataOnServer.TimeoutMillis, pauseDataOnServer.ActivationRemaining);
			PauseController.Log.DebugFormat("PauseData sent to PlayerAddress:{0}", new object[]
			{
				playerByAddress.PlayerAddress
			});
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
			long num = -1L;
			if (playerData == null)
			{
				PauseController.Log.DebugFormat("PauseState changed to {0}", new object[]
				{
					newState
				});
			}
			else if (playerData.IsNarrator)
			{
				PauseController.Log.DebugFormat("PauseState changed to {0}, as requested by narrator", new object[]
				{
					newState
				});
				num = playerData.PlayerId;
			}
			else if (playerData.CharacterInstance != null)
			{
				num = playerData.PlayerId;
				PauseController.Log.DebugFormat("PauseState changed to {0}, as requested by player (playerId: {1})", new object[]
				{
					newState,
					num
				});
			}
			else
			{
				PauseController.Log.DebugFormat("Something happen! i think... maybe not... {0}", new object[]
				{
					playerData
				});
			}
			PauseController.TeamPauseData pauseDataOnServer = this.GetPauseDataOnServer(playerData.Team);
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).ChangePauseStateOnClient((int)newState, num, pauseDataOnServer.TimeRemainingMillis, pauseDataOnServer.TimeoutMillis, pauseDataOnServer.ActivationRemaining);
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
		private void ChangePauseStateOnClient(int newState, long playerId, int timeRemaining, int timeoutMillis, int activations)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				throw new AccessViolationException("ChangePauseStateOnClient called on server");
			}
			PauseController.Log.DebugFormat("previous={0} new={1}", new object[]
			{
				this.CurrentState,
				newState
			});
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
			this._lastPlayerWhoToggled = playerId;
			if (anyByPlayerId != null)
			{
				showAsBlue = (GameHubBehaviour.Hub.Players.CurrentPlayerTeam == anyByPlayerId.Team);
				if (anyByPlayerId.Team == TeamKind.Red)
				{
					this._redPauseData.TimeoutMillis = timeoutMillis;
					this._redPauseData.ActivationRemaining = activations;
					this._redPauseData.TimeRemainingMillis = timeRemaining;
				}
				else
				{
					this._bluePauseData.TimeoutMillis = timeoutMillis;
					this._bluePauseData.ActivationRemaining = activations;
					this._bluePauseData.TimeRemainingMillis = timeRemaining;
				}
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
				HudInGamePause.Instance.ClearTeamLabels();
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
				PauseController.Log.DebugFormat("Changed Match Kind {0} on time {1}", new object[]
				{
					GameHubBehaviour.Hub.Match.Kind,
					GameHubBehaviour.Hub.GameTime.GetSynchTime()
				});
				this.PauseSettingsData = this._pauseSettings.GetPauseData(GameHubBehaviour.Hub.Match.Kind);
				this._redPauseData.TimeRemainingMillis = (int)(this.PauseSettingsData.InitialTimePoolForTeam * 1000f);
				this._bluePauseData.TimeRemainingMillis = (int)(this.PauseSettingsData.InitialTimePoolForTeam * 1000f);
				this._redPauseData.ActivationRemaining = this.PauseSettingsData.MaxActivationsPerTeam;
				this._bluePauseData.ActivationRemaining = this.PauseSettingsData.MaxActivationsPerTeam;
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

		public object Invoke(int classId, short methodId, object[] args, BitStream bitstream = null)
		{
			this._delayed = null;
			if (methodId == 7)
			{
				this.TogglePauseServer();
				return null;
			}
			if (methodId == 16)
			{
				this.TriggerPauseNotification((int)args[0], (float)args[1]);
				return null;
			}
			if (methodId != 20)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.ChangePauseStateOnClient((int)args[0], (long)args[1], (int)args[2], (int)args[3], (int)args[4]);
			return null;
		}

		[SerializeField]
		private PauseSettings _pauseSettings;

		public static readonly BitLogger Log = new BitLogger(typeof(PauseController));

		[Inject]
		private IScoreboardDispatcher _scoreboardDispatcher;

		[NonSerialized]
		public PauseSettings.PauseData PauseSettingsData;

		private int _lastToggleSynchTime;

		private long _lastPlayerWhoToggled;

		private float _beforePauseTimeScale;

		private readonly Dictionary<long, int> _playersUnpauseSynchTime = new Dictionary<long, int>(8);

		[InjectOnClient]
		private IControllerInputActionPoller _inputActionPoller;

		private PauseController.TeamPauseData _redPauseData;

		private PauseController.TeamPauseData _bluePauseData;

		public const int StaticClassId = 1028;

		private Identifiable _identifiable;

		[ThreadStatic]
		private PauseControllerAsync _async;

		[ThreadStatic]
		private PauseControllerDispatch _dispatch;

		private IFuture _delayed;

		public delegate void InGamePauseStateChangedEvent(PauseController.PauseState oldState, PauseController.PauseState newState, PlayerData playerData);

		public delegate void InGameOPauseCountdownEvent(int countdown);

		public class TeamPauseData
		{
			public int TimeoutMillis;

			public int ActivationRemaining;

			public int TimeRemainingMillis;
		}

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
			PlayerCooldown,
			TeamOutOfTime,
			TeamOutOfActivation,
			PlayerIsNarrator
		}

		public struct PauseNotification
		{
			public PauseController.PauseNotificationKind kind;

			public float delay;
		}
	}
}
