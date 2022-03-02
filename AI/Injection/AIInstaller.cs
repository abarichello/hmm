using System;
using HeavyMetalMachines.AI.Steering;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.BotAI.Infra;
using HeavyMetalMachines.Infra.DependencyInjection.Installers;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.AI.Injection
{
	public class AIInstaller : ServerMonoInstaller<AIInstaller>
	{
		protected override void Bind()
		{
			base.Container.Bind<IBotDifficultyCalculatorProvider>().To<BotDifficultyCalculatorProvider>().AsTransient();
			base.Container.Bind<IGetBotDifficulty>().To<GetBotDifficulty>().AsTransient();
			base.Container.Bind<IAIAgentFactory>().To<HMMAgentFactory>().AsSingle();
			base.Container.Bind<IAIScheduler>().FromInstance(this.Scheduler);
			base.Container.Bind<ISteeringContextParameters>().FromInstance(this.SteeringParameters);
			base.Container.Bind<IAIStaticSceneElementCollection>().To<AIStaticScene>().AsSingle();
			base.Container.Bind<IAIStaticSceneElementIterator>().To<AIStaticScene>().AsSingle();
			base.Container.Bind<ISteeringBehaviourFactory>().To<SteeringBehaviourFactory>().AsSingle();
			if (this._config.GetBoolValue(ConfigAccess.EnableBotsV3))
			{
				base.Container.Bind<ISteering>().To<DirectionalSteering>().AsTransient();
				base.Container.Bind<ISteeringContextResult>().To<SteeringContextResult>().AsTransient();
				base.Container.Bind<ISteeringContextMapEvaluator>().To<DiscardDangerEvaluator>().AsTransient();
				base.Container.Bind<ISteeringContextMapEvaluator>().To<SumEvaluator>().AsTransient();
				base.Container.Bind<ISteeringContext>().To<SteeringContext>().AsTransient();
			}
			else if (!this._config.GetBoolValue(ConfigAccess.DisableBotsV2))
			{
				base.Container.Bind<ISteering>().To<DirectionalSteering>().AsTransient();
				base.Container.Bind<ISteeringContext>().To<SimpleSteeringContext>().AsTransient();
			}
			else
			{
				base.Container.Bind<ISteering>().To<LegacySteering>().AsTransient();
				base.Container.Bind<ISteeringContext>().To<SimpleSteeringContext>().AsTransient();
			}
		}

		[SerializeField]
		private SteeringContextParameters SteeringParameters;

		[SerializeField]
		private UnityAIScheduler Scheduler;

		[Inject]
		private IConfigLoader _config;
	}
}
