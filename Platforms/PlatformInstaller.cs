using System;
using HeavyMetalMachines.HostingPlatforms;
using HeavyMetalMachines.Login;
using HeavyMetalMachines.Publishing;
using Zenject;

namespace HeavyMetalMachines.Platforms
{
	public class PlatformInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IIsOnConsolePlatform>().To<IsOnConsolePlatform>().AsTransient();
			base.Container.Bind<IPlatformNotifications>().To<PlatformNotifications>().AsTransient();
			base.Container.Bind<ITogglePlatformSessionEnding>().To<TogglePlatformSessionEnding>().AsSingle();
			base.Container.Bind<IGetHostPlatform>().To<GetHostPlatform>().AsTransient();
			base.Container.Bind<IPlatformSessionInvitations>().To<PlatformSessionInvitations>().AsTransient();
			base.Container.Bind<ILogPlatformFocusChange>().To<LogPlatformFocusChange>().AsSingle();
		}
	}
}
