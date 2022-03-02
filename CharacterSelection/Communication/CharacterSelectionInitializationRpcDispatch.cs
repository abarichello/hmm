using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionInitializationRpcDispatch : BaseRemoteStub<CharacterSelectionInitializationRpcDispatch>, ICharacterSelectionInitializationRpcDispatch, IDispatch
	{
		public CharacterSelectionInitializationRpcDispatch(int guid) : base(guid)
		{
		}

		public void SendInitializationData(InitializationDataSerialized dataSerialized)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1057, 3, base.IsReliable, new object[]
			{
				dataSerialized
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
