using System;
using HeavyMetalMachines.AI.Steering;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.AI
{
	public class HMMAgentFactory : IAIAgentFactory
	{
		public IAIAgent CreateAIAgent(GameObject agentObject)
		{
			HMMAgent hmmagent = new HMMAgent();
			Identifiable component = agentObject.GetComponent<Identifiable>();
			CarComponentHub componentHub = component.GetComponentHub<CarComponentHub>();
			componentHub.AIAgent = hmmagent;
			hmmagent.GoalManager = this._container.InstantiateComponent<BotAIGoalManager>(agentObject);
			hmmagent.GoalManager.CarHub = componentHub;
			hmmagent.GoalManager.Combat = componentHub.combatObject;
			hmmagent.Goals = (hmmagent.GoalManager.Goals = this.GetGoals(componentHub.Player));
			hmmagent.PathFind = this._container.InstantiateComponent<BotAIPathFind>(agentObject);
			hmmagent.Controller = this._container.InstantiateComponent<BotAIController>(agentObject);
			hmmagent.Controller.AIAgent = hmmagent;
			hmmagent.GoalManager.BotAIController = hmmagent.Controller;
			hmmagent.Controller.goalManager = hmmagent.GoalManager;
			hmmagent.Controller.Directives = hmmagent.GoalManager;
			SteeringBotContext botContext = new SteeringBotContext
			{
				BotParameters = hmmagent.Goals,
				BotInput = componentHub.carInput,
				BotSubject = new SteeringSubject
				{
					SubjectTransform = agentObject.transform,
					SubjectRigidbody = agentObject.GetComponent<Rigidbody>(),
					Combat = componentHub.combatObject
				},
				DesiredDestination = null
			};
			hmmagent.BotContext = botContext;
			hmmagent.SteeringContext = this._container.Resolve<ISteeringContext>();
			hmmagent.SteeringContext.SetBotContext(hmmagent.BotContext);
			this._aiScheduler.AddTask(hmmagent.SteeringContext);
			return hmmagent;
		}

		private BotAIGoal GetGoals(PlayerData player)
		{
			string characterBiName = player.GetCharacterBiName();
			BotAIGoal.BotDifficulty botDifficulty = this._getBotDifficulty.Get(player.Team);
			BotAIGoal botAiGoal = player.GetBotAiGoal(botDifficulty);
			if (botAiGoal == null)
			{
				HMMAgentFactory.Log.ErrorFormat("Team={0} Character={1} with unknown difficulty: {2}. Will fall back to easy.", new object[]
				{
					player.Team,
					characterBiName,
					botDifficulty
				});
				botDifficulty = BotAIGoal.BotDifficulty.Easy;
				botAiGoal = player.GetBotAiGoal(botDifficulty);
			}
			HMMAgentFactory.Log.DebugFormat("Selected AI {0} for={1}:{2}", new object[]
			{
				botDifficulty,
				player.PlayerAddress,
				characterBiName
			});
			return Object.Instantiate<BotAIGoal>(botAiGoal);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HMMAgentFactory));

		[Inject]
		private DiContainer _container;

		[Inject]
		private IGetBotDifficulty _getBotDifficulty;

		[Inject]
		private IAIScheduler _aiScheduler;
	}
}
