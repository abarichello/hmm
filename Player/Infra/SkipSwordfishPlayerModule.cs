using System;
using HeavyMetalMachines.Players.Business;
using Hoplon.DependencyInjection;

namespace HeavymetalMachines.Player.Infra
{
	public class SkipSwordfishPlayerModule : IInjectionModule, IInjectionBindable
	{
		public SkipSwordfishPlayerModule(IInjectionBinder injectionBinder)
		{
			this._injectionBinder = injectionBinder;
		}

		public void Bind()
		{
			this._injectionBinder.BindSingle<ILocalPlayerRestrictionsService, SkipSwordfishLocalPlayerRestrictionsService>();
		}

		private readonly IInjectionBinder _injectionBinder;
	}
}
