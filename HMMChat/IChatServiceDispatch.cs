using System;
using Pocketverse;

namespace HeavyMetalMachines.HMMChat
{
	public interface IChatServiceDispatch : IDispatch
	{
		void ReceiveMessage(bool group, string msg);

		void ReceiveDraftMessage(bool toTeam, string draft, string context, string[] messageParameters);

		void ClientReceiveMessage(bool group, string msg, byte playeraddress);

		void ClientReceiveDraftMessage(bool toTeam, string draft, string context, string[] messageParameters, byte playeraddress);
	}
}
