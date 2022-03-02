using System;
using HeavyMetalMachines.Infra.DependencyInjection.Installers;
using HeavyMetalMachines.Spectator.View;
using UnityEngine;

namespace HeavyMetalMachines.Spectator.Injection
{
	public class SpectatorInstaller : ClientMonoInstaller<SpectatorInstaller>
	{
		protected override void Bind()
		{
			base.Container.BindInstance<SpectatorQueueFilterTable>(this._spectatorQueueFilterTable);
			base.Container.Bind<IGetSpectatorQueueFilter>().To<GetSpectatorQueueFilter>().AsSingle();
			base.Container.Bind<ISpectatorService>().To<SpectatorService>().AsSingle();
			base.Container.Bind<ISpectatorCameraManager>().To<SpectatorCameraManager>().AsSingle();
			base.Container.Bind<ISpectatorHelperFactory>().To<SpectatorHelperFactory>().AsSingle();
			base.Container.Bind<ISpectatorHelperPresenter>().To<SpectatorHelperPresenter>().AsTransient();
			base.Container.Bind<ISpectatorCameraConfigProvider>().To<SpectatorCameraConfigProvider>().AsTransient();
		}

		[SerializeField]
		private SpectatorQueueFilterTable _spectatorQueueFilterTable;
	}
}
