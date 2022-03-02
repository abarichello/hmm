using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Objects;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Crossplay.DataTransferObjects;
using Hoplon.Assertions;
using Hoplon.Logging;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class SendGroupInvite : ISendGroupInvite
	{
		public SendGroupInvite(IGroupManager groupManager, IGetLocalPlayerCrossplayData getLocalPlayerCrossplayData, ILogger<ISendGroupInvite> logger)
		{
			Assert.ConstructorParametersAreNotNull(new object[]
			{
				groupManager,
				getLocalPlayerCrossplayData,
				logger
			});
			this._groupManager = groupManager;
			this._getLocalPlayerCrossplayData = getLocalPlayerCrossplayData;
			this._logger = logger;
		}

		public void Send(IPlayer player)
		{
			SerializableCrossplay serializableCrossplay = CrossplayDataSerializableConvertions.ToSerializable(this._getLocalPlayerCrossplayData.Get());
			this._logger.DebugFormat("Sending group invite, myinfo: {0}", new object[]
			{
				serializableCrossplay
			});
			this._groupManager.TryInviteToGroup(this.CreateUserFriendFromPlayer(player), serializableCrossplay.Serialize());
		}

		private UserFriend CreateUserFriendFromPlayer(IPlayer player)
		{
			this._logger.DebugFormat("group invite: {0} - {1} - {2}", new object[]
			{
				player.PlayerId,
				player.Nickname,
				player.UniversalId
			});
			return new UserFriend
			{
				PlayerId = player.PlayerId,
				PlayerName = player.Nickname,
				UniversalId = player.UniversalId,
				UniversalID = player.UniversalId,
				PlayerNameTag = player.PlayerTag
			};
		}

		private readonly IGroupManager _groupManager;

		private readonly IGetLocalPlayerCrossplayData _getLocalPlayerCrossplayData;

		private readonly ILogger<ISendGroupInvite> _logger;
	}
}
