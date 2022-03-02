using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionOthersBanVoteConfirmationRpcStub : BaseComponentStub
	{
		public CharacterSelectionOthersBanVoteConfirmationRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionOthersBanVoteConfirmationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionOthersBanVoteConfirmationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionOthersBanVoteConfirmationRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionOthersBanVoteConfirmationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionOthersBanVoteConfirmationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionOthersBanVoteConfirmationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionOthersBanVoteConfirmationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionOthersBanVoteConfirmationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionOthersBanVoteConfirmationRpcDispatch _dispatch;
	}
}
