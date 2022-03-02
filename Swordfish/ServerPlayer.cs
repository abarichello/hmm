using System;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Utils;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using Swordfish.Common.exceptions;

namespace HeavyMetalMachines.Swordfish
{
	public class ServerPlayer : GameHubObject
	{
		public static void GetServerStartData(string[] userIds, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			string text = Json.ToJSON(userIds);
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetServerStartData", text, onSuccess, onError);
		}

		public static void GetServerPlayerItems(long playerId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			string text = Json.ToJSON(playerId);
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetServerPlayerItems", text, onSuccess, onError);
		}

		public static void GetPlayer(long playerId, SwordfishClientApi.ParameterizedCallback<Player> onPlayerTaken, SwordfishClientApi.ErrorCallback onPlayerError)
		{
			GameHubObject.Hub.ClientApi.user.GetPlayer(null, playerId, onPlayerTaken, onPlayerError);
		}

		public static void SavePlayer(Player player, SwordfishClientApi.Callback onSave, SwordfishClientApi.ErrorCallback onError)
		{
			ServerPlayer.Log.DebugFormat("SavePlayer={0} playerBag={1}", new object[]
			{
				player.Id,
				player.Bag
			});
			GameHubObject.Hub.ClientApi.player.UpdatePlayer(null, player, onSave, onError);
		}

		private static void SaveBag(long playerId, PlayerBag pbag, long bagversion, SwordfishClientApi.ParameterizedCallback<long> onSave, SwordfishClientApi.ErrorCallback onError)
		{
			string text = (string)pbag;
			ServerPlayer.Log.DebugFormat("SaveBag PlayerId = {0} Bag = {1} BagVersion = {2}", new object[]
			{
				playerId,
				pbag,
				bagversion
			});
			BagWrapper bagWrapper = new BagWrapper(playerId, text, bagversion);
			GameHubObject.Hub.ClientApi.player.UpdatePlayerBag(playerId, bagWrapper, onSave, onError);
		}

		public static void ServerClearBagAndGiveRewards(PlayerData player, Action<NetResult> callback)
		{
			EventUtils.SActionCallbackConcainer<ServerPlayer.ServerClearBagAndRewardOperation, NetResult> sactionCallbackConcainer = new EventUtils.SActionCallbackConcainer<ServerPlayer.ServerClearBagAndRewardOperation, NetResult>(callback);
			ServerPlayer.ServerClearBagAndRewardOperation oOperation = new ServerPlayer.ServerClearBagAndRewardOperation(player, new Action<NetResult>(sactionCallbackConcainer.OnAction));
			sactionCallbackConcainer.Set(oOperation);
		}

		public static void SetCurrentServer(long playerId, int port, string ip, string matchId, string groupId, bool isNarrator, Action<NetResult> callback)
		{
			EventUtils.SActionCallbackConcainer<ServerPlayer.SetServerOperation, NetResult> sactionCallbackConcainer = new EventUtils.SActionCallbackConcainer<ServerPlayer.SetServerOperation, NetResult>(callback);
			ServerPlayer.SetServerOperation oOperation = new ServerPlayer.SetServerOperation(playerId, port, ip, matchId, groupId, isNarrator, new Action<NetResult>(sactionCallbackConcainer.OnAction));
			sactionCallbackConcainer.Set(oOperation);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ServerPlayer));

		private class ServerClearBagAndRewardOperation : AsyncOperation
		{
			public ServerClearBagAndRewardOperation(PlayerData playerData, Action<NetResult> callback) : base(callback)
			{
				AsyncOperation.Log.DebugFormat("[Progression] [PlayerId={0}] Clearing Match for player Data={1}", new object[]
				{
					playerData.PlayerId,
					playerData
				});
				this.Result = new NetResult
				{
					Success = false
				};
				this._playerData = playerData;
				ServerPlayer.GetPlayer(playerData.PlayerId, new SwordfishClientApi.ParameterizedCallback<Player>(this.OnPlayerTaken), new SwordfishClientApi.ErrorCallback(base.OnError));
			}

			protected override string ErrorLog
			{
				get
				{
					return string.Format("Failed to clear server bag for={0}", this._playerData.PlayerId);
				}
			}

			protected override string ErrorMsg
			{
				get
				{
					return "Clear bag operation failed.";
				}
			}

			protected override string SuccessMsg
			{
				get
				{
					return "Clear bag operation success.";
				}
			}

			private bool ClearServer(ref PlayerBag bag)
			{
				if (bag.CurrentPort != GameHubObject.Hub.Server.ServerPort || string.Compare(bag.CurrentServerIp, GameHubObject.Hub.Swordfish.Connection.GetIp()) != 0)
				{
					AsyncOperation.Log.DebugFormat("Player={0} no longer connected to this server, no changes needeed to player bag.", new object[]
					{
						this._playerData.PlayerId
					});
					return false;
				}
				bag.CurrentGroupId = null;
				bag.CurrentMatchId = null;
				bag.CurrentServerIp = null;
				bag.CurrentPort = 0;
				bag.CurrentIsNarrator = false;
				AsyncOperation.Log.DebugFormat("Player={0} bag match info cleared.", new object[]
				{
					this._playerData.PlayerId
				});
				return true;
			}

			private void OnPlayerTaken(object state, Player player)
			{
				if (player == null)
				{
					base.OnError(state, new NullReferenceException("Server returned null player."));
					return;
				}
				this._player = player;
				AsyncOperation.Log.DebugFormat("[Progression] [PlayerId={0}] PlayerTaken bag={1}", new object[]
				{
					this._player.Id,
					this._player.Bag
				});
				PlayerBag playerBag = (PlayerBag)this._player.Bag;
				this.SendFinishedManyMatchesEvent(playerBag.EndMatchPresenceCounter, this._player.Id);
				this.ClearServer(ref playerBag);
				ServerPlayer.SaveBag(this._player.Id, playerBag, this._player.BagVersion, new SwordfishClientApi.ParameterizedCallback<long>(this.OnSaveBagSuccess), new SwordfishClientApi.ErrorCallback(this.ConcurrentBagUpdateOnPlayerTaken));
				base.OnSuccess(state, 0L);
			}

			private void SendFinishedManyMatchesEvent(int endMatchPresenceCounter, long playerId)
			{
				if (!GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.EnableHoplonTTEvent))
				{
					return;
				}
				if (endMatchPresenceCounter < 3)
				{
					return;
				}
				PlayerData anyByPlayerId = GameHubObject.Hub.Players.GetAnyByPlayerId(playerId);
				if (anyByPlayerId == null)
				{
					AsyncOperation.Log.ErrorFormat("Could not found expected player with id={0} while sending FinishedManyMatchesEvent", new object[]
					{
						playerId
					});
					return;
				}
				string universalID = anyByPlayerId.UserSF.UniversalID;
				string value = GameHubObject.Hub.Config.GetValue(ConfigAccess.HoplonTTEventUrl);
				HoplonTrackingTool.FinishedManyMatches(value, universalID);
			}

			private void OnSaveBagSuccess(object state, long l)
			{
				AsyncOperation.Log.Info(string.Format("Player bag Updated {0}", this._player.Id));
			}

			private void ConcurrentBagUpdateOnPlayerTaken(object state, Exception exception)
			{
				if (exception is BagVersionException)
				{
					long playerId = (long)state;
					ServerPlayer.GetPlayer(playerId, new SwordfishClientApi.ParameterizedCallback<Player>(this.OnPlayerTaken), new SwordfishClientApi.ErrorCallback(base.OnError));
					return;
				}
				base.OnError(state, exception);
			}

			private Player _player;

			private PlayerData _playerData;
		}

		private class SetServerOperation : AsyncOperation
		{
			public SetServerOperation(long playerId, int port, string ip, string matchId, string groupId, bool isNarrator, Action<NetResult> callback) : base(callback)
			{
				this.PlayerId = playerId;
				this.Port = port;
				this.Ip = ip;
				this.MatchId = matchId;
				this.GroupId = groupId;
				this.IsNarrator = isNarrator;
				ServerPlayer.GetPlayer(playerId, new SwordfishClientApi.ParameterizedCallback<Player>(this.OnPlayerTaken), new SwordfishClientApi.ErrorCallback(base.OnError));
			}

			protected override string ErrorLog
			{
				get
				{
					return string.Format("Failed to set bag for player={0}.", this.PlayerId);
				}
			}

			protected override string ErrorMsg
			{
				get
				{
					return "Set bag failed.";
				}
			}

			protected override string SuccessMsg
			{
				get
				{
					return "Player bag set.";
				}
			}

			private void OnPlayerTaken(object state, Player obj)
			{
				if (obj == null)
				{
					base.OnError(state, new NullReferenceException("Server returned null player."));
					return;
				}
				if (GameHubObject.Hub.Match.Kind == 2 || GameHubObject.Hub.Match.Kind == 6)
				{
					AsyncOperation.Log.DebugFormat("OnPlayerTaken from SetServerOperation: PlayerId: {0}, PlayerBag not changed since it is tutorial or training", new object[]
					{
						obj.Id
					});
					return;
				}
				PlayerBag playerBag = (PlayerBag)obj.Bag;
				playerBag.CurrentServerIp = this.Ip;
				playerBag.CurrentPort = this.Port;
				playerBag.CurrentMatchId = this.MatchId;
				playerBag.CurrentGroupId = this.GroupId;
				playerBag.CurrentIsNarrator = this.IsNarrator;
				AsyncOperation.Log.DebugFormat("OnPlayerTaken from SetServerOperation: PlayerId: {0}, PlayerBag: {1}", new object[]
				{
					obj.Id,
					playerBag
				});
				ServerPlayer.SaveBag(this.PlayerId, playerBag, obj.BagVersion, new SwordfishClientApi.ParameterizedCallback<long>(base.OnSuccess), new SwordfishClientApi.ErrorCallback(this.ConcurrentBagUpdateOnPlayerTaken));
			}

			private void ConcurrentBagUpdateOnPlayerTaken(object state, Exception exception)
			{
				if (exception is BagVersionException)
				{
					long playerId = (long)state;
					ServerPlayer.GetPlayer(playerId, new SwordfishClientApi.ParameterizedCallback<Player>(this.OnPlayerTaken), new SwordfishClientApi.ErrorCallback(base.OnError));
					return;
				}
				base.OnError(state, exception);
			}

			private long PlayerId;

			private int Port;

			private string Ip;

			private string MatchId;

			private string GroupId;

			private bool IsNarrator;
		}
	}
}
