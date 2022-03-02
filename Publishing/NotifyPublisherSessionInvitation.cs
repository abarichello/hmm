using System;
using ClientAPI.Publisher3rdp.Contracts;
using HeavyMetalMachines.HostingPlatforms;
using HeavyMetalMachines.Publishing.SessionService;
using UnityEngine;

namespace HeavyMetalMachines.Publishing
{
	public class NotifyPublisherSessionInvitation : INotifyPublisherSessionInvitation
	{
		public NotifyPublisherSessionInvitation(IPublisherGroup groupService, IPublisherSessionService publisherSessionService)
		{
			this._groupService = groupService;
			this._publisherSessionService = publisherSessionService;
		}

		public void Notify(SessionInvitationData data)
		{
			PublisherSessionInvitationData publisherSessionInvitationData = default(PublisherSessionInvitationData);
			publisherSessionInvitationData.InvitationId = data.InvitationId;
			publisherSessionInvitationData.SessionId = data.SessionId;
			publisherSessionInvitationData.ReferralAccountId = data.ReferralAccountId;
			publisherSessionInvitationData.ReferralOnlineId = data.ReferralOnlineId;
			PublisherSessionInvitationData publisherSessionInvitationData2 = publisherSessionInvitationData;
			Debug.LogFormat("Notify {0} {1} {2} {3}", new object[]
			{
				data.SessionId,
				data.InvitationId,
				data.ReferralAccountId,
				data.ReferralOnlineId
			});
			this._groupService.OnInviteReceived(publisherSessionInvitationData2);
		}

		private readonly IPublisherGroup _groupService;

		private readonly IPublisherSessionService _publisherSessionService;
	}
}
