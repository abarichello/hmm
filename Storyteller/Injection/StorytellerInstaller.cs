using System;
using HeavyMetalMachines.Infra.DependencyInjection.Installers;

namespace HeavyMetalMachines.Storyteller.Injection
{
	public class StorytellerInstaller : ClientMonoInstaller<StorytellerInstaller>
	{
		protected override void Bind()
		{
			base.Container.Bind<IStorytellerQueueProvider>().To<StorytellerQueueProvider>().AsTransient();
			base.Container.Bind<IStorytellerTranslationProvider>().To<StorytellerTranslationProvider>().AsTransient();
			base.Container.Bind<IStorytellerGameserverSearchService>().To<StorytellerGameserverSearchService>().AsTransient();
			base.Container.Bind<IStorytellerGameserverSearch>().To<StorytellerGameserverSearch>().AsTransient();
			base.Container.Bind<IStorytellerMatchConnectionService>().To<StorytellerMatchConnectionService>().AsTransient();
			base.Container.Bind<IStorytellerMatchConnection>().To<StorytellerMatchConnection>().AsTransient();
		}
	}
}
