using System;
using HeavyMetalMachines.Social.Avatar.Infra;
using Hoplon.Assertions;
using Hoplon.DependencyInjection;

namespace HeavyMetalMachines.Social.Avatar
{
	public class SwordfishAvatarModule : IInjectionModule, IInjectionBindable
	{
		public SwordfishAvatarModule(IInjectionBinder injectionBinder)
		{
			Assert.ConstructorParametersAreNotNull(new object[]
			{
				injectionBinder
			});
			this._injectionBinder = injectionBinder;
		}

		public void Bind()
		{
			this._injectionBinder.BindTransient<IPlayerAvatarProvider, SwordfishPlayerAvatarProvider>();
		}

		private readonly IInjectionBinder _injectionBinder;
	}
}
