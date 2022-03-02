using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionPickConfirmationRejectionRpcStub : BaseComponentStub
	{
		public CharacterSelectionPickConfirmationRejectionRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionPickConfirmationRejectionRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionPickConfirmationRejectionRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionPickConfirmationRejectionRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionPickConfirmationRejectionRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickConfirmationRejectionRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionPickConfirmationRejectionRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickConfirmationRejectionRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionPickConfirmationRejectionRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionPickConfirmationRejectionRpcDispatch _dispatch;
	}
}
