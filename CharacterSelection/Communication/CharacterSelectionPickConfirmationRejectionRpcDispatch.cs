using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionPickConfirmationRejectionRpcDispatch : BaseRemoteStub<CharacterSelectionPickConfirmationRejectionRpcDispatch>, ICharacterSelectionPickConfirmationRejectionRpcDispatch, IDispatch
	{
		public CharacterSelectionPickConfirmationRejectionRpcDispatch(int guid) : base(guid)
		{
		}

		public void RejectionSent(PickConfirmationRejectionSerialized pickConfirmationRejection)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1060, 2, base.IsReliable, new object[]
			{
				pickConfirmationRejection
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
