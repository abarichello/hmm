using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionPickStepResultRpcDispatch : BaseRemoteStub<CharacterSelectionPickStepResultRpcDispatch>, ICharacterSelectionPickStepResultRpcDispatch, IDispatch
	{
		public CharacterSelectionPickStepResultRpcDispatch(int guid) : base(guid)
		{
		}

		public void ReceiveResult(PickStepResultSerialized pickStepResult)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1062, 3, base.IsReliable, new object[]
			{
				pickStepResult
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
