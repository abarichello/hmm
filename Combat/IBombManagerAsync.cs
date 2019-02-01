using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public interface IBombManagerAsync : IAsync
	{
		IFuture OnGridGameStarted();

		IFuture OnGridGameFinished(byte playerAdress, float finalValue);

		IFuture OnPlayerUpdatedGridProgress(byte playerAddress, int clientProgress);

		IFuture DisputeStarted();

		IFuture DisputeFinished(int teamKind);

		IFuture SlowMotionCallback(bool enable);

		IFuture ClientEnableOvertimeEffects(int id);

		IFuture ClientListenToBombAlmostDeliveredTriggerEnter(int trackTeamKind);

		IFuture ClientListenToBombLastCurveTriggerEnter(int trackTeamKind);

		IFuture ClientListenToBombFirstCurveTriggerEnter(int trackTeamKind);

		IFuture ClientListenToBombTrackEntryTriggerEnter(int trackTeamKind);

		IFuture ClientListenToBombAlmostDeliveredTriggerExit();
	}
}
