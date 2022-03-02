using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterChoiceChangesClientToServerRpcDispatch : BaseRemoteStub<CharacterChoiceChangesClientToServerRpcDispatch>, ICharacterChoiceChangesClientToServerRpcDispatch, IDispatch
	{
		public CharacterChoiceChangesClientToServerRpcDispatch(int guid) : base(guid)
		{
		}

		public void CharacterChoiceReceived(Guid characterId)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1049, 2, base.IsReliable, new object[]
			{
				characterId
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
