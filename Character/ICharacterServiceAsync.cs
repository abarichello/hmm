using System;
using Pocketverse;

namespace HeavyMetalMachines.Character
{
	public interface ICharacterServiceAsync : IAsync
	{
		IFuture<bool> SelectCharacter(int characterId);

		IFuture ConfirmSelection(byte playerAddress, int characterId);

		IFuture<int> PickCharacter();

		IFuture ConfirmPick(byte playerAddress, int teamID, int characterId, Guid lastSkin);

		IFuture ConfirmSkin(string characterGuid, string skinGuid);

		IFuture ConfirmSkinCallback(byte playerAddress, int teamID, bool success, string skinGuid);

		IFuture<float> GetPickTime();

		IFuture ServerSendPickModeStateToPlayer();

		IFuture PickTimeOutClient(float customizationTime);

		IFuture SelectGrid(int gridIndex);

		IFuture ConfirmGridSelection(byte playerAddress, int gridIndex);

		IFuture<int> PickGrid();

		IFuture ConfirmGridPick(byte playerAddress, int gridIndex, Guid skinSelected);

		IFuture ClientSendCounselorActivation(bool counselorActivation);
	}
}
