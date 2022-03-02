using System;
using HeavyMetalMachines.Input.ControllerInput;
using Zenject;

namespace HeavyMetalMachines.ControllerInput
{
	public class ControllerInputInstaller : MonoInstaller<ControllerInputInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IControllerInputActionPoller>().To<ControllerInputActionPoller>().AsSingle().NonLazy();
		}
	}
}
