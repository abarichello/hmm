using System;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class InjectOnClient : InjectAttributeBase
	{
		public InjectOnClient()
		{
			base.Optional = InjectOnClient.IsOptional();
		}

		private static bool IsOptional()
		{
			return !(GameHubBehaviour.Hub == null) && GameHubBehaviour.Hub.Net.IsServer();
		}
	}
}
