using System;
using Pocketverse;

namespace HeavyMetalMachines.HMMChat
{
	public interface IChatServiceAsync : IAsync
	{
		IFuture ReceiveMessage(bool group, string msg);

		IFuture ClientReceiveMessage(bool group, string msg, byte playeraddress);
	}
}
