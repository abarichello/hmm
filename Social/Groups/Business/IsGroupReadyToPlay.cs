using System;
using System.Linq;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Social.Friends.Business;
using HeavyMetalMachines.Social.Friends.Models;
using HeavyMetalMachines.Social.Groups.Models;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class IsGroupReadyToPlay : IIsGroupReadyToPlay
	{
		public IsGroupReadyToPlay(IIsGroupMemberReadyToPlay isGroupMemberReadyToPlay, IGroupStorage groupStorage, IObservePlayerChanges<Friend> observeFriendChanges, ILogger<IsGroupReadyToPlay> logger)
		{
			this._isGroupMemberReadyToPlay = isGroupMemberReadyToPlay;
			this._groupStorage = groupStorage;
			this._observeFriendChanges = observeFriendChanges;
			this._logger = logger;
		}

		public bool CheckIsReady()
		{
			return this.CheckAreAllMembersReady();
		}

		public IObservable<bool> IsReady()
		{
			return this.AreAllMembersReady();
		}

		private IObservable<bool> AreAllMembersReady()
		{
			return Observable.Defer<bool>(() => Observable.Return(this.CheckAreAllMembersReady()));
		}

		private bool CheckAreAllMembersReady()
		{
			Group group = this._groupStorage.Group;
			return this.IsSolo(group) || group.Members.All(new Func<GroupMember, bool>(this.IsGroupMemberReadyToPlay));
		}

		private bool IsSolo(Group group)
		{
			return group == null || group.Members == null || group.Members.Count == 1;
		}

		private bool IsGroupMemberReadyToPlay(GroupMember groupMember)
		{
			return this._isGroupMemberReadyToPlay.IsReady(groupMember.PlayerId);
		}

		public IObservable<bool> Observe()
		{
			return Observable.Merge<bool>(new IObservable<bool>[]
			{
				this.ObserveGroupFriendChanges(),
				this.ObserveGroupChanges()
			});
		}

		private IObservable<bool> ObserveGroupChanges()
		{
			return Observable.SelectMany<Group, bool>(this._groupStorage.OnGroupChanged, (Group _) => this.AreAllMembersReady());
		}

		private IObservable<bool> ObserveGroupFriendChanges()
		{
			return Observable.SelectMany<Friend, bool>(Observable.Where<Friend>(this._observeFriendChanges.Observe(), new Func<Friend, bool>(this.IsFriendGroupMember)), this.IsReady());
		}

		private bool IsFriendGroupMember(Friend friend)
		{
			Group group = this._groupStorage.Group;
			return group != null && group.Members != null && group.Members.Any((GroupMember m) => m.PlayerId == friend.PlayerId);
		}

		private IIsGroupMemberReadyToPlay _isGroupMemberReadyToPlay;

		private IGroupStorage _groupStorage;

		private IObservePlayerChanges<Friend> _observeFriendChanges;

		private readonly ILogger<IsGroupReadyToPlay> _logger;
	}
}
