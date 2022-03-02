using System;
using Pocketverse;

namespace HeavyMetalMachines.HMMChat
{
	public interface IChatServiceAsync : IAsync
	{
		IFuture ReceiveMessage(bool group, string msg);

		IFuture ReceiveDraftMessage(bool toTeam, string draft, string context, string[] messageParameters);

		IFuture ClientReceiveMessage(bool group, string msg, byte playeraddress);

		IFuture ClientReceiveDraftMessage(bool toTeam, string draft, string context, string[] messageParameters, byte playeraddress);
	}
}
