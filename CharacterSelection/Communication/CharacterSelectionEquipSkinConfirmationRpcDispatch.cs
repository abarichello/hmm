using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionEquipSkinConfirmationRpcDispatch : BaseRemoteStub<CharacterSelectionEquipSkinConfirmationRpcDispatch>, ICharacterSelectionEquipSkinConfirmationRpcDispatch, IDispatch
	{
		public CharacterSelectionEquipSkinConfirmationRpcDispatch(int guid) : base(guid)
		{
		}

		public void SendEquipSkinConfirmations(bool success, long playerId, Guid characterId, Guid skinId)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1055, 4, base.IsReliable, new object[]
			{
				success,
				playerId,
				characterId,
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
