using System;
using System.Runtime.InteropServices;

namespace HeavyMetalMachines.Orbis
{
	public struct OrbisSessionInvitationData
	{
		public string SessionId
		{
			get
			{
				return Marshal.PtrToStringAnsi(this._sessionId);
			}
		}

		public string InvitationId
		{
			get
			{
				return Marshal.PtrToStringAnsi(this._invitationId);
			}
		}

		public string ReferralOnlineId
		{
			get
			{
				return Marshal.PtrToStringAnsi(this._referralOnlineId);
			}
		}

		private IntPtr _sessionId;

		private IntPtr _invitationId;

		private IntPtr _referralOnlineId;

		public ulong ReferralAccountId;
	}
}
