using System;
using System.Diagnostics;
using ClientAPI.Publisher3rdp.Contracts;
using UnityEngine;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishPublisherGroup : IPublisherGroup
	{
		public void OnInviteReceived(PublisherSessionInvitationData sessionInvitationData)
		{
			Debug.LogFormat("OnInviteReceived {0} {1}", new object[]
			{
				sessionInvitationData.SessionId,
				sessionInvitationData.InvitationId
			});
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<PublisherGroupEvent> InviteReceived;
	}
}
