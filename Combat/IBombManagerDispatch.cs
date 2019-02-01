using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public interface IBombManagerDispatch : IDispatch
	{
		void OnGridGameStarted();

		void OnGridGameFinished(byte playerAdress, float finalValue);

		void OnPlayerUpdatedGridProgress(byte playerAddress, int clientProgress);

		void DisputeStarted();

		void DisputeFinished(int teamKind);

		void SlowMotionCallback(bool enable);

		void ClientEnableOvertimeEffects(int id);

		void ClientListenToBombAlmostDeliveredTriggerEnter(int trackTeamKind);

		void ClientListenToBombLastCurveTriggerEnter(int trackTeamKind);

		void ClientListenToBombFirstCurveTriggerEnter(int trackTeamKind);

		void ClientListenToBombTrackEntryTriggerEnter(int trackTeamKind);

		void ClientListenToBombAlmostDeliveredTriggerExit();
	}
}
