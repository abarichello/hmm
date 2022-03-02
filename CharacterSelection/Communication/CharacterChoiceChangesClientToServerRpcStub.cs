using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterChoiceChangesClientToServerRpcStub : BaseComponentStub
	{
		public CharacterChoiceChangesClientToServerRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterChoiceChangesClientToServerRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterChoiceChangesClientToServerRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterChoiceChangesClientToServerRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterChoiceChangesClientToServerRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterChoiceChangesClientToServerRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterChoiceChangesClientToServerRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterChoiceChangesClientToServerRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterChoiceChangesClientToServerRpcAsync _async;

		[ThreadStatic]
		private CharacterChoiceChangesClientToServerRpcDispatch _dispatch;
	}
}
