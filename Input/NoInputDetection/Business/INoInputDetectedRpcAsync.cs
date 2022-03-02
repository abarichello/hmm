using System;
using Pocketverse;

namespace HeavyMetalMachines.Input.NoInputDetection.Business
{
	public interface INoInputDetectedRpcAsync : IAsync
	{
		IFuture ReceiveNoInputDetectedMessage(byte playerAddress);
	}
}
