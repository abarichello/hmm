using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Social.Friends.Business;
using HeavyMetalMachines.Social.Friends.Models;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class IsGroupMemberReadyToPlay : IIsGroupMemberReadyToPlay
	{
		public IsGroupMemberReadyToPlay(IFriendsStorage friendsStorage, IGetLocalPlayer getLocalPlayer)
		{
			this._friendsStorage = friendsStorage;
			this._getLocalPlayer = getLocalPlayer;
		}

		public bool IsReady(long playerId)
		{
			if (playerId == this._getLocalPlayer.Get().PlayerId)
			{
				return true;
			}
			Friend[] friends = this._friendsStorage.Friends;
			IEnumerable<Friend> source = from friend in friends
			where friend.PlayerId == playerId
			select friend;
			if (IsGroupMemberReadyToPlay.<>f__mg$cache0 == null)
			{
				IsGroupMemberReadyToPlay.<>f__mg$cache0 = new Func<Friend, bool>(IsGroupMemberReadyToPlay.IsFriendReady);
			}
			return source.All(IsGroupMemberReadyToPlay.<>f__mg$cache0);
		}

		private static bool IsFriendReady(Friend friend)
		{
			return friend.Status == 1;
		}

		private static BitLogger _logger = new BitLogger(typeof(IsGroupMemberReadyToPlay));

		private readonly IFriendsStorage _friendsStorage;

		private readonly IGetLocalPlayer _getLocalPlayer;

		[CompilerGenerated]
		private static Func<Friend, bool> <>f__mg$cache0;
	}
}
