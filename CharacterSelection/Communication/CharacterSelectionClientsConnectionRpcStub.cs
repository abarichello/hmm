using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionClientsConnectionRpcStub : BaseComponentStub
	{
		public CharacterSelectionClientsConnectionRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionClientsConnectionRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionClientsConnectionRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionClientsConnectionRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionClientsConnectionRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionClientsConnectionRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionClientsConnectionRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionClientsConnectionRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionClientsConnectionRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionClientsConnectionRpcDispatch _dispatch;
	}
}
