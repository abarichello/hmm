using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionEquipSkinRpcAsync : IAsync
	{
		IFuture SendEquipSkinRequest(Guid skinId);
	}
}
