using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace HeavyMetalMachines.Orbis
{
	public static class OrbisSessionInvitation
	{
		static OrbisSessionInvitation()
		{
			if (OrbisSessionInvitation.<>f__mg$cache0 == null)
			{
				OrbisSessionInvitation.<>f__mg$cache0 = new OrbisSessionInvitation.OnSessionInvitationDelegate(OrbisSessionInvitation.OnSessionInvitationCallbackFunction);
			}
			OrbisSessionInvitation._onSessionInvitationDelegate = OrbisSessionInvitation.<>f__mg$cache0;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action<OrbisSessionInvitationData> OnOrbisSessionInvitation;

		[MonoPInvokeCallback(typeof(OrbisSessionInvitation.OnSessionInvitationDelegate))]
		private static void OnSessionInvitationCallbackFunction(OrbisSessionInvitationData sessionInvitationData)
		{
			Debug.LogFormat("Received Invitation : {0} {1}", new object[]
			{
				sessionInvitationData.SessionId,
				sessionInvitationData.InvitationId
			});
			if (OrbisSessionInvitation.OnOrbisSessionInvitation != null)
			{
				OrbisSessionInvitation.OnOrbisSessionInvitation(sessionInvitationData);
			}
		}

		public static IntPtr GetOnSessionInvitationDelegateAddress()
		{
			return Marshal.GetFunctionPointerForDelegate(OrbisSessionInvitation._onSessionInvitationDelegate);
		}

		private static readonly OrbisSessionInvitation.OnSessionInvitationDelegate _onSessionInvitationDelegate;

		[CompilerGenerated]
		private static OrbisSessionInvitation.OnSessionInvitationDelegate <>f__mg$cache0;

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void OnSessionInvitationDelegate(OrbisSessionInvitationData sessionInvitationData);
	}
}
