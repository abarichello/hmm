using System;
using HeavyMetalMachines.Battlepass.Seasons;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers._3MainMenu
{
	public class BattlepassModeInstaller : ClientMonoInstaller<BattlepassModeInstaller>
	{
		protected override void Bind()
		{
			base.Container.Bind<IBattlepassSeasonNewsService>().To<BattlepassSeasonNewsService>().AsTransient();
			base.Container.Bind<IConsumeCurrentBattlepassSeasonNews>().To<ConsumeCurrentBattlepassSeasonNews>().AsTransient();
		}
	}
}
