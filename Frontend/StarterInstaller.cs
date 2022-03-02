using System;
using HeavyMetalMachines.DependencyInjection;
using HeavyMetalMachines.HardwareAnalysis;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class StarterInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			base.Container.BindInstance<SystemCheckSettings>(this._systemCheckSettings);
			base.Container.Bind<IExecuteStarterState>().To<ExecuteStarterState>().AsTransient();
			base.Container.Bind<IShowSystemCheckDialogs>().To<ShowSystemCheckDialogs>().AsTransient();
			base.Container.Bind<ISystemRequirementsProvider>().To<UnitySystemRequirementsProvider>().AsTransient();
			base.Container.Bind<IHardwareSpecificationsProvider>().To<UnityHardwareSpecificationsProvider>().AsTransient();
			base.Container.Bind<ICrashReporter>().FromMethod((InjectContext _) => CrashReporter.Get());
			base.Container.Bind<ISendSystemSpecificationsToBi>().To<SendSystemSpecificationsToBi>().AsTransient();
			ZenjectInjectionBinder zenjectInjectionBinder = new ZenjectInjectionBinder(base.Container);
			new HardwareAnalysisModule(zenjectInjectionBinder).Bind();
		}

		[SerializeField]
		private SystemCheckSettings _systemCheckSettings;
	}
}
