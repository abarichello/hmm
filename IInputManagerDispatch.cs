using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IInputManagerDispatch : IDispatch
	{
		void ClientSendInput(PlayerController.InputMap inputs);
	}
}
