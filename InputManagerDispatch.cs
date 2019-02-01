using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class InputManagerDispatch : BaseRemoteStub<InputManagerDispatch>, IInputManagerDispatch, IDispatch
	{
		public InputManagerDispatch(int guid) : base(guid)
		{
		}

		public void ClientSendInput(PlayerController.InputMap inputs)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1019, 3, base.IsReliable, new object[]
			{
				inputs
			});
		}

		int IDispatch.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IDispatch.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}

		bool IDispatch.get_IsReliable()
		{
			return base.IsReliable;
		}

		void IDispatch.set_IsReliable(bool value)
		{
			base.IsReliable = value;
		}
	}
}
