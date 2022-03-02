using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Objects;
using HeavyMetalMachines.Social.Groups.Models;
using ModestTree;
using UniRx;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class GroupStorage : IGroupStorage
	{
		public GroupStorage(GroupManager groupManager)
		{
			Assert.IsNotNull(groupManager, "Cannot create GroupStorage with null groupManager.");
			this._groupManager = groupManager;
			this._groupObservation = new Subject<Group>();
			ObservableExtensions.Subscribe<Group>(Observable.Select<Unit, Group>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this.ListenToGroupUpdate(),
				this.ListenToGroupQuit(),
				this.ListenToGroupRemoved(),
				this.ListenToGroupLeave()
			}), (Unit _) => GroupStorage.ConvertFromSwordfishGroup(this._groupManager.GetCurrentGroupIfExists())), delegate(Group group)
			{
				this._groupObservation.OnNext(group);
			});
		}

		public Group Group
		{
			get
			{
				Group currentGroupIfExists = this._groupManager.GetCurrentGroupIfExists();
				return GroupStorage.ConvertFromSwordfishGroup(currentGroupIfExists);
			}
			set
			{
				throw new NotImplementedException("A group cannot be set on GroupStorage because the GroupManager is still the class resposible for storing this information.");
			}
		}

		public IObservable<Group> OnGroupChanged
		{
			get
			{
				return this._groupObservation;
			}
		}

		private static Group ConvertFromSwordfishGroup(Group swordfishGroup)
		{
			if (swordfishGroup == null)
			{
				return null;
			}
			Group group = new Group
			{
				Guid = swordfishGroup.Id
			};
			group.Members = new List<GroupMember>(swordfishGroup.Members.Count);
			for (int i = 0; i < swordfishGroup.Members.Count; i++)
			{
				GroupMember groupMember = swordfishGroup.Members[i];
				GroupMember groupMember2 = new GroupMember
				{
					PlayerId = groupMember.PlayerId,
					UniversalId = groupMember.UniversalID,
					IsPendingInvite = groupMember.IsPendingInviteToGroup(),
					Nickname = groupMember.PlayerName,
					IsGroupLeader = groupMember.IsUserGroupLeader(),
					PlayerTag = groupMember.NameTag
				};
				group.Members.Add(groupMember2);
				if (groupMember.IsUserGroupLeader())
				{
					group.Leader = groupMember2;
				}
			}
			return group;
		}

		private IObservable<Unit> ListenToGroupUpdate()
		{
			return Observable.FromEvent(delegate(Action handler)
			{
				this._groupManager.OnGroupUpdate += handler;
			}, delegate(Action handler)
			{
				this._groupManager.OnGroupUpdate -= handler;
			});
		}

		private IObservable<Unit> ListenToGroupQuit()
		{
			return Observable.AsUnitObservable<string>(Observable.FromEvent<string>(delegate(Action<string> handler)
			{
				this._groupManager.EvtGroupQuit += handler;
			}, delegate(Action<string> handler)
			{
				this._groupManager.EvtGroupQuit -= handler;
			}));
		}

		private IObservable<Unit> ListenToGroupRemoved()
		{
			return Observable.AsUnitObservable<Guid>(Observable.FromEvent<Guid>(delegate(Action<Guid> handler)
			{
				this._groupManager.EvtCurrentUserRemovedFromGroup += handler;
			}, delegate(Action<Guid> handler)
			{
				this._groupManager.EvtCurrentUserRemovedFromGroup -= handler;
			}));
		}

		private IObservable<Unit> ListenToGroupLeave()
		{
			return Observable.AsUnitObservable<Guid>(Observable.FromEvent<Guid>(delegate(Action<Guid> handler)
			{
				this._groupManager.EvtCurrentUserLeaveGroup += handler;
			}, delegate(Action<Guid> handler)
			{
				this._groupManager.EvtCurrentUserLeaveGroup -= handler;
			}));
		}

		private readonly GroupManager _groupManager;

		private Subject<Group> _groupObservation;
	}
}
