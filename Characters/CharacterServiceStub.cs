using System;
using Pocketverse;

namespace HeavyMetalMachines.Characters
{
	public class CharacterServiceStub : BaseComponentStub
	{
		public CharacterServiceStub(int guid) : base(guid)
		{
		}

		public ICharacterServiceAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterServiceAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterServiceAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterServiceDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterServiceDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterServiceDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterServiceDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private CharacterServiceAsync _async;

		[ThreadStatic]
		private CharacterServiceDispatch _dispatch;
	}
}
