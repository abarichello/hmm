using System;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class InjectOnServer : InjectAttributeBase
	{
		public InjectOnServer()
		{
			base.Optional = InjectOnServer.IsOptional();
		}

		private static bool IsOptional()
		{
			return GameHubBehaviour.Hub.Net.IsClient();
		}
	}
}
