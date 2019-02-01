using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class ServerInfoAsync : BaseRemoteStub<ServerInfoAsync>, IServerInfoAsync, IAsync
	{
		public ServerInfoAsync(int guid) : base(guid)
		{
		}

		public IFuture SetInfo(MatchData data)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 1, new object[]
			{
				data
			});
			return future;
		}

		public IFuture SetPlayerRewards(string rewardString)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 3, new object[]
			{
				rewardString
			});
			return future;
		}

		public IFuture PlaybackReady(long playbackStartTime, int lastSynchTimeScaleChange, int accumulatedSynchDelay, float timeScale)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 7, new object[]
			{
				playbackStartTime,
				lastSynchTimeScaleChange,
				accumulatedSynchDelay,
				timeScale
			});
			return future;
		}

		public IFuture FullDataSent()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 8, new object[0]);
			return future;
		}

		public IFuture ServerSet()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 9, new object[0]);
			return future;
		}

		public IFuture ServerPlayerLoadingInfo(long playerId, float progress)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 10, new object[]
			{
				playerId,
				progress
			});
			return future;
		}

		public IFuture ServerPlayerDisconnectInfo()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 12, new object[0]);
			return future;
		}

		public IFuture ClientPlayerAFKTimeUpdate(float afkRemainingTime)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 16, new object[]
			{
				afkRemainingTime
			});
			return future;
		}

		public IFuture ServerEventRequest()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 19, new object[0]);
			return future;
		}

		public IFuture OnServerPlayerReady()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 21, new object[0]);
			return future;
		}

		public IFuture ServerPlayerLoadingUpdate(float progress)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 25, new object[]
			{
				progress
			});
			return future;
		}

		public IFuture ServerReloadAFKTime()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 27, new object[0]);
			return future;
		}

		public IFuture ServerPlayerInputPressed()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 28, new object[0]);
			return future;
		}

		public IFuture ServerLeaverWarningCallback(bool timedOut)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 29, new object[]
			{
				timedOut
			});
			return future;
		}

		int IAsync.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IAsync.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}
	}
}
