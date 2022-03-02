using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterChoiceChangesServerToClientsRpcStub : BaseComponentStub
	{
		public CharacterChoiceChangesServerToClientsRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterChoiceChangesServerToClientsRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterChoiceChangesServerToClientsRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterChoiceChangesServerToClientsRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterChoiceChangesServerToClientsRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterChoiceChangesServerToClientsRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterChoiceChangesServerToClientsRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterChoiceChangesServerToClientsRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterChoiceChangesServerToClientsRpcAsync _async;

		[ThreadStatic]
		private CharacterChoiceChangesServerToClientsRpcDispatch _dispatch;
	}
}
