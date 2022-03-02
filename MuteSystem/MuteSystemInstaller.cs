using System;
using HeavymetalMachines.ReportSystem;
using Zenject;

namespace HeavyMetalMachines.MuteSystem
{
	public class MuteSystemInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IMuteSystemPresenter>().To<MuteSystemPresenter>().AsSingle().Lazy();
			base.Container.Bind<IReportSystemPresenter>().To<ReportSystemPresenter>().AsSingle().Lazy();
		}
	}
}
