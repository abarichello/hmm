using System;
using ClientAPI.Chat;
using ClientAPI.Objects;
using ClientAPI.Objects.Partial;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Players.Business
{
	public static class PlayerExtensions
	{
		public static IPlayer ConvertToPlayer(this LobbyMember lobbyMember)
		{
			return new Player
			{
				Id = lobbyMember.Id,
				Nickname = lobbyMember.PlayerName,
				PlayerId = lobbyMember.PlayerId,
				PlayerTag = lobbyMember.PlayerNameTag,
				UniversalId = lobbyMember.UniversalID
			};
		}

		public static IPlayer ConvertToPlayer(this PlayerData playerData)
		{
			return new Player
			{
				Nickname = playerData.Name,
				PlayerId = playerData.PlayerId,
				PlayerTag = new long?(playerData.PlayerTag),
				UniversalId = playerData.UserId
			};
		}

		public static IPlayer ConvertToPlayer(this GroupMember groupMember)
		{
			return new Player
			{
				Nickname = groupMember.PlayerName,
				PlayerId = groupMember.PlayerId,
				UniversalId = groupMember.UniversalID
			};
		}

		public static IPlayer GetSenderPlayer(this ChatMessage chatMessage)
		{
			return new Player
			{
				Nickname = chatMessage.PlayerName,
				PlayerId = chatMessage.SenderPlayerId,
				UniversalId = chatMessage.FromUniversalId
			};
		}
	}
}
