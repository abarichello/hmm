using System;
using HeavyMetalMachines.Battlepass.Business;
using HeavyMetalMachines.Battlepass.BuyLevels;
using HeavyMetalMachines.Battlepass.Rewards.Presenter;
using HeavyMetalMachines.Battlepass.View;
using HeavyMetalMachines.Infra.DependencyInjection.Installers;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Players.Business;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass.Injection
{
	public class BattlepassInstaller : ClientMonoInstaller<BattlepassInstaller>
	{
		protected override void Bind()
		{
			base.Container.BindInstance<BattlepassProgressScriptableObject>(this._localPlayerBattlepassProgress);
			base.Container.Bind<IGetBattlepassProgress>().To<LegacyGetBattlepassProgress>().AsSingle();
			base.Container.Bind<BattlepassPresenter>().AsTransient();
			base.Container.Bind<IBattlepassRewardsPresenter>().To<BattlepassRewardsPresenter>().AsTransient();
			base.Container.Bind<IBattlepassBuyLevelsPresenter>().To<BattlepassBuyLevelsPresenter>().AsTransient();
			base.Container.Bind<IObserveBattlepassProgress>().To<ObserveBattlepassProgress>().AsTransient();
		}

		[SerializeField]
		private BattlepassProgressScriptableObject _localPlayerBattlepassProgress;
	}
}
