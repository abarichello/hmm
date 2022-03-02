using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionClientReadyRpcDispatch : IDispatch
	{
		void SendReady();
	}
}
