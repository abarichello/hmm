using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IServerInfoDispatch : IDispatch
	{
		void SetInfo(MatchData data);

		void SetPlayerCompetitiveState(string state);

		void SetPlayerRewards(string rewardString);

		void PlaybackReady(long playbackStartTime, int lastSynchTimeScaleChange, int accumulatedSynchDelay, float timeScale);

		void FullDataSent();

		void ServerSet();

		void ServerPlayerLoadingInfo(long playerId, float progress);

		void ServerPlayerDisconnectInfo();

		void ClientPlayerAFKTimeUpdate(float afkRemainingTime);

		void ServerEventRequest();

		void OnServerPlayerReady();

		void ServerPlayerLoadingUpdate(float progress);

		void ServerReloadAFKTime();

		void ServerPlayerInputPressed();

		void ServerLeaverWarningCallback(bool timedOut);
	}
}
