using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionBanVoteConfirmationRpcStub : BaseComponentStub
	{
		public CharacterSelectionBanVoteConfirmationRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionBanVoteConfirmationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionBanVoteConfirmationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionBanVoteConfirmationRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionBanVoteConfirmationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionBanVoteConfirmationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionBanVoteConfirmationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionBanVoteConfirmationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionBanVoteConfirmationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionBanVoteConfirmationRpcDispatch _dispatch;
	}
}
