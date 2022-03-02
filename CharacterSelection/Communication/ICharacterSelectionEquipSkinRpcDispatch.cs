using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionEquipSkinRpcDispatch : IDispatch
	{
		void SendEquipSkinRequest(Guid skinId);
	}
}
