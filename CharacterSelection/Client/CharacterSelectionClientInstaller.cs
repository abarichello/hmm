using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.API;
using HeavyMetalMachines.DependencyInjection;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class CharacterSelectionClientInstaller : MonoInstaller<CharacterSelectionClientInstaller>
	{
		public override void InstallBindings()
		{
			ZenjectInjectionBinder zenjectInjectionBinder = new ZenjectInjectionBinder(base.Container);
			new CharacterSelectionModule(zenjectInjectionBinder).Bind();
			new CharacterSelectionClientModule(zenjectInjectionBinder).Bind();
			new CharacterSelectionClientPresentingModule(zenjectInjectionBinder).Bind();
		}
	}
}
