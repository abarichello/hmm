using System;
using Pocketverse;

namespace HeavyMetalMachines.Character
{
	public interface ICharacterServiceDispatch : IDispatch
	{
		void SelectCharacter(int characterId);

		void ConfirmSelection(byte playerAddress, int characterId);

		void PickCharacter();

		void ConfirmPick(byte playerAddress, int teamID, int characterId, Guid lastSkin);

		void ConfirmSkin(string characterGuid, string skinGuid);

		void ConfirmSkinCallback(byte playerAddress, int teamID, bool success, string skinGuid);

		void GetPickTime();

		void ServerSendPickModeStateToPlayer();

		void PickTimeOutClient(float customizationTime);

		void SelectGrid(int gridIndex);

		void ConfirmGridSelection(byte playerAddress, int gridIndex);

		void PickGrid();

		void ConfirmGridPick(byte playerAddress, int gridIndex, Guid skinSelected);

		void ClientSendCounselorActivation(bool counselorActivation);
	}
}
