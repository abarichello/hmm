using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionEquipSkinRpcStub : BaseComponentStub
	{
		public CharacterSelectionEquipSkinRpcStub(int guid) : base(guid)
		{
		}

		public ICharacterSelectionEquipSkinRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionEquipSkinRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionEquipSkinRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionEquipSkinRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionEquipSkinRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionEquipSkinRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionEquipSkinRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterSelectionEquipSkinRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionEquipSkinRpcDispatch _dispatch;
	}
}
