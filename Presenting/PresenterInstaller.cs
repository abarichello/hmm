using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting.Navigation;
using HeavyMetalMachines.Presenting.Unity;
using Zenject;

namespace HeavyMetalMachines.Presenting
{
	public class PresenterInstaller : MonoInstaller<PresenterInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IViewLoader>().To<UnityViewLoader>().AsTransient();
			base.Container.Bind<IViewProvider>().To<ViewProvider>().AsSingle();
			base.Container.Bind<ITextColorFormatter>().To<UnityTextColorFormatter>().AsTransient();
			base.Container.Bind<IPresenterTree>().To<PresenterTree>().AsSingle();
			base.Container.Bind<IUIWindowFactory>().To<UIWindowFactory>().AsSingle();
		}
	}
}
