using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX.PlotKids
{
	public class FriendHolder
	{
		public FriendHolder(UserFriend userFriend)
		{
			this.RefreshUserFriend(userFriend);
		}

		public bool IsFriendBagLoaded
		{
			get
			{
				return this._friendBagLoaded;
			}
		}

		public UserFriend UserFriend
		{
			get
			{
				return this._userFriend;
			}
		}

		public FriendBag FriendBag
		{
			get
			{
				if (this._friendBag == null || !this._friendBagLoaded)
				{
					this.TryRefreshFriendBag();
				}
				return this._friendBag;
			}
		}

		public void TryRefreshFriendBag()
		{
			this._friendBagLoaded = false;
			if (string.IsNullOrEmpty(this._userFriend.Bag))
			{
				if (this._friendBag == null)
				{
					this._friendBag = new FriendBag();
				}
				return;
			}
			try
			{
				this._friendBag = (FriendBag)((JsonSerializeable<T>)this._userFriend.Bag);
				this._friendBagLoaded = true;
			}
			catch (Exception ex)
			{
				FriendHolder.Log.ErrorFormat("Exception while updating FriendBag from user {0}.\nException: {1}", new object[]
				{
					this._userFriend,
					ex.Message
				});
			}
		}

		public void RefreshUserFriend(UserFriend userFriend)
		{
			if (this._userFriend != null && !string.Equals(this._userFriend.UniversalID, userFriend.UniversalID))
			{
				Debug.LogWarningFormat("[RefreshUserFriend] Trying to replace User \"{0}\" with User \"{1}\"", new object[]
				{
					this._userFriend.UniversalID,
					userFriend.UniversalID
				});
			}
			this._userFriend = userFriend;
			this.TryRefreshFriendBag();
		}

		public static BitLogger Log = new BitLogger(typeof(FriendHolder));

		private UserFriend _userFriend;

		private FriendBag _friendBag;

		private bool _friendBagLoaded;
	}
}
