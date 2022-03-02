using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionResultRpcStub : BaseComponentStub
	{
		public CharacterSelectionResultRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionResultRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionResultRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionResultRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionResultRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionResultRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionResultRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionResultRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionResultRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionResultRpcDispatch _dispatch;
	}
}
