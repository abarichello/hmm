using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionEquipSkinConfirmationRpcDispatch : IDispatch
	{
		void SendEquipSkinConfirmations(bool success, long playerId, Guid characterId, Guid skinId);
	}
}
