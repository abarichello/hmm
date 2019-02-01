using System;
using System.Collections.Generic;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class CreepBotController : CreepController, IBotAIDirectives, ICachedObject
	{
		protected override void Awake()
		{
		}

		public override void OnObjectSpawned(SpawnEvent evt)
		{
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
		}

		protected override void FixedUpdate()
		{
		}

		protected override void Update()
		{
		}

		public List<CombatObject> GetEnemies()
		{
			return this._enemyCarsList;
		}

		public bool AlwaysAttackNearTargets()
		{
			return true;
		}

		public bool IsPathFixed()
		{
			return this.PatrolController != null && this.PatrolController.Path;
		}

		public void SetController(BotAIController botAiController, CharacterInfo characterInfo)
		{
			botAiController.SetCreepController(this);
			BotAIGadgetShop component = botAiController.GetComponent<BotAIGadgetShop>();
			if (component != null)
			{
				component.enabled = false;
			}
			this.CreateEnemiesList();
		}

		private void CreateEnemiesList()
		{
			this._enemyCarsList.Clear();
			this.AddEnemyCreeps();
		}

		private void AddEnemyCreeps()
		{
			if (this.CreepCombat.CreepTeam == TeamKind.Neutral)
			{
				return;
			}
			List<CreepController> list = (this.CreepCombat.CreepTeam != TeamKind.Red) ? GameHubBehaviour.Hub.Events.Creeps.GetRedCreepList() : GameHubBehaviour.Hub.Events.Creeps.GetBlueCreepList();
			for (int i = 0; i < list.Count; i++)
			{
				CreepController creepController = list[i];
				this._enemyCarsList.Add(creepController.Combat);
			}
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepSpawn += this.OnCreepSpawn;
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn += this.OnCreepUnspawn;
			this._creepsListener = true;
		}

		private void OnCreepSpawn(CreepController creepcontroller)
		{
			if (creepcontroller == this)
			{
				return;
			}
			if (creepcontroller.CreepCombat.CreepTeam == this.CreepCombat.CreepTeam || creepcontroller.CreepCombat.CreepTeam == TeamKind.Neutral)
			{
				return;
			}
			this._enemyCarsList.Add(creepcontroller.Combat);
		}

		private void OnCreepUnspawn(CreepRemoveEvent data)
		{
			CombatObject combatObject = null;
			for (int i = 0; i < this._enemyCarsList.Count; i++)
			{
				CombatObject combatObject2 = this._enemyCarsList[i];
				if (combatObject2.Id.ObjId == data.CreepId)
				{
					combatObject = combatObject2;
					break;
				}
			}
			if (combatObject != null)
			{
				this._enemyCarsList.Remove(combatObject);
			}
		}

		public void OnSendToCache()
		{
			if (!this._creepsListener)
			{
				return;
			}
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepSpawn -= this.OnCreepSpawn;
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn -= this.OnCreepUnspawn;
			this._creepsListener = false;
		}

		public void OnGetFromCache()
		{
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CreepBotController));

		public BotPatrolController PatrolController;

		private List<CombatObject> _enemyCarsList = new List<CombatObject>();

		private bool _creepsListener;
	}
}
