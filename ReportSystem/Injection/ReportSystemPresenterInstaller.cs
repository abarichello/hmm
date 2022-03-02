using System;
using HeavymetalMachines.ReportSystem;
using Zenject;

namespace HeavyMetalMachines.ReportSystem.Injection
{
	public class ReportSystemPresenterInstaller : MonoInstaller<ReportSystemPresenterInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IFeedbackWindowPresenter>().To<FeedbackWindowPresenter>().AsSingle();
		}
	}
}
