using System;
using HeavyMetalMachines.Players.Business;
using Hoplon.DependencyInjection;

namespace HeavymetalMachines.Player.Infra
{
	public class SwordfishPlayerModule : IInjectionModule, IInjectionBindable
	{
		public SwordfishPlayerModule(IInjectionBinder injectionBinder)
		{
			this._injectionBinder = injectionBinder;
		}

		public void Bind()
		{
			this._injectionBinder.BindSingle<ILocalPlayerRestrictionsService, LocalPlayerRestrictionsService>();
		}

		private readonly IInjectionBinder _injectionBinder;
	}
}
