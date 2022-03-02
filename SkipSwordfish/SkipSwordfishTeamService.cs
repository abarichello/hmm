using System;
using ClientAPI;
using ClientAPI.Enum;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using Pocketverse;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishTeamService : ITeam
	{
		public void CreateTeam(object state, string[] members, string leaderUniversalId, string Name, string Tag, string ImageURL, SwordfishClientApi.ParameterizedCallback<Team> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("CreateTeam");
		}

		public Team CreateTeamSync(string[] members, string leaderUniversalId, string Name, string Tag, string ImageURL)
		{
			SkipSwordfishTeamService.Log.Info("CreateTeamSync");
			Guid guid = Guid.NewGuid();
			return new Team
			{
				Bag = string.Empty,
				Id = guid,
				Invites = new TeamInvite[0],
				Name = Name,
				Tag = Tag,
				BagVersion = 0L,
				ImageUrl = ImageURL,
				TeamChanges = new TeamChanges[0],
				TeamMembers = new TeamMember[]
				{
					new TeamMember
					{
						Id = Guid.NewGuid(),
						IsLeader = true,
						JoinDate = DateTime.Now,
						PlayerId = 0L,
						PlayerName = "Mock Player 0",
						TeamId = guid,
						UniversalId = leaderUniversalId
					},
					new TeamMember
					{
						Id = Guid.NewGuid(),
						IsLeader = true,
						JoinDate = DateTime.Now,
						PlayerId = 1L,
						PlayerName = "Mock Player 1",
						TeamId = guid,
						UniversalId = "1"
					},
					new TeamMember
					{
						Id = Guid.NewGuid(),
						IsLeader = true,
						JoinDate = DateTime.Now,
						PlayerId = 2L,
						PlayerName = "Mock Player 2",
						TeamId = guid,
						UniversalId = "2"
					},
					new TeamMember
					{
						Id = Guid.NewGuid(),
						IsLeader = true,
						JoinDate = DateTime.Now,
						PlayerId = 3L,
						PlayerName = "Mock 3",
						TeamId = guid,
						UniversalId = "3"
					}
				}
			};
		}

		public void GetAllTeams(object state, SwordfishClientApi.ParameterizedCallback<Team[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("GetAllTeams");
		}

		public Team[] GetAllTeamsSync()
		{
			SkipSwordfishTeamService.Log.Info("GetAllTeams");
			return new Team[0];
		}

		public void GetAllTeamsPaged(object state, int page, int recordset, TeamEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<Team[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("GetAllTeamsPaged");
		}

		public Team[] GetAllTeamsPagedSync(int page, int recordset, TeamEnum orderfield, bool sortorder, out int pagecount)
		{
			pagecount = 0;
			SkipSwordfishTeamService.Log.Info("GetAllTeamsPagedSync");
			return new Team[]
			{
				this.CreateTeamSync(null, "0", "Team Mock", "TMA", string.Empty)
			};
		}

		public void CheckTagIsUnique(object state, string Tag, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("CheckTagIsUnique");
		}

		public bool CheckTagIsUniqueSync(string Tag)
		{
			SkipSwordfishTeamService.Log.Info("CheckTagIsUniqueSync");
			return true;
		}

		public void SetMMRByTeamId(object state, Guid teamId, long stepId, TeamMMRInfo teamMMRInfo, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("SetMMRByTeamId");
		}

		public void SetMMRByTeamIdSync(Guid teamId, long stepId, TeamMMRInfo teamMMRInfo)
		{
			SkipSwordfishTeamService.Log.Info("SetMMRByTeamIdSync");
		}

		public void SetTeamAsSortable(object state, Guid teamId, long stepId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("SetTeamAsSortable");
		}

		public void SetTeamAsSortableSync(Guid teamId, long stepId)
		{
			SkipSwordfishTeamService.Log.Info("SetTeamAsSortableSync");
		}

		public void GetMMRByStepId(object state, Guid teamId, long stepId, SwordfishClientApi.ParameterizedCallback<TeamMMRInfo> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("GetMMRByStepId");
		}

		public TeamMMRInfo GetMMRByStepIdSync(Guid teamId, long stepId)
		{
			SkipSwordfishTeamService.Log.Info("GetMMRByStepIdSync");
			return new TeamMMRInfo
			{
				MMR = 0.0,
				MMRInfo = string.Empty
			};
		}

		public void GetMMRByUniversalId(object state, string universalId, long stepId, SwordfishClientApi.ParameterizedCallback<TeamMMRInfo> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("GetMMRByUniversalId");
		}

		public TeamMMRInfo GetMMRByUniversalIdSync(string universalId, long stepId)
		{
			SkipSwordfishTeamService.Log.Info("GetMMRByUniversalIdSync");
			return new TeamMMRInfo
			{
				MMR = 0.0,
				MMRInfo = string.Empty
			};
		}

		public void UpdateImageTeam(object state, Guid TeamId, string leaderUniversalId, string ImageURL, SwordfishClientApi.ParameterizedCallback<TeamBusinessResponse> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("UpdateImageTeam");
		}

		public TeamBusinessResponse UpdateImageTeamSync(Guid TeamId, string leaderUniversalId, string ImageURL)
		{
			SkipSwordfishTeamService.Log.Info("UpdateImageTeamSync");
			return new TeamBusinessResponse
			{
				Data = string.Empty,
				Result = false
			};
		}

		public void SendInvite(object state, Guid TeamId, string LeaderUniversalId, string MemberUniversalID, SwordfishClientApi.ParameterizedCallback<TeamBusinessResponse> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("SendInvite");
		}

		public TeamBusinessResponse SendInviteSync(Guid TeamId, string LeaderUniversalId, string MemberUniversalID)
		{
			SkipSwordfishTeamService.Log.Info("SendInviteSync");
			return new TeamBusinessResponse
			{
				Data = string.Empty,
				Result = false
			};
		}

		public void CancelInvite(object state, Guid TeamId, string LeaderUniversalId, string MemberUniversalID, SwordfishClientApi.ParameterizedCallback<TeamBusinessResponse> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("CancelInvite");
		}

		public TeamBusinessResponse CancelInviteSync(Guid TeamId, string LeaderUniversalId, string MemberUniversalID)
		{
			SkipSwordfishTeamService.Log.Info("CancelInviteSync");
			return new TeamBusinessResponse
			{
				Data = string.Empty,
				Result = false
			};
		}

		public void RejectInvite(object state, Guid TeamId, string MemberUniversalID, SwordfishClientApi.ParameterizedCallback<TeamBusinessResponse> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("RejectInvite");
		}

		public TeamBusinessResponse RejectInviteSync(Guid TeamId, string MemberUniversalID)
		{
			SkipSwordfishTeamService.Log.Info("RejectInviteSync");
			return new TeamBusinessResponse
			{
				Data = string.Empty,
				Result = false
			};
		}

		public void AcceptInvite(object state, Guid TeamId, string UniversalID, SwordfishClientApi.ParameterizedCallback<TeamBusinessResponse> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("AcceptInvite");
		}

		public TeamBusinessResponse AcceptInviteSync(Guid TeamId, string UniversalID)
		{
			SkipSwordfishTeamService.Log.Info("AcceptInviteSync");
			return new TeamBusinessResponse
			{
				Data = string.Empty,
				Result = false
			};
		}

		public void GetInvitationsForUniversalId(object state, string UniversalID, SwordfishClientApi.ParameterizedCallback<TeamInviteModelView[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("GetInvitationsForUniversalId");
		}

		public TeamInviteModelView[] GetInvitationsForUniversalIdSync(string UniversalID)
		{
			SkipSwordfishTeamService.Log.Info("GetInvitationsForUniversalId");
			return new TeamInviteModelView[0];
		}

		public void RemoveMember(object state, Guid TeamId, string LeaderUniversalId, string MemberUniversalID, SwordfishClientApi.ParameterizedCallback<TeamBusinessResponse> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("RemoveMember");
		}

		public TeamBusinessResponse RemoveMemberSync(Guid TeamId, string LeaderUniversalId, string MemberUniversalID)
		{
			SkipSwordfishTeamService.Log.Info("RemoveMemberSync");
			return new TeamBusinessResponse
			{
				Data = string.Empty,
				Result = false
			};
		}

		public void GetTeamByUniversalId(object state, string UniversalId, SwordfishClientApi.ParameterizedCallback<Team> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("GetTeamByUniversalId");
		}

		public Team GetTeamByUniversalIdSync(string UniversalId)
		{
			SkipSwordfishTeamService.Log.Info("GetTeamByUniversalIdSync");
			return this.CreateTeamSync(null, "0", "Team Mock", "TMA", string.Empty);
		}

		public void RevokeLeadership(object state, Guid TeamId, string UniversalId, SwordfishClientApi.ParameterizedCallback<TeamBusinessResponse> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("RevokeLeadership");
		}

		public TeamBusinessResponse RevokeLeadershipSync(Guid TeamId, string UniversalId)
		{
			SkipSwordfishTeamService.Log.Info("RevokeLeadershipSync");
			return new TeamBusinessResponse
			{
				Data = string.Empty,
				Result = false
			};
		}

		public void TransferLeadership(object state, Guid TeamId, string leaderUniversalId, string newLeaderUniversalId, SwordfishClientApi.ParameterizedCallback<TeamBusinessResponse> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("TransferLeadership");
		}

		public TeamBusinessResponse TransferLeadershipSync(Guid TeamId, string leaderUniversalId, string newLeaderUniversalId)
		{
			SkipSwordfishTeamService.Log.Info("TransferLeadershipSync");
			return new TeamBusinessResponse
			{
				Data = string.Empty,
				Result = false
			};
		}

		public void GetTeam(object state, Guid TeamId, SwordfishClientApi.ParameterizedCallback<Team> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("GetTeam");
		}

		public Team GetTeamSync(Guid TeamId)
		{
			SkipSwordfishTeamService.Log.Info("GetTeamSync");
			return this.CreateTeamSync(null, "0", "Team Mock", "TMA", string.Empty);
		}

		public void GetTeamByName(object state, string teamName, SwordfishClientApi.ParameterizedCallback<Team> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("GetTeamByName");
		}

		public Team GetTeamByNameSync(string teamName)
		{
			SkipSwordfishTeamService.Log.Info("GetTeamByNameSync");
			return this.CreateTeamSync(null, "0", "Team Mock", "TMA", string.Empty);
		}

		public void GetTeamByTag(object state, string tag, SwordfishClientApi.ParameterizedCallback<Team> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("GetTeamByTag");
		}

		public Team GetTeamByTagSync(string tag)
		{
			SkipSwordfishTeamService.Log.Info("GetTeamByTagSync");
			return this.CreateTeamSync(null, "0", "Team Mock", "TMA", string.Empty);
		}

		public void LeaveTeam(object state, string UniversalId, SwordfishClientApi.ParameterizedCallback<TeamBusinessResponse> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("LeaveTeam");
		}

		public TeamBusinessResponse LeaveTeamSync(string UniversalId)
		{
			SkipSwordfishTeamService.Log.Info("LeaveTeamSync");
			return new TeamBusinessResponse
			{
				Data = string.Empty,
				Result = false
			};
		}

		public void TeamHasChanges(object state, Guid teamId, string universalId, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("TeamHasChangesSync");
		}

		public bool TeamHasChangesSync(Guid teamId, string universalId)
		{
			SkipSwordfishTeamService.Log.Info("TeamHasChangesSync");
			return false;
		}

		public void TeamHasChangesForUser(object state, Guid teamId, string universalId, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("TeamHasChangesForUser");
		}

		public bool TeamHasChangesForUserSync(Guid teamId, string universalId)
		{
			SkipSwordfishTeamService.Log.Info("TeamHasChangesForUserSync");
			return false;
		}

		public void ChangesVisualizedByUser(object state, Guid teamId, string universalId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("ChangesVisualizedByUser");
		}

		public void ChangesVisualizedByUserSync(Guid teamId, string universalId)
		{
			SkipSwordfishTeamService.Log.Info("ChangesVisualizedByUserSync");
		}

		public void GetTeamMembers(object state, Guid teamId, SwordfishClientApi.ParameterizedCallback<TeamMember[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("GetTeamMembers");
		}

		public TeamMember[] GetTeamMembersSync(Guid teamId)
		{
			SkipSwordfishTeamService.Log.Info("GetTeamMembersSync");
			Team team = this.CreateTeamSync(null, "0", "Team Mock", "TMA", string.Empty);
			return team.TeamMembers;
		}

		public void IsLeader(object state, Guid teamId, string UniversalId, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("IsLeader");
		}

		public bool IsLeaderSync(Guid teamId, string UniversalId)
		{
			SkipSwordfishTeamService.Log.Info("IsLeaderSync");
			return false;
		}

		public void GetInvitationsForTeamId(object state, Guid teamId, string leaderUniversalId, SwordfishClientApi.ParameterizedCallback<TeamInvite[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("GetInvitationsForTeamId");
		}

		public TeamInvite[] GetInvitationsForTeamIdSync(Guid teamId, string leaderUniversalId)
		{
			SkipSwordfishTeamService.Log.Info("GetInvitationsForTeamIdSync");
			return new TeamInvite[0];
		}

		public void UpdateTeamTag(object state, Guid teamId, string leaderUniversalId, string tag, SwordfishClientApi.ParameterizedCallback<TeamBusinessResponse> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("UpdateTeamTag");
		}

		public TeamBusinessResponse UpdateTeamTagSync(Guid teamId, string leaderUniversalId, string tag)
		{
			SkipSwordfishTeamService.Log.Info("UpdateTeamTagSync");
			return new TeamBusinessResponse
			{
				Data = string.Empty,
				Result = false
			};
		}

		public void UpdateTeamName(object state, Guid teamId, string leaderUniversalId, string name, SwordfishClientApi.ParameterizedCallback<TeamBusinessResponse> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("UpdateTeamName");
		}

		public TeamBusinessResponse UpdateTeamNameSync(Guid teamId, string leaderUniversalId, string name)
		{
			SkipSwordfishTeamService.Log.Info("UpdateTeamNameSync");
			return new TeamBusinessResponse
			{
				Data = string.Empty,
				Result = false
			};
		}

		public void UpdateTeamBag(object state, GuidBagWrapper bagWrapper, SwordfishClientApi.ParameterizedCallback<long> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishTeamService.Log.Info("UpdateTeamBag");
		}

		public long UpdateTeamBagSync(GuidBagWrapper bagWrapper)
		{
			SkipSwordfishTeamService.Log.Info("UpdateTeamBagSync");
			return 0L;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SkipSwordfishFriendService));
	}
}
