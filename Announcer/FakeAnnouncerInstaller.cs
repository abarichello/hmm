using System;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.CompetitiveMode.View.Matches;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Announcer
{
	public class FakeAnnouncerInstaller : MonoInstaller<FakeAnnouncerInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IScoreBoard>().To<FakeScoreBoard>().AsSingle();
			base.Container.Bind<IMatchPlayers>().To<FakeMatchPlayers>().AsSingle();
			base.Container.Bind<IMatchTeams>().To<FakeMatchTeams>().AsSingle();
			base.Container.Bind<IAnnouncerService>().To<FakeAnnouncerService>().AsSingle();
			base.Container.Bind<IGetCharacterData>().To<FakeGetCharacterData>().AsTransient();
		}

		private GameObject Collection;
	}
}
