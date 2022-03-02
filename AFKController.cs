using System;
using System.Collections.Generic;
using System.Text;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Input.NoInputDetection.Business;
using HeavyMetalMachines.Match;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines
{
	public class AFKController : GameHubBehaviour, ISerializationCallbackReceiver, ClientReconnectMessage.IClientReconnectListener, IAFKManager
	{
		public List<AFKController.AFKEntry> Entries { get; private set; }

		private void Start()
		{
			this.Entries = new List<AFKController.AFKEntry>();
			this._disconnectionLimitSeconds = GameHubBehaviour.Hub.Config.GetFloatValue(ConfigAccess.AFKDisconnectionLimit);
			this._modifierLimitSeconds = GameHubBehaviour.Hub.Config.GetFloatValue(ConfigAccess.AFKModifierLimit);
			this._inputLimitSeconds = GameHubBehaviour.Hub.Config.GetFloatValue(ConfigAccess.AFKInputLimit);
			this._afkTimeSeconds = GameHubBehaviour.Hub.Config.GetFloatValue(ConfigAccess.AFKLimit);
			this._serverAfkTimeLimit = this._afkTimeSeconds + 10f;
			this._updater.PeriodMillis = 1000;
			this._noInputDetectedRpc = this._container.Resolve<INoInputDetectedRpc>();
			ObservableExtensions.Subscribe<AFKController.AFKEntry>(Observable.Do<AFKController.AFKEntry>(Observable.Where<AFKController.AFKEntry>(Observable.Select<byte, AFKController.AFKEntry>(this._noInputDetectedRpc.OnPlayerInputDisconnection, new Func<byte, AFKController.AFKEntry>(this.ConvertToInputDisconnectedAFKEntry)), (AFKController.AFKEntry entry) => entry != null), new Action<AFKController.AFKEntry>(this.OnEnteredAFK)));
		}

		private AFKController.AFKEntry ConvertToInputDisconnectedAFKEntry(byte playerAddress)
		{
			AFKController.AFKEntry afkentry;
			if (!this._entriesToCheck.TryGetValue(playerAddress, out afkentry))
			{
				return null;
			}
			afkentry.AfkReason = AFKController.AFKReason.INPUTDISCONNECTED;
			return afkentry;
		}

		public bool CheckLeaver(PlayerData playerData)
		{
			return this.CheckLeaver(playerData.PlayerAddress, playerData.PlayerId, playerData.UserSF.PublisherUserId);
		}

		public bool CheckLeaver(byte pPlayerAdress, long playerId, string publisherUserId)
		{
			AFKController.AFKEntry afkentry;
			if (!this._entriesToCheck.TryGetValue(pPlayerAdress, out afkentry))
			{
				AFKController.Log.ErrorFormat("Trying to check leaver for a player address={0} pid={1} uid={2} not accounted for in the afk controller.", new object[]
				{
					pPlayerAdress,
					playerId,
					publisherUserId
				});
				return false;
			}
			bool flag = afkentry.IsLeaver();
			if (flag && !afkentry.BILogged)
			{
				afkentry.BILogged = true;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("ActivityId={0}", GameHubBehaviour.Hub.Swordfish.Connection.ServerMatchId);
				stringBuilder.AppendFormat(" PublisherUserId={0}", publisherUserId);
				stringBuilder.AppendFormat(" PlayerId={0}", playerId);
				stringBuilder.AppendFormat(" Reason={0}", afkentry.LeaverReason);
				GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(4, stringBuilder.ToString(), false);
			}
			return flag;
		}

		public void ResetValues()
		{
			AFKController.Log.Debug("ResetValues");
			for (int i = 0; i < this.Entries.Count; i++)
			{
				AFKController.AFKEntry afkentry = this.Entries[i];
				afkentry.Reset();
			}
		}

		public float GetAFKTime(byte playerAddress)
		{
			AFKController.AFKEntry afkentry;
			if (!this._entriesToCheck.TryGetValue(playerAddress, out afkentry))
			{
				AFKController.Log.WarnFormat("Asked for AFKTime from NOT managed Address {0}", new object[]
				{
					playerAddress
				});
				return 0f;
			}
			return this._afkTimeSeconds - afkentry.AFKTime;
		}

		private void Update()
		{
			if (!GameHubBehaviour.Hub.Net.IsServer() || this._updater.ShouldHalt() || !GameHubBehaviour.Hub.Match.State.IsGame() || GameHubBehaviour.Hub.Match.State != MatchData.MatchState.MatchStarted || GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.BombDelivery || GameHubBehaviour.Hub.Match.LevelIsTutorial() || GameHubBehaviour.Hub.Match.Kind == 6 || (PauseController.Instance != null && PauseController.Instance.IsGamePaused))
			{
				return;
			}
			float num = (float)this._updater.LastDelta / 1000f;
			foreach (KeyValuePair<byte, AFKController.AFKEntry> keyValuePair in this._entriesToCheck)
			{
				AFKController.AFKEntry value = keyValuePair.Value;
				PlayerData player = value.Player;
				if (player.Connected)
				{
					if (value.Combat.SpawnController.State != SpawnStateKind.Unspawned)
					{
						if (value.IsAfk())
						{
							value.AFKTime += num;
						}
						else
						{
							value.InputLastMatchTime += num;
							if (value.InputLastMatchTime > this._inputLimitSeconds)
							{
								AFKController.Log.Debug("Time without input exceeded.");
								value.AfkReason = AFKController.AFKReason.INPUT;
								this.OnEnteredAFK(value);
							}
							value.ModifierLastMatchTime += num;
							int num2 = Mathf.FloorToInt(this._modifierLimitSeconds - value.ModifierLastMatchTime);
							if (num2 == 10)
							{
								this.TriggerModifierWarningEvent(value, (float)num2);
							}
							else if (num2 == 30)
							{
								this.TriggerModifierWarningEvent(value, (float)num2);
							}
						}
					}
				}
				else
				{
					if (!value.IsAfk())
					{
						AFKController.Log.Debug("Entering afk: disconnected");
						value.AfkReason = AFKController.AFKReason.DISCONNECTION;
						this.OnEnteredAFK(value);
					}
					value.DisconnectionTime += num;
				}
				if (!value.IsLeaver())
				{
					if (value.DisconnectionTime > this._disconnectionLimitSeconds)
					{
						AFKController.Log.Debug("Disconnection time exceeded.");
						value.LeaverReason = AFKController.AFKReason.DISCONNECTION;
						this.OnEnteredLeaver(value);
					}
					else if (value.AFKTime > this._serverAfkTimeLimit)
					{
						AFKController.Log.WarnFormat("AFK time exceeded before receiving timed window timeout. Penalizing player {0}", new object[]
						{
							value.PlayerAddress
						});
						value.LeaverReason = AFKController.AFKReason.AFKEXPIRATION;
						this.OnEnteredLeaver(value);
					}
					else if (value.ModifierLastMatchTime > this._modifierLimitSeconds)
					{
						AFKController.Log.Debug("Time without modifier exceeded.");
						value.LeaverReason = AFKController.AFKReason.MODIFIER;
						this.OnEnteredLeaver(value);
					}
				}
			}
		}

		private void TriggerModifierWarningEvent(AFKController.AFKEntry entry, float remainingModifierTime)
		{
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.LeaverModifierWarning,
				Killer = entry.Player.CharacterInstance.ObjId,
				MainReward = Mathf.FloorToInt(remainingModifierTime)
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		private void OnExitAFK(AFKController.AFKEntry entry)
		{
			AFKController.Log.InfoFormat("Player={0} leaving afk reason={1}", new object[]
			{
				entry.PlayerAddress,
				entry.AfkReason
			});
			entry.AfkReason = AFKController.AFKReason.NONE;
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.AFKEnd,
				Killer = entry.Player.CharacterInstance.Id.ObjId
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		private void OnEnteredAFK(AFKController.AFKEntry entry)
		{
			PlayerData player = entry.Player;
			Identifiable characterInstance = player.CharacterInstance;
			if (!player.IsBotControlled)
			{
				AFKController.Log.InfoFormat("Player {0} Adress {1} became AFK while connected. Activating Bot Controller. Reason: {2}", new object[]
				{
					entry.Player.CharacterInstance.ObjId,
					entry.PlayerAddress,
					entry.AfkReason
				});
				PlayerController bitComponent = characterInstance.GetBitComponent<PlayerController>();
				bitComponent.ActivateBotController();
			}
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.AFKGeneric,
				Killer = player.CharacterInstance.Id.ObjId,
				MainReward = Mathf.CeilToInt(this._afkTimeSeconds - entry.AFKTime)
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		private void OnEnteredLeaver(AFKController.AFKEntry entry)
		{
			AFKController.Log.InfoFormat("Player is now considered Leaver. Address: {0} - Reason: {1}", new object[]
			{
				entry.PlayerAddress,
				entry.LeaverReason
			});
			PlayerData player = entry.Player;
			player.IsLeaver = true;
			this._playersDispatcher.UpdatePlayer(player.PlayerCarId);
			string text = string.Empty;
			text += string.Format("SwordfishSessionID={0}", Guid.Empty);
			text += string.Format(" MatchID={0}", GameHubBehaviour.Hub.Swordfish.Connection.ServerMatchId);
			text += string.Format(" SteamID={0}", player.UserSF.UniversalID);
			text += string.Format(" EventAt={0}", DateTime.UtcNow);
			text += string.Format(" CurrentPunishmentLevel={0}", player.Bag.LeaverStatus);
			GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(13, text, false);
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.LeaverGeneric,
				MainReward = (int)entry.LeaverReason,
				Killer = player.CharacterInstance.Id.ObjId
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		public void AddEntry(byte connectionId)
		{
			AFKController.AFKEntry afkentry;
			if (this._entriesToCheck.TryGetValue(connectionId, out afkentry))
			{
				return;
			}
			afkentry = new AFKController.AFKEntry(connectionId);
			this._entriesToCheck.Add(connectionId, afkentry);
			this.Entries.Add(afkentry);
			AFKController.Log.DebugFormat("Added new entry for {0}", new object[]
			{
				connectionId
			});
		}

		public void OnClientReconnect(ClientReconnectMessage msg)
		{
			byte connectionId = msg.Session.ConnectionId;
			AFKController.Log.DebugFormat("OnClient reconnected={0}", new object[]
			{
				connectionId
			});
			AFKController.AFKEntry afkentry;
			if (!this._entriesToCheck.TryGetValue(connectionId, out afkentry))
			{
				List<PlayerData> narrators = GameHubBehaviour.Hub.Players.Narrators;
				for (int i = 0; i < narrators.Count; i++)
				{
					if (narrators[i].PlayerAddress == connectionId)
					{
						return;
					}
				}
				AFKController.Log.ErrorFormat("Client reconnected not added to afk controller! address={0}", new object[]
				{
					msg.Session.ConnectionId
				});
				return;
			}
			PlayerData player = afkentry.Player;
			if (!player.IsLeaver && afkentry.AFKTime > this._afkTimeSeconds)
			{
				afkentry.LeaverReason = AFKController.AFKReason.AFKEXPIRATION;
				this.OnEnteredLeaver(afkentry);
			}
		}

		public void AddModifier(CombatObject causer)
		{
			if (!GameHubBehaviour.Hub.Match.State.IsGame())
			{
				return;
			}
			byte playerAddress = causer.Player.PlayerAddress;
			AFKController.AFKEntry afkentry;
			if (!this._entriesToCheck.TryGetValue(playerAddress, out afkentry))
			{
				AFKController.Log.ErrorFormat("Player used modifier but he was not managed by AFKController. This is not the expected behaviour. Address: {0}  CauserName: {1} CharacterName: {2}", new object[]
				{
					playerAddress,
					causer.name,
					causer.Player.Character.name
				});
				return;
			}
			afkentry.ModifierLastMatchTime = 0f;
		}

		public void InputChanged(PlayerData player)
		{
			if (!GameHubBehaviour.Hub.Match.State.IsGame() || GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.Replay || GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.PreReplay)
			{
				return;
			}
			AFKController.AFKEntry afkentry;
			if (!this._entriesToCheck.TryGetValue(player.PlayerAddress, out afkentry))
			{
				if (!GameHubBehaviour.Hub.Net.isTest)
				{
					AFKController.Log.ErrorFormat("Player used an input but he was not managed by AFKController. This is not the expected behaviour. Address: {0}", new object[]
					{
						player.PlayerAddress
					});
				}
				return;
			}
			afkentry.InputLastMatchTime = 0f;
			if (player.IsBotControlled)
			{
				PlayerController bitComponent = player.CharacterInstance.GetBitComponent<PlayerController>();
				bitComponent.DeactivateBotController();
			}
			if (afkentry.IsAfk())
			{
				this.OnExitAFK(afkentry);
			}
		}

		public void LeaverWarningCallback(bool timedOut, byte playerAddress)
		{
			if (!GameHubBehaviour.Hub.Match.State.IsGame())
			{
				return;
			}
			AFKController.AFKEntry afkentry;
			if (!this._entriesToCheck.TryGetValue(playerAddress, out afkentry))
			{
				if (!GameHubBehaviour.Hub.Net.isTest)
				{
					AFKController.Log.ErrorFormat("Player sent LeaverWarningCallback but he was not managed by AFKController. This is not the expected behaviour. Address: {0}", new object[]
					{
						playerAddress
					});
				}
				return;
			}
			if (timedOut)
			{
				AFKController.Log.WarnFormat("AFK time exceeded. Penalizing player {0}", new object[]
				{
					afkentry.PlayerAddress
				});
				afkentry.LeaverReason = AFKController.AFKReason.AFKEXPIRATION;
				this.OnEnteredLeaver(afkentry);
			}
			else
			{
				this.InputChanged(afkentry.Player);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(AFKController));

		[Inject]
		private IMatchPlayersDispatcher _playersDispatcher;

		[Inject]
		private DiContainer _container;

		private INoInputDetectedRpc _noInputDetectedRpc;

		public float _disconnectionLimitSeconds;

		public float _modifierLimitSeconds;

		public float _inputLimitSeconds;

		public float _afkTimeSeconds;

		public float _serverAfkTimeLimit;

		private readonly Dictionary<byte, AFKController.AFKEntry> _entriesToCheck = new Dictionary<byte, AFKController.AFKEntry>();

		private TimedUpdater _updater;

		public enum AFKReason
		{
			NONE,
			DISCONNECTION,
			INPUT,
			MODIFIER,
			AFKEXPIRATION,
			INPUTDISCONNECTED
		}

		[Serializable]
		public class AFKEntry
		{
			public AFKEntry(byte pConnectionId)
			{
				this.PlayerAddress = pConnectionId;
			}

			public CombatObject Combat
			{
				get
				{
					if (this._combat == null)
					{
						this._combat = this.Player.CharacterInstance.GetBitComponent<CombatObject>();
					}
					return this._combat;
				}
			}

			public PlayerData Player
			{
				get
				{
					if (this._player == null)
					{
						this._player = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.PlayerAddress);
					}
					return this._player;
				}
			}

			public void Reset()
			{
				this.AFKTime = 0f;
				this.DisconnectionTime = 0f;
				this.ModifierLastMatchTime = 0f;
				this.InputLastMatchTime = 0f;
				this._player = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.PlayerAddress);
				this._combat = this.Player.CharacterInstance.GetBitComponent<CombatObject>();
				AFKController.Log.DebugFormat("Reseting for PlayerAddress: {0}", new object[]
				{
					this.PlayerAddress
				});
			}

			public bool IsAfk()
			{
				return this.AfkReason != AFKController.AFKReason.NONE;
			}

			public bool IsLeaver()
			{
				return this.LeaverReason != AFKController.AFKReason.NONE;
			}

			public byte PlayerAddress;

			public AFKController.AFKReason LeaverReason;

			public AFKController.AFKReason AfkReason;

			public float AFKTime;

			public float DisconnectionTime;

			public float ModifierLastMatchTime;

			public float InputLastMatchTime;

			public bool BILogged;

			private CombatObject _combat;

			private PlayerData _player;
		}
	}
}
