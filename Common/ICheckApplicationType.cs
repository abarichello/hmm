using System;

namespace HeavyMetalMachines.Common
{
	public interface ICheckApplicationType
	{
		bool IsClient();

		bool IsServer();
	}
}
