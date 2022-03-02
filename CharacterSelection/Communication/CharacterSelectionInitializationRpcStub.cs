using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionInitializationRpcStub : BaseComponentStub
	{
		public CharacterSelectionInitializationRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionInitializationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionInitializationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionInitializationRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionInitializationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionInitializationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionInitializationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionInitializationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionInitializationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionInitializationRpcDispatch _dispatch;
	}
}
