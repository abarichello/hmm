using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionPickStepResultRpcStub : BaseComponentStub
	{
		public CharacterSelectionPickStepResultRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionPickStepResultRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionPickStepResultRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionPickStepResultRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionPickStepResultRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickStepResultRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionPickStepResultRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickStepResultRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionPickStepResultRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionPickStepResultRpcDispatch _dispatch;
	}
}
