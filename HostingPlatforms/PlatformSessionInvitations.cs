using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.HostingPlatforms
{
	public class PlatformSessionInvitations : IPlatformSessionInvitations
	{
		public IEnumerable<SessionInvitationData> GetSessionInvitations()
		{
			return Platform.Current.GetSessionInvitations();
		}

		public void StartNotifyingSessionInvitations()
		{
			Platform.Current.StartNotifyingSessionInvitations();
		}

		public void StopNotifyingSessionInvitations()
		{
			Platform.Current.StopNotifyingSessionInvitations();
		}
	}
}
