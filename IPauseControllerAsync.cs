using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IPauseControllerAsync : IAsync
	{
		IFuture TogglePauseServer();

		IFuture TriggerPauseNotification(int kind, float delay);

		IFuture ChangePauseStateOnClient(int newState, long playerId, int timeRemaining, int timeoutMillis, int activations);
	}
}
