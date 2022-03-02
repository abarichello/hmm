using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionClientReadyRpcAsync : IAsync
	{
		IFuture SendReady();
	}
}
