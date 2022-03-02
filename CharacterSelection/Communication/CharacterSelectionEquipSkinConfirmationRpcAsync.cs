using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionEquipSkinConfirmationRpcAsync : BaseRemoteStub<CharacterSelectionEquipSkinConfirmationRpcAsync>, ICharacterSelectionEquipSkinConfirmationRpcAsync, IAsync
	{
		public CharacterSelectionEquipSkinConfirmationRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture SendEquipSkinConfirmations(bool success, long playerId, Guid characterId, Guid skinId)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1055, 4, new object[]
			{
				success,
				playerId,
				characterId,
				skinId
			});
			return future;
		}

		int IAsync.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IAsync.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}
	}
}
