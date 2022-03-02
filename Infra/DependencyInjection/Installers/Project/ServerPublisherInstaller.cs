using System;
using Assets.Standard_Assets.Scripts.Infra;
using Assets.Standard_Assets.Scripts.Infra.Publisher;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers.Project
{
	public class ServerPublisherInstaller : MonoInstaller<ServerPublisherInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IPublisher>().To<ServerPublisher>().AsSingle();
		}
	}
}
