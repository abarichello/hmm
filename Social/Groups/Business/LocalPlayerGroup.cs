using System;
using HeavyMetalMachines.Social.Groups.Models;
using ModestTree;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class LocalPlayerGroup : ILocalPlayerGroup
	{
		public LocalPlayerGroup(IGroupStorage groupStorage, UserInfo userInfo)
		{
			Assert.IsNotNull(groupStorage, "Cannot create LocalPlayerGroup with null groupStorage.");
			Assert.IsNotNull(userInfo, "Cannot create LocalPlayerGroup with null userInfo.");
			this._groupStorage = groupStorage;
			this._userInfo = userInfo;
		}

		public bool IsPlayerInGroup()
		{
			return this._groupStorage.Group != null;
		}

		public bool IsPlayerGroupOwner()
		{
			Assert.IsNotNull(this._userInfo, "QAHMM-27319: LocalPlayerGroup Cannot have null userInfo.");
			Group group = this._groupStorage.Group;
			return group != null && group.Leader != null && group.Leader.UniversalId == this._userInfo.UniversalId;
		}

		private readonly IGroupStorage _groupStorage;

		private readonly UserInfo _userInfo;
	}
}
