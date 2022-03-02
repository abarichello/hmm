using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionClientReadyRpcDispatch : BaseRemoteStub<CharacterSelectionClientReadyRpcDispatch>, ICharacterSelectionClientReadyRpcDispatch, IDispatch
	{
		public CharacterSelectionClientReadyRpcDispatch(int guid) : base(guid)
		{
		}

		public void SendReady()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1053, 4, base.IsReliable, new object[0]);
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
