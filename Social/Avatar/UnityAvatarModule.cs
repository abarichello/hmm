using System;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Social.Avatar.Business;
using HeavyMetalMachines.Social.Avatar.Infra;
using Hoplon.Assertions;
using Hoplon.DependencyInjection;

namespace HeavyMetalMachines.Social.Avatar
{
	public class UnityAvatarModule : IInjectionModule, IInjectionBindable
	{
		public UnityAvatarModule(IInjectionBinder injectionBinder)
		{
			Assert.ConstructorParametersAreNotNull(new object[]
			{
				injectionBinder
			});
			this._injectionBinder = injectionBinder;
		}

		public void Bind()
		{
			this._injectionBinder.BindTransient<IGetPlayerAvatarIconName, GetPlayerAvatarIconName>();
			this._injectionBinder.BindTransient<IGetLocalUserPortraitBorderIconName, GetLocalUserPortraitIconName>();
			this._injectionBinder.BindTransient<IGetPortraitBorderIconName, GetPortraitIconName>();
			this._injectionBinder.BindTransient<IGetAvatarGuid, GetAvatarGuid>();
			this._injectionBinder.BindSingle<IAvatarStorage, AvatarStorage>();
			this._injectionBinder.BindSingle<IAvatarStorageHandler, AvatarStorageHandler>();
			this._injectionBinder.BindTransient<IGetLocalPlayerCustomizationContent, GetLocalPlayerCustomizationContent>();
		}

		private readonly IInjectionBinder _injectionBinder;
	}
}
