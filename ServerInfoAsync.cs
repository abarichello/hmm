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
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 1, new object[]
			{
				data
			});
			return future;
		}

		public IFuture SetPlayerCompetitiveState(string state)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 3, new object[]
			{
				state
			});
			return future;
		}

		public IFuture SetPlayerRewards(string rewardString)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 4, new object[]
			{
				rewardString
			});
			return future;
		}

		public IFuture PlaybackReady(long playbackStartTime, int lastSynchTimeScaleChange, int accumulatedSynchDelay, float timeScale)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 6, new object[]
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
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 7, new object[0]);
			return future;
		}

		public IFuture ServerSet()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 8, new object[0]);
			return future;
		}

		public IFuture ServerPlayerLoadingInfo(long playerId, float progress)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 11, new object[]
			{
				playerId,
				progress
			});
			return future;
		}

		public IFuture ServerPlayerDisconnectInfo()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 13, new object[0]);
			return future;
		}

		public IFuture ClientPlayerAFKTimeUpdate(float afkRemainingTime)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 17, new object[]
			{
				afkRemainingTime
			});
			return future;
		}

		public IFuture ServerEventRequest()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 20, new object[0]);
			return future;
		}

		public IFuture OnServerPlayerReady()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 22, new object[0]);
			return future;
		}

		public IFuture ServerPlayerLoadingUpdate(float progress)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 26, new object[]
			{
				progress
			});
			return future;
		}

		public IFuture ServerReloadAFKTime()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 28, new object[0]);
			return future;
		}

		public IFuture ServerPlayerInputPressed()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 29, new object[0]);
			return future;
		}

		public IFuture ServerLeaverWarningCallback(bool timedOut)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 30, new object[]
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
