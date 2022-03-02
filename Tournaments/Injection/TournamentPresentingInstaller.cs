using System;
using HeavyMetalMachines.DependencyInjection;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Tournaments.Presenting;
using Zenject;

namespace HeavyMetalMachines.Tournaments.Injection
{
	public class TournamentPresentingInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			ZenjectInjectionBinder zenjectInjectionBinder = new ZenjectInjectionBinder(base.Container);
			TournamentPresentingModule tournamentPresentingModule = new TournamentPresentingModule(zenjectInjectionBinder);
			tournamentPresentingModule.Bind();
			PresentingSystem presentingSystem = new PresentingSystem(zenjectInjectionBinder);
			presentingSystem.Bind();
		}
	}
}
