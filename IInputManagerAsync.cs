using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IInputManagerAsync : IAsync
	{
		IFuture ClientSendInput(PlayerController.InputMap inputs);
	}
}
