using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Crossplay.DataTransferObjects;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Business;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class CanLocalPlayerJoinGroup : ICanLocalPlayerJoinGroup
	{
		public CanLocalPlayerJoinGroup(ILocalPlayerStorage localPlayerStorage, IGetLocalPlayerCrossplayData localPlayerCrossplayData, IIsLocalPlayerInMatch localPlayerInMatch, IGroupInviteRestriction groupInviteRestriction, IMatchManager matchManager)
		{
			this._localPlayerInMatch = localPlayerInMatch;
			this._groupInviteRestriction = groupInviteRestriction;
			this._matchManager = matchManager;
			this._localPlayerStorage = localPlayerStorage;
			this._localPlayerCrossplayData = localPlayerCrossplayData;
		}

		public bool CanJoin(bool isOwnerCrossplayEnabled, string ownerPublisher, out string reason)
		{
			IPlayer player = this._localPlayerStorage.Player;
			if (this._groupInviteRestriction.ShouldRestrict(player.PlayerId))
			{
				reason = "RestrictedByParentalControl";
				return false;
			}
			if (this._localPlayerInMatch.IsInMatch())
			{
				reason = "NotInMainMenu";
				return false;
			}
			if (this._matchManager.IsUserInLobby)
			{
				reason = "IsUserInLobby";
				return false;
			}
			if (this.IsJoinGroupBlockedByCrossplay(isOwnerCrossplayEnabled, ownerPublisher, out reason))
			{
				return false;
			}
			reason = string.Empty;
			return true;
		}

		private bool IsJoinGroupBlockedByCrossplay(bool isOwnerCrossplayEnabled, string ownerPublisher, out string reason)
		{
			PlayerCrossplayData playerCrossplayData = this._localPlayerCrossplayData.Get();
			if (playerCrossplayData == null)
			{
				reason = "InvalidLocalCrossplayData";
				return true;
			}
			if (isOwnerCrossplayEnabled)
			{
				if (!playerCrossplayData.IsEnabled)
				{
					reason = "CrossplayIncompatibleByGuestOff";
					return true;
				}
			}
			else if (string.CompareOrdinal(playerCrossplayData.Publisher, ownerPublisher) != 0)
			{
				reason = "CrossplayIncompatibleByDifferentPublisher";
				return true;
			}
			reason = string.Empty;
			return false;
		}

		private readonly IIsLocalPlayerInMatch _localPlayerInMatch;

		private readonly IGroupInviteRestriction _groupInviteRestriction;

		private readonly IMatchManager _matchManager;

		private readonly ILocalPlayerStorage _localPlayerStorage;

		private readonly IGetLocalPlayerCrossplayData _localPlayerCrossplayData;
	}
}
