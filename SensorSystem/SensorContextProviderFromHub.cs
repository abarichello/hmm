using System;
using Hoplon.SensorSystem;
using Pocketverse;

namespace HeavyMetalMachines.SensorSystem
{
	public class SensorContextProviderFromHub : GameHubObject, ISensorContextProvider
	{
		public ISensorController SensorContext
		{
			get
			{
				return GameHubObject.Hub.SensorController.SensorContext;
			}
		}
	}
}
