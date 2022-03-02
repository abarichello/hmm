using System;
using HeavyMetalMachines.Common;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers
{
	public class ClientCheckApplicationType : ICheckApplicationType
	{
		public bool IsClient()
		{
			return true;
		}

		public bool IsServer()
		{
			return false;
		}
	}
}
