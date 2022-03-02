using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionClientReadyRpcStub : BaseComponentStub
	{
		public CharacterSelectionClientReadyRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionClientReadyRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionClientReadyRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionClientReadyRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionClientReadyRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionClientReadyRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionClientReadyRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionClientReadyRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionClientReadyRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionClientReadyRpcDispatch _dispatch;
	}
}
