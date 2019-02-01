using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class ServerInfoDispatch : BaseRemoteStub<ServerInfoDispatch>, IServerInfoDispatch, IDispatch
	{
		public ServerInfoDispatch(int guid) : base(guid)
		{
		}

		public void SetInfo(MatchData data)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 1, base.IsReliable, new object[]
			{
				data
			});
		}

		public void SetPlayerRewards(string rewardString)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 3, base.IsReliable, new object[]
			{
				rewardString
			});
		}

		public void PlaybackReady(long playbackStartTime, int lastSynchTimeScaleChange, int accumulatedSynchDelay, float timeScale)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 7, base.IsReliable, new object[]
			{
				playbackStartTime,
				lastSynchTimeScaleChange,
				accumulatedSynchDelay,
				timeScale
			});
		}

		public void FullDataSent()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 8, base.IsReliable, new object[0]);
		}

		public void ServerSet()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 9, base.IsReliable, new object[0]);
		}

		public void ServerPlayerLoadingInfo(long playerId, float progress)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 10, base.IsReliable, new object[]
			{
				playerId,
				progress
			});
		}

		public void ServerPlayerDisconnectInfo()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 12, base.IsReliable, new object[0]);
		}

		public void ClientPlayerAFKTimeUpdate(float afkRemainingTime)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 16, base.IsReliable, new object[]
			{
				afkRemainingTime
			});
		}

		public void ServerEventRequest()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 19, base.IsReliable, new object[0]);
		}

		public void OnServerPlayerReady()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 21, base.IsReliable, new object[0]);
		}

		public void ServerPlayerLoadingUpdate(float progress)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 25, base.IsReliable, new object[]
			{
				progress
			});
		}

		public void ServerReloadAFKTime()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 27, base.IsReliable, new object[0]);
		}

		public void ServerPlayerInputPressed()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 28, base.IsReliable, new object[0]);
		}

		public void ServerLeaverWarningCallback(bool timedOut)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1016, 29, base.IsReliable, new object[]
			{
				timedOut
			});
		}

		int IDispatch.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IDispatch.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}

		bool IDispatch.get_IsReliable()
		{
			return base.IsReliable;
		}

		void IDispatch.set_IsReliable(bool value)
		{
			base.IsReliable = value;
		}
	}
}
