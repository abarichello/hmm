using System;
using HeavyMetalMachines.ReportSystem.Infra;
using Zenject;

namespace HeavyMetalMachines.ReportSystem.Injection
{
	public class ReportSystemMainMenuInstaller : MonoInstaller<ReportSystemMainMenuInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IFeedbackStorage>().To<FeedbackStorage>().AsSingle();
			base.Container.Bind<IRestrictionStorage>().To<RestrictionStorage>().AsSingle();
			base.Container.Bind<IFeedbackWatcher>().To<FeedbackWatcher>().AsSingle();
		}
	}
}
