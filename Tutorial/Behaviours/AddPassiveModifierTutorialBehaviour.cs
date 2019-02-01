using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class AddPassiveModifierTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this._modifierDatas = ModifierData.CreateData(this._modifierInfos);
			this.AddOrRemovePassiveModifier(true);
			this.CompleteBehaviourAndSync();
		}

		public void RemovePassiveModifier()
		{
			this.AddOrRemovePassiveModifier(false);
		}

		private void AddOrRemovePassiveModifier(bool shouldAddModif)
		{
			EventScopeKind targetType = this._targetType;
			if (targetType != EventScopeKind.Player)
			{
				if (targetType != EventScopeKind.Creep)
				{
					if (targetType != EventScopeKind.Bot)
					{
						throw new Exception("Invalid TargetType!!!");
					}
					this.IterateAddingOrRemovingPassiveModifier<PlayerData>(shouldAddModif, GameHubBehaviour.Hub.Players.Bots, (PlayerData b) => b.Team == this.TeamKind && b.CharacterInstance != null, (PlayerData b) => b.CharacterInstance.GetComponent<CombatController>());
				}
				else
				{
					List<CreepController> list = (this.TeamKind != TeamKind.Blue) ? GameHubBehaviour.Hub.Events.Creeps.GetRedCreepList() : GameHubBehaviour.Hub.Events.Creeps.GetBlueCreepList();
					this.IterateAddingOrRemovingPassiveModifier<CreepController>(shouldAddModif, list, (CreepController c) => true, (CreepController c) => c.CreepCombat.Controller);
					if (shouldAddModif)
					{
						GameHubBehaviour.Hub.Events.Creeps.ListenToCreepSpawn += this.OnCreepSpawn;
					}
					else
					{
						GameHubBehaviour.Hub.Events.Creeps.ListenToCreepSpawn -= this.OnCreepSpawn;
					}
				}
			}
			else
			{
				this.IterateAddingOrRemovingPassiveModifier<PlayerData>(shouldAddModif, GameHubBehaviour.Hub.Players.Players, (PlayerData p) => p.Team == this.TeamKind && p.CharacterInstance != null, (PlayerData p) => p.CharacterInstance.GetComponent<CombatController>());
			}
		}

		private void IterateAddingOrRemovingPassiveModifier<T>(bool shouldAddModif, List<T> list, Predicate<T> filter, Func<T, CombatController> combatControllerSelector)
		{
			List<T> source = list.FindAll(filter);
			IEnumerable<CombatController> enumerable = source.Select(combatControllerSelector);
			foreach (CombatController combatController in enumerable)
			{
				if (shouldAddModif)
				{
					combatController.AddPassiveModifiers(this._modifierDatas, null, -1);
				}
				else
				{
					combatController.RemovePassiveModifiers(this._modifierDatas, null, -1);
				}
			}
		}

		private void OnCreepSpawn(CreepController creepcontroller)
		{
			creepcontroller.Combat.Controller.AddPassiveModifiers(this._modifierDatas, null, -1);
		}

		public EventScopeKind _targetType;

		public TeamKind TeamKind;

		public ModifierInfo[] _modifierInfos;

		private ModifierData[] _modifierDatas;
	}
}
