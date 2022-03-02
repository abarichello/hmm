using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionOthersPickConfirmationRpcStub : BaseComponentStub
	{
		public CharacterSelectionOthersPickConfirmationRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionOthersPickConfirmationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionOthersPickConfirmationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionOthersPickConfirmationRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionOthersPickConfirmationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionOthersPickConfirmationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionOthersPickConfirmationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionOthersPickConfirmationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionOthersPickConfirmationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionOthersPickConfirmationRpcDispatch _dispatch;
	}
}
