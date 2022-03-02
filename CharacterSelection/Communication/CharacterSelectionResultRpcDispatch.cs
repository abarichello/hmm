using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionResultRpcDispatch : BaseRemoteStub<CharacterSelectionResultRpcDispatch>, ICharacterSelectionResultRpcDispatch, IDispatch
	{
		public CharacterSelectionResultRpcDispatch(int guid) : base(guid)
		{
		}

		public void ReceiveResult(CharacterSelectionResultSerialized result)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1063, 3, base.IsReliable, new object[]
			{
				result
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
