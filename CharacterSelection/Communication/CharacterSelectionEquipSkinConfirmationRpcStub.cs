using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionEquipSkinConfirmationRpcStub : BaseComponentStub
	{
		public CharacterSelectionEquipSkinConfirmationRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionEquipSkinConfirmationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionEquipSkinConfirmationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionEquipSkinConfirmationRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionEquipSkinConfirmationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionEquipSkinConfirmationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionEquipSkinConfirmationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionEquipSkinConfirmationRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionEquipSkinConfirmationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionEquipSkinConfirmationRpcDispatch _dispatch;
	}
}
