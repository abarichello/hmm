using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IPauseControllerDispatch : IDispatch
	{
		void TogglePauseServer();

		void TriggerPauseNotification(int kind, float delay);

		void ChangePauseStateOnClient(int newState, long playerId);
	}
}
