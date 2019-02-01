using System;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Tutorial.Behaviours;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class SetBotHpBehaviour : InGameTutorialBehaviourPathSystemHolder
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this._botStarter = base.GetComponent<BotStarterTutorialBehaviour>();
			this.SetHp(this._botStarter.BotData);
		}

		public void SetHp(PlayerData botData)
		{
			CombatData disabledComponent = BotAIUtils.GetDisabledComponent<CombatData>(botData.CharacterInstance);
			disabledComponent.HP = this.Hp;
		}

		public float Hp;

		private BotStarterTutorialBehaviour _botStarter;
	}
}
