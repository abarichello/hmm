using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterChoiceChangesServerToClientsRpcAsync : BaseRemoteStub<CharacterChoiceChangesServerToClientsRpcAsync>, ICharacterChoiceChangesServerToClientsRpcAsync, IAsync
	{
		public CharacterChoiceChangesServerToClientsRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture CharacterChoiceReceived(CharacterChoiceSerialized characterChoiceSerialized)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1050, 3, new object[]
			{
				characterChoiceSerialized
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
