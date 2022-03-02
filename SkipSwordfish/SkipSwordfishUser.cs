using System;
using ClientAPI;
using ClientAPI.Enum;
using ClientAPI.Objects;
using ClientAPI.Objects.Interfaces;
using ClientAPI.Service.Interfaces;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishUser : IUser
	{
		public void FacebookLogin(object state, string facebookTokenId, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string FacebookLoginSync(string facebookTokenId)
		{
			throw new NotImplementedException();
		}

		public IXboxLiveUser GetXboxUserLogged()
		{
			throw new NotImplementedException();
		}

		public void GuestUserLogin(object state, string token, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string GuestUserLoginSync(string token)
		{
			throw new NotImplementedException();
		}

		public void PublisherLogin(object state, string username, string password, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string PublisherLoginSync(string username, string password)
		{
			throw new NotImplementedException();
		}

		public void ServerLogin(object state, string username, string password, SwordfishClientApi.ParameterizedCallback<ServerLoginInfo> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		ServerLoginInfo IUser.ServerLoginSync(string username, string password)
		{
			throw new NotImplementedException();
		}

		public void ServerLogin(object state, string username, string password, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void GetPsnOnlineId(object state, string universalId, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			callback.Invoke(null, "SkipSF OnlineId");
		}

		public string GetPsnOnlineIdSync(string universalId)
		{
			return universalId;
		}

		public void GetXboxLiveOnlineId(object state, string universalId, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			callback.Invoke(null, "SkipSF OnlineId");
		}

		public string GetXboxLiveOnlineIdSync(string universalId)
		{
			return universalId;
		}

		public void HavePrivilege(object state, int privilege, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			callback.Invoke(state, true);
		}

		public bool HavePrivilegeSync(int privilege)
		{
			return true;
		}

		public void ActivatePublisherUser(object state, string publisherUserId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void ActivatePublisherUserSync(string publisherUserId)
		{
			throw new NotImplementedException();
		}

		public void BanUser(object state, Guid userId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void BanUserSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void BanUserByUniversalId(object state, string universalId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void BanUserByUniversalIdSync(string universalId)
		{
			throw new NotImplementedException();
		}

		public void CreateGuest(object state, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string CreateGuestSync()
		{
			throw new NotImplementedException();
		}

		public void CreatePublisherUser(object state, string login, string password, string email, string firstname, string lastname, string nickname, DateTime birthDate, int gender, bool receiveNews, string friendReferralEmail, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string CreatePublisherUserSync(string login, string password, string email, string firstname, string lastname, string nickname, DateTime birthDate, int gender, bool receiveNews, string friendReferralEmail)
		{
			throw new NotImplementedException();
		}

		public void CreateUser(object state, User user, SwordfishClientApi.ParameterizedCallback<User> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User CreateUserSync(User user)
		{
			throw new NotImplementedException();
		}

		public void DeleteUser(object state, Guid userId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void DeleteUserSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void GetAllUsers(object state, SwordfishClientApi.ParameterizedCallback<User[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User[] GetAllUsersSync()
		{
			throw new NotImplementedException();
		}

		public void GetAllUsersPaged(object state, int page, int recordset, UserEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<User[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User[] GetAllUsersPagedSync(int page, int recordset, UserEnum orderfield, bool sortorder, out int pagecount)
		{
			throw new NotImplementedException();
		}

		public void GetClusterAuthorizedUsers(object state, SwordfishClientApi.ParameterizedCallback<User[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User[] GetClusterAuthorizedUsersSync()
		{
			throw new NotImplementedException();
		}

		public void GetClusterAuthorizedUsersPaged(object state, int page, int recordset, UserEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<User[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User[] GetClusterAuthorizedUsersPagedSync(int page, int recordset, UserEnum orderfield, bool sortorder, out int pagecount)
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

		public void GetMyPublisher(object state, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string GetMyPublisherSync()
		{
			throw new NotImplementedException();
		}

		public void GetMySessionId(object state, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string GetMySessionIdSync()
		{
			throw new NotImplementedException();
		}

		public void GetMyUniversalId(object state, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string GetMyUniversalIdSync()
		{
			throw new NotImplementedException();
		}

		public void GetMyUser(object state, SwordfishClientApi.ParameterizedCallback<User> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User GetMyUserSync()
		{
			throw new NotImplementedException();
		}

		public void GetMyUserId(object state, SwordfishClientApi.ParameterizedCallback<Guid> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public Guid GetMyUserIdSync()
		{
			throw new NotImplementedException();
		}

		public void GetPlayer(object state, long playerId, SwordfishClientApi.ParameterizedCallback<Player> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public Player GetPlayerSync(long playerId)
		{
			throw new NotImplementedException();
		}

		public void GetPlayerByUserId(object state, Guid userId, SwordfishClientApi.ParameterizedCallback<Player> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public Player GetPlayerByUserIdSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void GetPublisherByUniversalId(object state, string universalId, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string GetPublisherByUniversalIdSync(string universalId)
		{
			throw new NotImplementedException();
		}

		public void GetUniversalIdByPublisherUserId(object state, string publisherUserId, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string GetUniversalIdByPublisherUserIdSync(string publisherUserId)
		{
			throw new NotImplementedException();
		}

		public void GetUniversalIdBySwordfishUserId(object state, Guid userId, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string GetUniversalIdBySwordfishUserIdSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void GetUser(object state, Guid userId, SwordfishClientApi.ParameterizedCallback<User> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User GetUserSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void GetUserByCharacterId(object state, long characterId, SwordfishClientApi.ParameterizedCallback<User> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User GetUserByCharacterIdSync(long characterId)
		{
			throw new NotImplementedException();
		}

		public void GetUserByCharacterName(object state, string characterName, SwordfishClientApi.ParameterizedCallback<User> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User GetUserByCharacterNameSync(string characterName)
		{
			throw new NotImplementedException();
		}

		public void GetUserByLogin(object state, string login, SwordfishClientApi.ParameterizedCallback<User> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User GetUserByLoginSync(string login)
		{
			throw new NotImplementedException();
		}

		public void GetUserByPlayerId(object state, long playerId, SwordfishClientApi.ParameterizedCallback<User> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User GetUserByPlayerIdSync(long playerId)
		{
			throw new NotImplementedException();
		}

		public void GetUserByPlayerName(object state, string playerName, SwordfishClientApi.ParameterizedCallback<User> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User GetUserByPlayerNameSync(string playerName)
		{
			throw new NotImplementedException();
		}

		public void GetUserByUniversalID(object state, string universalId, SwordfishClientApi.ParameterizedCallback<User> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public User GetUserByUniversalIDSync(string universalId)
		{
			throw new NotImplementedException();
		}

		public void GetUsersByUniversalID(object state, string[] universalIds, SwordfishClientApi.ParameterizedCallback<UserInfo[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public UserInfo[] GetUsersByUniversalIDSync(string[] universalIds)
		{
			throw new NotImplementedException();
		}

		public void GetUserSteamIdByPublisherUserId(object state, string publisherUserId, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string GetUserSteamIdByPublisherUserIdSync(string publisherUserId)
		{
			throw new NotImplementedException();
		}

		public void GiveClusterAuthorization(object state, Guid userId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void GiveClusterAuthorizationSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void IsBanned(object state, Guid userId, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public bool IsBannedSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void IsBannedByUniversalId(object state, string universalId, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public bool IsBannedByUniversalIdSync(string universalId)
		{
			throw new NotImplementedException();
		}

		public void IsClusterAuthorized(object state, Guid userId, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public bool IsClusterAuthorizedSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void IsCurrentUserGameServer(object state, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public bool IsCurrentUserGameServerSync()
		{
			throw new NotImplementedException();
		}

		public void IsOwnerOfToken(object state, TokenOwner tokenOwner, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public bool IsOwnerOfTokenSync(TokenOwner tokenOwner)
		{
			throw new NotImplementedException();
		}

		public void IsSuspended(object state, Guid userId, SwordfishClientApi.ParameterizedCallback<SuspendedUser> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public SuspendedUser IsSuspendedSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void Logout(object state, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void LogoutSync()
		{
			throw new NotImplementedException();
		}

		public void Logout(object state, string swordfishUserId, string swordfishSessionId, string action, string message, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void LogoutSync(string swordfishUserId, string swordfishSessionId, string action, string message)
		{
			throw new NotImplementedException();
		}

		public void RefreshSession(object state, SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public string RefreshSessionSync()
		{
			throw new NotImplementedException();
		}

		public void RevokeClusterAuthorization(object state, Guid userId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void RevokeClusterAuthorizationSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void SuspendUser(object state, Guid userId, DateTime suspensionEndDate, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void SuspendUserSync(Guid userId, DateTime suspensionEndDate)
		{
			throw new NotImplementedException();
		}

		public void UnbanUser(object state, Guid userId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void UnbanUserSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void UnbanUserByUniversalId(object state, string universalId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void UnbanUserByUniversalIdSync(string universalId)
		{
			throw new NotImplementedException();
		}

		public void UnsuspendUser(object state, Guid userId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void UnsuspendUserSync(Guid userId)
		{
			throw new NotImplementedException();
		}

		public void UpdatePublisherUser(object state, string publisherUserId, string password, string email, string firstname, string lastname, string nickname, DateTime birthDate, int gender, int receiveNews, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void UpdatePublisherUserSync(string publisherUserId, string password, string email, string firstname, string lastname, string nickname, DateTime birthDate, int gender, int receiveNews)
		{
			throw new NotImplementedException();
		}

		public LoginType LoginFrom { get; set; }
	}
}
