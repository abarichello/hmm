using System;
using HeavyMetalMachines.Social.Buttons;
using Zenject;

namespace HeavyMetalMachines.OpenUrl.Infra
{
	public class OpenUrlInstaller : MonoInstaller<OpenUrlInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IOpenUrlService>().To<OpenUrlService>().AsSingle();
			base.Container.Bind<IOpenUrl>().To<OpenUrl>().AsSingle();
			base.Container.Bind<IOpenUrlUgcRestricted>().To<OpenUrlUgcRestricted>().AsTransient();
			base.Container.Bind<ICallWebApi>().To<SwordfishCallWebApi>().AsSingle();
			base.Container.Bind<ISocialButtonsOpenUrl>().To<SocialButtonsOpenUrl>().AsTransient();
		}
	}
}
