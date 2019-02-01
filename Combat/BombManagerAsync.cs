using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class BombManagerAsync : BaseRemoteStub<BombManagerAsync>, IBombManagerAsync, IAsync
	{
		public BombManagerAsync(int guid) : base(guid)
		{
		}

		public IFuture OnGridGameStarted()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 7, new object[0]);
			return future;
		}

		public IFuture OnGridGameFinished(byte playerAdress, float finalValue)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 8, new object[]
			{
				playerAdress,
				finalValue
			});
			return future;
		}

		public IFuture OnPlayerUpdatedGridProgress(byte playerAddress, int clientProgress)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 9, new object[]
			{
				playerAddress,
				clientProgress
			});
			return future;
		}

		public IFuture DisputeStarted()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 20, new object[0]);
			return future;
		}

		public IFuture DisputeFinished(int teamKind)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 21, new object[]
			{
				teamKind
			});
			return future;
		}

		public IFuture SlowMotionCallback(bool enable)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 39, new object[]
			{
				enable
			});
			return future;
		}

		public IFuture ClientEnableOvertimeEffects(int id)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 49, new object[]
			{
				id
			});
			return future;
		}

		public IFuture ClientListenToBombAlmostDeliveredTriggerEnter(int trackTeamKind)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 59, new object[]
			{
				trackTeamKind
			});
			return future;
		}

		public IFuture ClientListenToBombLastCurveTriggerEnter(int trackTeamKind)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 60, new object[]
			{
				trackTeamKind
			});
			return future;
		}

		public IFuture ClientListenToBombFirstCurveTriggerEnter(int trackTeamKind)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 61, new object[]
			{
				trackTeamKind
			});
			return future;
		}

		public IFuture ClientListenToBombTrackEntryTriggerEnter(int trackTeamKind)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 62, new object[]
			{
				trackTeamKind
			});
			return future;
		}

		public IFuture ClientListenToBombAlmostDeliveredTriggerExit()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1029, 64, new object[0]);
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
