using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionBanStepResultRpcStub : BaseComponentStub
	{
		public CharacterSelectionBanStepResultRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionBanStepResultRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionBanStepResultRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionBanStepResultRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionBanStepResultRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionBanStepResultRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionBanStepResultRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionBanStepResultRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionBanStepResultRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionBanStepResultRpcDispatch _dispatch;
	}
}
