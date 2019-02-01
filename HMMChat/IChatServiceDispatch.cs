using System;
using Pocketverse;

namespace HeavyMetalMachines.HMMChat
{
	public interface IChatServiceDispatch : IDispatch
	{
		void ReceiveMessage(bool group, string msg);

		void ClientReceiveMessage(bool group, string msg, byte playeraddress);
	}
}
