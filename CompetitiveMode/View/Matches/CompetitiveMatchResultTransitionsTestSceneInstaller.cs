using System;
using Assets.Standard_Assets.Scripts.HMM.CompetitiveMode.View;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Net.Infra;
using HeavyMetalMachines.Swordfish.Overlay;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class CompetitiveMatchResultTransitionsTestSceneInstaller : MonoInstaller<CompetitiveMatchResultTransitionsTestSceneInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IHMMPlayerPrefs>().To<EmptyHmmPlayerPrefs>().AsTransient();
			base.Container.Bind<IMatchTeams>().To<FakeMatchTeams>().AsTransient();
			base.Container.Bind<IOverlayProvider>().To<FakeOverlayProvider>().AsTransient();
			base.Container.Bind<IWaitAndGetMyPlayerCompetitiveStateProgress>().To<FakeWaitAndGetMyPlayerCompetitiveStateProgress>().AsTransient();
			base.Container.Bind<IGetCompetitiveDivisions>().To<FakeGetCompetitiveDivisions>().AsTransient();
			base.Container.Bind<ICompetitiveDivisionsBadgeNameBuilder>().To<CompetitiveDivisionsBadgeNameBuilder>().AsTransient();
			base.Container.Bind<IGetCurrentOrNextCompetitiveSeason>().To<FakeGetCurrentOrNextCompetitiveSeason>().AsTransient();
			base.Container.Bind<INetwork>().To<FakeNetwork>().AsTransient();
			base.Container.Bind<IInitializable>().To<Initializable>().AsSingle();
			base.Container.Bind<ManualInitializeLocalization>().AsSingle();
			base.Container.BindInstance<FmodFakeLoader>(this._fmodFakeLoader);
			BitLogger.Initialize(new FakeLogOutput());
		}

		[SerializeField]
		private FmodFakeLoader _fmodFakeLoader;
	}
}
