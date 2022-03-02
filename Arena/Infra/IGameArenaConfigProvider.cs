using System;

namespace HeavyMetalMachines.Arena.Infra
{
	public interface IGameArenaConfigProvider
	{
		IGameArenaConfig GameArenaConfig { get; }
	}
}
