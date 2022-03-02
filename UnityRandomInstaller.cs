using System;
using Hoplon;
using Zenject;

namespace HeavyMetalMachines
{
	public class UnityRandomInstaller : MonoInstaller<UnityRandomInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IRandom>().To<UnityRandom>().AsSingle();
		}
	}
}
