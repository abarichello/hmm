using System;
using Pocketverse;

namespace HeavyMetalMachines.Input.NoInputDetection.Business
{
	public interface INoInputDetectedRpcDispatch : IDispatch
	{
		void ReceiveNoInputDetectedMessage(byte playerAddress);
	}
}
