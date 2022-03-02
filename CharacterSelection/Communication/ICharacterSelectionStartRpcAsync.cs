using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionStartRpcAsync : IAsync
	{
		IFuture SendStarted();
	}
}
