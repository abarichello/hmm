using System;
using ClientAPI;
using ClientAPI.Enum;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishPlayer : IPlayer
	{
		public ImageSize[] GetSupportedAvatarSize()
		{
			return null;
		}

		public void GetAvatarAsync(object state, string universalId, int width, int height, SwordfishClientApi.ParameterizedCallback<SwordfishImage> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public void GetAvatar(object state, string universalId, int width, int height, SwordfishClientApi.ParameterizedCallback<SwordfishImage> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public SwordfishImage GetAvatar(string universalId, int width, int height)
		{
			return null;
		}

		public SwordfishImage GetAvatarSync(string universalId, int width, int height)
		{
			return null;
		}

		public void GetSteamProfileImageAsync(object state, SwordfishClientApi.ParameterizedCallback<byte[]> callback, SwordfishClientApi.ErrorCallback errorCallback, string personaName = "", ulong steamID = 0UL)
		{
		}

		public void GetSteamProfileImage(object state, SwordfishClientApi.ParameterizedCallback<byte[]> callback, SwordfishClientApi.ErrorCallback errorCallback, string personaName = "", ulong steamID = 0UL)
		{
		}

		public byte[] GetSteamProfileImage(out int bpp, out int width, out int height, string personaName = "", ulong steamID = 0UL)
		{
			throw new NotImplementedException();
		}

		public byte[] GetSteamProfileImageSync(out int bpp, out int width, out int height, string personaName = "", ulong steamID = 0UL)
		{
			throw new NotImplementedException();
		}

		public void BlockPlayer(object state, long blockPlayerId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public void BlockPlayerSync(long blockPlayerId)
		{
		}

		public void UnblockPlayer(object state, long blockPlayerId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public void UnblockPlayerSync(long blockPlayerId)
		{
		}

		public void UpdatedCurrentPlayerBlockListWithPublisherBlockListSync()
		{
		}

		public void UpdatedCurrentPlayerBlockListWithPublisherBlockList(object state, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			callback.Invoke(state);
		}

		public void CreateMyPlayer(object state, Player player, SwordfishClientApi.ParameterizedCallback<Player> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player CreateMyPlayerSync(Player player)
		{
			return null;
		}

		public void CreatePlayer(object state, Player player, Guid ownerId, SwordfishClientApi.ParameterizedCallback<Player> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player CreatePlayerSync(Player player, Guid ownerId)
		{
			return null;
		}

		public void GetFacebookFriends(object state, SwordfishClientApi.ParameterizedCallback<Player[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player[] GetFacebookFriendsSync()
		{
			return null;
		}

		public void GetFacebookFriendsPaged(object state, int page, int recordset, FacebookFriendshipEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<Player[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player[] GetFacebookFriendsPagedSync(int page, int recordset, FacebookFriendshipEnum orderfield, bool sortorder, out int pagecount)
		{
			throw new NotImplementedException();
		}

		public void UpdatePlayer(object state, Player player, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public void UpdatePlayerSync(Player player)
		{
		}

		public void UpdatePlayerBlockListWithPublisherBlockers(object state, long playerId, string[] publisherBlockersUniversalId, SwordfishClientApi.ParameterizedCallback<UpdatePlayerBlockListWithPublisherBlockersResult> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		UpdatePlayerBlockListWithPublisherBlockersResult IPlayer.UpdatePlayerBlockListWithPublisherBlockersSync(long playerId, string[] publisherBlockersUniversalId)
		{
			throw new NotImplementedException();
		}

		public void UpdatePlayerBlockListWithPublisherBlockersSync(long playerId, string[] publisherBlockersUniversalId)
		{
			throw new NotImplementedException();
		}

		public void UpdatePlayerName(object state, long playerId, string name, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void UpdatePlayerNameSync(long playerId, string name)
		{
			throw new NotImplementedException();
		}

		public void UpdatePlayerBag(object state, BagWrapper bagWrapper, SwordfishClientApi.ParameterizedCallback<long> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public long UpdatePlayerBagSync(BagWrapper bagWrapper)
		{
			return 0L;
		}

		public void UpdatePlayerBlockListWithPublisherBlockers(object state, long playerId, string[] publisherBlockersUniversalId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void GetAllPlayers(object state, SwordfishClientApi.ParameterizedCallback<Player[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player[] GetAllPlayersSync()
		{
			return null;
		}

		public void GetAllPlayersPaged(object state, int page, int recordset, PlayerEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<Player[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player[] GetAllPlayersPagedSync(int page, int recordset, PlayerEnum orderfield, bool sortorder, out int pagecount)
		{
			throw new NotImplementedException();
		}

		public void GetAllPlayersFromUsers(object state, Guid[] userIds, SwordfishClientApi.ParameterizedCallback<Player[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player[] GetAllPlayersFromUsersSync(Guid[] userIds)
		{
			return null;
		}

		public void GetAllPlayersFromUsersPaged(object state, Guid[] userIds, int page, int recordset, PlayerEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<Player[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player[] GetAllPlayersFromUsersPagedSync(Guid[] userIds, int page, int recordset, PlayerEnum orderfield, bool sortorder, out int pagecount)
		{
			throw new NotImplementedException();
		}

		public void GetMyPlayer(object state, SwordfishClientApi.ParameterizedCallback<Player> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public Player GetMyPlayerSync()
		{
			throw new NotImplementedException();
		}

		public void GetPlayersIdsFromUsersIds(object state, Guid[] userIds, SwordfishClientApi.ParameterizedCallback<long[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public long[] GetPlayersIdsFromUsersIdsSync(Guid[] userIds)
		{
			throw new NotImplementedException();
		}

		public void GetPlayersIdsFromUsersIdsPaged(object state, Guid[] userIds, int page, int recordset, PlayerEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<long[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public long[] GetPlayersIdsFromUsersIdsPagedSync(Guid[] userIds, int page, int recordset, PlayerEnum orderfield, bool sortorder, out int pagecount)
		{
			pagecount = 0;
			return null;
		}

		public void ExistPlayerName(object state, string playerName, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public bool ExistPlayerNameSync(string playerName)
		{
			return false;
		}

		public void GetPlayer(object state, long playerId, SwordfishClientApi.ParameterizedCallback<Player> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player GetPlayerSync(long playerId)
		{
			return null;
		}

		public string GetPlayerNameByPlayerIdSync(long playerId)
		{
			throw new NotImplementedException();
		}

		public void GetPlayerNameByUniversalId(object state, string universalId, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public string GetPlayerNameByUniversalIdSync(string universalId)
		{
			return null;
		}

		public void GetPlayerIdByUniversalId(object state, string universalId, SwordfishClientApi.ParameterizedCallback<long> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public long GetPlayerIdByUniversalIdSync(string universalId)
		{
			throw new NotImplementedException();
		}

		public void GetPlayerNameByPlayerId(object state, long playerId, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void GetPlayerStartWithName(object state, string name, SwordfishClientApi.ParameterizedCallback<Player[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player[] GetPlayerStartWithNameSync(string name)
		{
			return null;
		}

		public void GetPlayerStartWithNamePaged(object state, string name, int page, int recordset, PlayerEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<Player[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player[] GetPlayerStartWithNamePagedSync(string name, int page, int recordset, PlayerEnum orderfield, bool sortorder, out int pagecount)
		{
			throw new NotImplementedException();
		}

		public void GetPlayerByName(object state, string name, SwordfishClientApi.ParameterizedCallback<Player> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public Player GetPlayerByNameSync(string name)
		{
			throw new NotImplementedException();
		}

		public void GetPlayerByNamePaged(object state, string name, int page, int recordset, PlayerEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<Player> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public Player GetPlayerByNamePagedSync(string name, int page, int recordset, PlayerEnum orderfield, bool sortorder, out int pagecount)
		{
			throw new NotImplementedException();
		}

		public void GetPlayerByNameTag(object state, long nameTag, SwordfishClientApi.ParameterizedCallback<Player> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player GetPlayerByNameTagSync(long nameTag)
		{
			return null;
		}

		public void GetPlayersByPlayersIds(object state, long[] playerIds, SwordfishClientApi.ParameterizedCallback<Player[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player[] GetPlayersByPlayersIdsSync(long[] playerIds)
		{
			return null;
		}

		public void GetBlockedPlayers(object state, SwordfishClientApi.ParameterizedCallback<BlockedPlayer[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			BlockedPlayer[] array = new BlockedPlayer[0];
			callback.Invoke(array, array);
		}

		public BlockedPlayer[] GetBlockedPlayersSync()
		{
			return null;
		}

		public void GetPlayersWhoBlockedCurrentPlayer(object state, SwordfishClientApi.ParameterizedCallback<BlockedPlayer[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			BlockedPlayer[] array = new BlockedPlayer[0];
			callback.Invoke(array, array);
		}

		public BlockedPlayer[] GetPlayersWhoBlockedCurrentPlayerSync()
		{
			return null;
		}

		public void GetPlayerByUniversalId(object state, string universalId, SwordfishClientApi.ParameterizedCallback<Player> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public Player GetPlayerByUniversalIdSync(string universalId)
		{
			return null;
		}

		public void IsPlayerSuspendedFromQueue(object state, string universalId, string queueName, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public bool IsPlayerSuspendedFromQueueSync(string universalId, string queueName)
		{
			return false;
		}

		public void IsPlayerSuspendedFromQueue(object state, long playerId, string queueName, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public bool IsPlayerSuspendedFromQueueSync(long playerId, string queueName)
		{
			return false;
		}

		public void IsPlayerSuspendedFromQueue(object state, string queueName, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
		}

		public bool IsPlayerSuspendedFromQueueSync(string queueName)
		{
			return false;
		}
	}
}
