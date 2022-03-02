using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IServerInfoAsync : IAsync
	{
		IFuture SetInfo(MatchData data);

		IFuture SetPlayerCompetitiveState(string state);

		IFuture SetPlayerRewards(string rewardString);

		IFuture PlaybackReady(long playbackStartTime, int lastSynchTimeScaleChange, int accumulatedSynchDelay, float timeScale);

		IFuture FullDataSent();

		IFuture ServerSet();

		IFuture ServerPlayerLoadingInfo(long playerId, float progress);

		IFuture ServerPlayerDisconnectInfo();

		IFuture ClientPlayerAFKTimeUpdate(float afkRemainingTime);

		IFuture ServerEventRequest();

		IFuture OnServerPlayerReady();

		IFuture ServerPlayerLoadingUpdate(float progress);

		IFuture ServerReloadAFKTime();

		IFuture ServerPlayerInputPressed();

		IFuture ServerLeaverWarningCallback(bool timedOut);
	}
}
