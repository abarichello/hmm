using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionOthersBanVoteConfirmationRpcDispatch : BaseRemoteStub<CharacterSelectionOthersBanVoteConfirmationRpcDispatch>, ICharacterSelectionOthersBanVoteConfirmationRpcDispatch, IDispatch
	{
		public CharacterSelectionOthersBanVoteConfirmationRpcDispatch(int guid) : base(guid)
		{
		}

		public void BroadcastBanVoteConfirmation(ServerBanVoteSerializable client)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1058, 4, base.IsReliable, new object[]
			{
				client
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
