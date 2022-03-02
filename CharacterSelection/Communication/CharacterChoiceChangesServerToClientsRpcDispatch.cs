using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterChoiceChangesServerToClientsRpcDispatch : BaseRemoteStub<CharacterChoiceChangesServerToClientsRpcDispatch>, ICharacterChoiceChangesServerToClientsRpcDispatch, IDispatch
	{
		public CharacterChoiceChangesServerToClientsRpcDispatch(int guid) : base(guid)
		{
		}

		public void CharacterChoiceReceived(CharacterChoiceSerialized characterChoiceSerialized)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1050, 3, base.IsReliable, new object[]
			{
				characterChoiceSerialized
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
