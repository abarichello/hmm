using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public interface IPlayerPingDispatch : IDispatch
	{
		void ServerCreatePing(int pingKind);
	}
}
