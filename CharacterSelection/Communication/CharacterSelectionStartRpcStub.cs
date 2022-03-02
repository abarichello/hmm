using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionStartRpcStub : BaseComponentStub
	{
		public CharacterSelectionStartRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionStartRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionStartRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionStartRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionStartRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionStartRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionStartRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionStartRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionStartRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionStartRpcDispatch _dispatch;
	}
}
