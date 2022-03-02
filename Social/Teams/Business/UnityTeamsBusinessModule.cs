using System;
using Hoplon.Assertions;
using Hoplon.DependencyInjection;

namespace HeavyMetalMachines.Social.Teams.Business
{
	public class UnityTeamsBusinessModule : IInjectionModule, IInjectionBindable
	{
		public UnityTeamsBusinessModule(IInjectionBinder injectionBinder)
		{
			Assert.ConstructorParametersAreNotNull(new object[]
			{
				injectionBinder
			});
			this._injectionBinder = injectionBinder;
		}

		public void Bind()
		{
		}

		private readonly IInjectionBinder _injectionBinder;
	}
}
