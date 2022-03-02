using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionEquipSkinConfirmationRpcAsync : IAsync
	{
		IFuture SendEquipSkinConfirmations(bool success, long playerId, Guid characterId, Guid skinId);
	}
}
