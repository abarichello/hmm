using System;
using Hoplon.DependencyInjection;

namespace HeavyMetalMachines.Players.Business
{
	public class UnityPlayersBusinessModule : IInjectionModule, IInjectionBindable
	{
		public UnityPlayersBusinessModule(IInjectionBinder injectionBinder)
		{
			this._injectionBinder = injectionBinder;
		}

		public void Bind()
		{
			this._injectionBinder.BindTransient<ICopyPlayerTag, CopyPlayerTag>();
			this._injectionBinder.BindSingle<ILocalPlayerRestrictionsInitializer, LocalPlayerRestrictionsInitializer>();
			this._injectionBinder.BindSingle<ILocalPlayerRestrictionsStorage, LocalPlayerRestrictionsStorage>();
			this._injectionBinder.BindSingle<IIsPlayerRestrictedByTextChat, IsPlayerRestrictedByTextChat>();
			this._injectionBinder.BindSingle<ISetTextChatPlayerRestriction, SetTextChatPlayerRestriction>();
			this._injectionBinder.BindSingle<IIsLocalPlayerInMatch, IsLocalPlayerInMatch>();
			this._injectionBinder.BindSingle<IIsLocalPlayerInQueue, IsLocalPlayerInQueue>();
		}

		private readonly IInjectionBinder _injectionBinder;
	}
}
