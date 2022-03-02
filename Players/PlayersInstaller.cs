using System;
using HeavyMetalMachines.Players.Business;
using Zenject;

namespace HeavyMetalMachines.Players
{
	public class PlayersInstaller : MonoInstaller<PlayersInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IGetPlayerAccountLevel>().To<LegacyGetPlayerAccountLevel>().AsTransient();
		}
	}
}
