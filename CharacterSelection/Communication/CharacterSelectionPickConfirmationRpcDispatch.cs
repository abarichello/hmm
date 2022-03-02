using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionPickConfirmationRpcDispatch : BaseRemoteStub<CharacterSelectionPickConfirmationRpcDispatch>, ICharacterSelectionPickConfirmationRpcDispatch, IDispatch
	{
		public CharacterSelectionPickConfirmationRpcDispatch(int guid) : base(guid)
		{
		}

		public void SendPickConfirmation(Guid characterId)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1061, 4, base.IsReliable, new object[]
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
