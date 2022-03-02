using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionPickConfirmationRpcStub : BaseComponentStub
	{
		public CharacterSelectionPickConfirmationRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionPickConfirmationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionPickConfirmationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionPickConfirmationRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionPickConfirmationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickConfirmationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionPickConfirmationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickConfirmationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionPickConfirmationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionPickConfirmationRpcDispatch _dispatch;
	}
}
