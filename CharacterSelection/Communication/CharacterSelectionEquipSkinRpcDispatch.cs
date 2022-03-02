using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionEquipSkinRpcDispatch : BaseRemoteStub<CharacterSelectionEquipSkinRpcDispatch>, ICharacterSelectionEquipSkinRpcDispatch, IDispatch
	{
		public CharacterSelectionEquipSkinRpcDispatch(int guid) : base(guid)
		{
		}

		public void SendEquipSkinRequest(Guid skinId)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1056, 4, base.IsReliable, new object[]
			{
				skinId
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
