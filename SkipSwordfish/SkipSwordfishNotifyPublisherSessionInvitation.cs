using System;
using HeavyMetalMachines.HostingPlatforms;
using HeavyMetalMachines.Publishing;
using UnityEngine;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishNotifyPublisherSessionInvitation : INotifyPublisherSessionInvitation
	{
		public void Notify(SessionInvitationData sessionInvitationData)
		{
			Debug.LogFormat("Notify {0} {1}", new object[]
			{
				sessionInvitationData.SessionId,
				sessionInvitationData.InvitationId
			});
		}
	}
}
