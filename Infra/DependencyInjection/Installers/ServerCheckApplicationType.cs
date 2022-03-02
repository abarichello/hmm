using System;
using HeavyMetalMachines.Common;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers
{
	public class ServerCheckApplicationType : ICheckApplicationType
	{
		public bool IsClient()
		{
			return false;
		}

		public bool IsServer()
		{
			return true;
		}
	}
}
