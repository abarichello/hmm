using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionStartRpcDispatch : IDispatch
	{
		void SendStarted();
	}
}
