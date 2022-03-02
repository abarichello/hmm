using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionEquipSkinRpcAsync : BaseRemoteStub<CharacterSelectionEquipSkinRpcAsync>, ICharacterSelectionEquipSkinRpcAsync, IAsync
	{
		public CharacterSelectionEquipSkinRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture SendEquipSkinRequest(Guid skinId)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1056, 4, new object[]
			{
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
