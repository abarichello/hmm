using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterChoiceChangesClientToServerRpcAsync : BaseRemoteStub<CharacterChoiceChangesClientToServerRpcAsync>, ICharacterChoiceChangesClientToServerRpcAsync, IAsync
	{
		public CharacterChoiceChangesClientToServerRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture CharacterChoiceReceived(Guid characterId)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1049, 2, new object[]
			{
				characterId
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
