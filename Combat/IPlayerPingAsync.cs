using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public interface IPlayerPingAsync : IAsync
	{
		IFuture ServerCreatePing(int pingKind);
	}
}
