using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using Assets.Standard_Assets.Scripts.HMM.Swordfish.Services;
using ClientAPI.Objects;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Crossplay.DataTransferObjects;
using HeavyMetalMachines.Social.Groups.Models;
using Hoplon.Logging;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class GroupCrossplayStorage : IGroupCrossplayStorage
	{
		public GroupCrossplayStorage(IGroupManager groupManager, IGroupStorage groupStorage, IGetCrossplayDataByPlayerId getCrossplayDataByPlayerId, ILogger<GroupCrossplayStorage> logger, IRemoveGroupMemberFromGroup removeGroupMember, IGetLocalPlayerCrossplayData getLocalPlayerCrossplayData)
		{
			this._groupManager = groupManager;
			this._groupStorage = groupStorage;
			this._getCrossplayDataByPlayerId = getCrossplayDataByPlayerId;
			this._logger = logger;
			this._removeGroupMember = removeGroupMember;
			this._getLocalPlayerCrossplayData = getLocalPlayerCrossplayData;
			this._groupMemberCrossplayDatas = new List<PlayerCrossplayData>();
		}

		public void Initialize()
		{
			if (this._isInitialized)
			{
				return;
			}
			this._logger.Debug("Initialize");
			ObservableExtensions.Subscribe<Group>(Observable.Do<Group>(this._groupStorage.OnGroupChanged, new Action<Group>(this.OnGroupUpdate)));
			ObservableExtensions.Subscribe<GroupEventArgs>(Observable.Do<GroupEventArgs>(Observable.Merge<GroupEventArgs>(this.ListenToInviteAccept(), new IObservable<GroupEventArgs>[]
			{
				this.ListenToInviteRecived()
			}), new Action<GroupEventArgs>(this.HandleInviteAcceptOrReceived)));
			this._isInitialized = true;
		}

		private void HandleInviteAcceptOrReceived(GroupEventArgs newMember)
		{
			if (string.IsNullOrEmpty(newMember.Bag))
			{
				this._logger.DebugFormat("Wrong bag format in invite accept. PlayerId: {0}, bag: {1}", new object[]
				{
					newMember.Member.PlayerId,
					newMember.Bag
				});
				return;
			}
			SerializableCrossplay serializableCrossplay = JsonSerializeable<SerializableCrossplay>.Deserialize(newMember.Bag);
			PlayerCrossplayData newMemberCrossplayData = CrossplayDataSerializableConvertions.ToModel(serializableCrossplay);
			if (!this.IsPLayerValidToJoinGroup(newMemberCrossplayData))
			{
				GroupMember member = newMember.Member;
				GroupMember groupMember = new GroupMember
				{
					PlayerId = member.PlayerId,
					UniversalId = member.UniversalID,
					IsPendingInvite = member.IsPendingInviteToGroup(),
					Nickname = member.PlayerName,
					IsGroupLeader = member.IsUserGroupLeader(),
					PlayerTag = member.NameTag
				};
				this._removeGroupMember.Remove(groupMember);
				return;
			}
			this.AddNewMember(newMemberCrossplayData);
		}

		private void AddNewMember(PlayerCrossplayData newMemberCrossplayData)
		{
			if (this._groupMemberCrossplayDatas.Any((PlayerCrossplayData member) => member.PlayerId == newMemberCrossplayData.PlayerId))
			{
				return;
			}
			this._logger.DebugFormat("Add new member playerId : {0}, CrossplayEnable : {1}, Publisher : {2}", new object[]
			{
				newMemberCrossplayData.PlayerId,
				newMemberCrossplayData.IsEnabled,
				newMemberCrossplayData.Publisher
			});
			this._groupMemberCrossplayDatas.Add(newMemberCrossplayData);
			this._logger.DebugFormat("Group Crossplay Members count: {0}", new object[]
			{
				this._groupMemberCrossplayDatas.Count
			});
		}

		private bool IsPLayerValidToJoinGroup(PlayerCrossplayData newMemberCrossplayData)
		{
			this._logger.DebugFormat("Validating player to joying group : {0}, CrossplayEnable : {1}, Publisher : {2}", new object[]
			{
				newMemberCrossplayData.PlayerId,
				newMemberCrossplayData.IsEnabled,
				newMemberCrossplayData.Publisher
			});
			Group group = this._groupStorage.Group;
			if (group == null)
			{
				this._logger.Debug("Group is null, checking with player");
				return this.ValidatePlayer(newMemberCrossplayData, this._getLocalPlayerCrossplayData.Get());
			}
			GroupMember groupLeader = group.Leader;
			if (groupLeader == null)
			{
				this._logger.Debug("Group Leader is null, checking with player");
				return this.ValidatePlayer(newMemberCrossplayData, this._getLocalPlayerCrossplayData.Get());
			}
			PlayerCrossplayData playerCrossplayData = this._groupMemberCrossplayDatas.FirstOrDefault((PlayerCrossplayData member) => member.PlayerId == groupLeader.PlayerId);
			if (playerCrossplayData == null)
			{
				this._logger.Debug("Group Leader not found in CrossplayData, checking with player");
				return this.ValidatePlayer(newMemberCrossplayData, this._getLocalPlayerCrossplayData.Get());
			}
			this._logger.DebugFormat("Validating with group leader: {0}, CrossplayEnable : {1}, Publisher : {2}", new object[]
			{
				playerCrossplayData.PlayerId,
				playerCrossplayData.IsEnabled,
				playerCrossplayData.Publisher
			});
			return this.ValidatePlayer(newMemberCrossplayData, playerCrossplayData);
		}

		private bool ValidatePlayer(PlayerCrossplayData newMemberCrossplayData, PlayerCrossplayData leaderCrossplayData)
		{
			if (leaderCrossplayData.IsEnabled)
			{
				return newMemberCrossplayData.IsEnabled || newMemberCrossplayData.Publisher.Equals(leaderCrossplayData.Publisher);
			}
			return newMemberCrossplayData.Publisher.Equals(leaderCrossplayData.Publisher);
		}

		private IObservable<GroupEventArgs> ListenToInviteAccept()
		{
			return Observable.FromEvent<GroupEventArgs>(delegate(Action<GroupEventArgs> handler)
			{
				this._groupManager.EvtGroupInviteAccepted += handler;
			}, delegate(Action<GroupEventArgs> handler)
			{
				this._groupManager.EvtGroupInviteAccepted -= handler;
			});
		}

		private IObservable<GroupEventArgs> ListenToInviteRecived()
		{
			return Observable.FromEvent<GroupEventArgs>(delegate(Action<GroupEventArgs> handler)
			{
				this._groupManager.EvtGroupInviteReceived += handler;
			}, delegate(Action<GroupEventArgs> handler)
			{
				this._groupManager.EvtGroupInviteReceived -= handler;
			});
		}

		public List<PlayerCrossplayData> GetMembers()
		{
			return this._groupMemberCrossplayDatas;
		}

		private void OnGroupUpdate(Group group)
		{
			if (group == null)
			{
				this.ResetGroup();
				return;
			}
			this._logger.DebugFormat("Group count: {0}", new object[]
			{
				group.Members.Count
			});
			if (this.CheckAndRemoveMembers(group))
			{
				return;
			}
			ObservableExtensions.Subscribe<Unit>(this.AddGroupMember(group));
		}

		private IObservable<Unit> AddGroupMember(Group group)
		{
			return Observable.Concat<Unit>(Observable.Select<GroupMember, IObservable<Unit>>(Observable.Do<GroupMember>(Observable.Where<GroupMember>(Observable.ToObservable<GroupMember>(group.Members), (GroupMember groupMember) => this._groupMemberCrossplayDatas.All((PlayerCrossplayData member) => member.PlayerId != groupMember.PlayerId)), delegate(GroupMember groupMember)
			{
				this._logger.DebugFormat("Try Add player: {0}, playerId: {1}", new object[]
				{
					groupMember.Nickname,
					groupMember.PlayerId
				});
			}), (GroupMember groupMember) => this.GetCrossplayInfoAndAddNewMember(groupMember.PlayerId)));
		}

		private IObservable<Unit> GetCrossplayInfoAndAddNewMember(long newMemberPlayerId)
		{
			return Observable.AsUnitObservable<PlayerCrossplayData>(Observable.Do<PlayerCrossplayData>(this._getCrossplayDataByPlayerId.Get(newMemberPlayerId), new Action<PlayerCrossplayData>(this.HandleGroupUpdate)));
		}

		private void HandleGroupUpdate(PlayerCrossplayData newMemberCrossplayData)
		{
			if (!this.IsPLayerValidToJoinGroup(newMemberCrossplayData))
			{
				GroupMember groupMember = this._groupStorage.Group.Members.FirstOrDefault((GroupMember p) => p.PlayerId == newMemberCrossplayData.PlayerId);
				this._logger.DebugFormat("Removing group Member playerId{0},", new object[]
				{
					newMemberCrossplayData.PlayerId
				});
				this._removeGroupMember.Remove(groupMember);
				return;
			}
			this.AddNewMember(newMemberCrossplayData);
		}

		private bool CheckAndRemoveMembers(Group group)
		{
			this._logger.Debug("Checking and removing");
			if (this._groupMemberCrossplayDatas.Count < group.Members.Count)
			{
				return false;
			}
			List<PlayerCrossplayData> membersToBeRemoved = this.GetMembersToBeRemoved(group);
			if (membersToBeRemoved.Count <= 0)
			{
				this._logger.Debug("No member to remove");
				return false;
			}
			foreach (PlayerCrossplayData playerCrossplayData in membersToBeRemoved)
			{
				this._logger.DebugFormat("Removing player: {0}", new object[]
				{
					playerCrossplayData.PlayerId
				});
				this._groupMemberCrossplayDatas.Remove(playerCrossplayData);
			}
			return true;
		}

		private void ResetGroup()
		{
			this._logger.Debug("Reset group");
			this._groupMemberCrossplayDatas.Clear();
		}

		private List<PlayerCrossplayData> GetMembersToBeRemoved(Group group)
		{
			List<PlayerCrossplayData> list = new List<PlayerCrossplayData>();
			using (List<PlayerCrossplayData>.Enumerator enumerator = this._groupMemberCrossplayDatas.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PlayerCrossplayData groupMember = enumerator.Current;
					if (!group.Members.Any((GroupMember member) => member.PlayerId == groupMember.PlayerId))
					{
						list.Add(groupMember);
					}
				}
			}
			return list;
		}

		private readonly List<PlayerCrossplayData> _groupMemberCrossplayDatas;

		private readonly IGroupManager _groupManager;

		private readonly IGroupStorage _groupStorage;

		private readonly IGetCrossplayDataByPlayerId _getCrossplayDataByPlayerId;

		private readonly ILogger<GroupCrossplayStorage> _logger;

		private readonly IRemoveGroupMemberFromGroup _removeGroupMember;

		private readonly IGetLocalPlayerCrossplayData _getLocalPlayerCrossplayData;

		private bool _isInitialized;
	}
}
