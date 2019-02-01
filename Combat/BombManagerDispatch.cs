using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class BombManagerDispatch : BaseRemoteStub<BombManagerDispatch>, IBombManagerDispatch, IDispatch
	{
		public BombManagerDispatch(int guid) : base(guid)
		{
		}

		public void OnGridGameStarted()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 7, base.IsReliable, new object[0]);
		}

		public void OnGridGameFinished(byte playerAdress, float finalValue)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 8, base.IsReliable, new object[]
			{
				playerAdress,
				finalValue
			});
		}

		public void OnPlayerUpdatedGridProgress(byte playerAddress, int clientProgress)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 9, base.IsReliable, new object[]
			{
				playerAddress,
				clientProgress
			});
		}

		public void DisputeStarted()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 20, base.IsReliable, new object[0]);
		}

		public void DisputeFinished(int teamKind)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 21, base.IsReliable, new object[]
			{
				teamKind
			});
		}

		public void SlowMotionCallback(bool enable)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 39, base.IsReliable, new object[]
			{
				enable
			});
		}

		public void ClientEnableOvertimeEffects(int id)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 49, base.IsReliable, new object[]
			{
				id
			});
		}

		public void ClientListenToBombAlmostDeliveredTriggerEnter(int trackTeamKind)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 59, base.IsReliable, new object[]
			{
				trackTeamKind
			});
		}

		public void ClientListenToBombLastCurveTriggerEnter(int trackTeamKind)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 60, base.IsReliable, new object[]
			{
				trackTeamKind
			});
		}

		public void ClientListenToBombFirstCurveTriggerEnter(int trackTeamKind)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 61, base.IsReliable, new object[]
			{
				trackTeamKind
			});
		}

		public void ClientListenToBombTrackEntryTriggerEnter(int trackTeamKind)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 62, base.IsReliable, new object[]
			{
				trackTeamKind
			});
		}

		public void ClientListenToBombAlmostDeliveredTriggerExit()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1029, 64, base.IsReliable, new object[0]);
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
