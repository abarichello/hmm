using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionOthersPickConfirmationRpcDispatch : BaseRemoteStub<CharacterSelectionOthersPickConfirmationRpcDispatch>, ICharacterSelectionOthersPickConfirmationRpcDispatch, IDispatch
	{
		public CharacterSelectionOthersPickConfirmationRpcDispatch(int guid) : base(guid)
		{
		}

		public void BroadcastPickConfirmation(PickConfirmationSerialized pickConfirmation)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1059, 4, base.IsReliable, new object[]
			{
				pickConfirmation
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
