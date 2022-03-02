using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionStartRpcDispatch : BaseRemoteStub<CharacterSelectionStartRpcDispatch>, ICharacterSelectionStartRpcDispatch, IDispatch
	{
		public CharacterSelectionStartRpcDispatch(int guid) : base(guid)
		{
		}

		public void SendStarted()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1064, 4, base.IsReliable, new object[0]);
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
