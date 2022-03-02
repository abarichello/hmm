using System;
using Hoplon.DependencyInjection;
using Zenject;

namespace HeavyMetalMachines.DependencyInjection
{
	public class ZenjectInjectionResolver : IInjectionResolver
	{
		public ZenjectInjectionResolver(DiContainer injectionContainer)
		{
			this._injectionContainer = injectionContainer;
		}

		public TImplementation Resolve<TImplementation>()
		{
			return this._injectionContainer.Resolve<TImplementation>();
		}

		private readonly DiContainer _injectionContainer;
	}
}
