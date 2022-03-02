using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionBanStepResultRpcDispatch : BaseRemoteStub<CharacterSelectionBanStepResultRpcDispatch>, ICharacterSelectionBanStepResultRpcDispatch, IDispatch
	{
		public CharacterSelectionBanStepResultRpcDispatch(int guid) : base(guid)
		{
		}

		public void ReceiveResult(BanStepResultSerializable banStepResult)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 1, base.IsReliable, new object[]
			{
				banStepResult
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
