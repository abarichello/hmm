using System;
using HeavyMetalMachines.Arena.Business;
using HeavyMetalMachines.CharacterSelection.Rotation;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers
{
	public class RotationInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			if (this._config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				base.Container.Bind<IRotationWeekStorage>().To<SkipSwordfishRotationWeekStorage>().AsSingle();
			}
			else
			{
				base.Container.Bind<IRotationWeekStorage>().To<RotationWeekStorage>().AsSingle();
			}
			base.Container.Bind<IGetPlayerRotation>().To<GetPlayerRotation>().AsTransient();
			base.Container.Bind<IGetCurrentArenaId>().To<LegacyGetCurrentArenaId>().AsTransient();
		}

		[Inject]
		private IConfigLoader _config;
	}
}
