using System;
using System.Collections.Generic;
using HeavyMetalMachines.Arena.Infra;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Business;
using HeavyMetalMachines.Combat.Infra;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.SensorSystem;
using Zenject;

namespace HeavyMetalMachines.Arena.Business
{
	public class ArenaModifierInitializer : IArenaModifierInitializer
	{
		public void InitializeModifierApplier()
		{
			this._modifierAppliers = new List<IArenaModifierApplier>();
			PlayerData playerData = this._matchPlayers.Players[0];
			TeamKind team = playerData.Team;
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			List<PlayerData> bots = this._matchPlayers.Bots;
			for (int i = 0; i < bots.Count; i++)
			{
				PlayerData playerData2 = bots[i];
				if (playerData2.Team == team)
				{
					list.Add(playerData2.CharacterInstance.ObjId);
				}
				else
				{
					list2.Add(playerData2.CharacterInstance.ObjId);
				}
			}
			this.InstantiateArenaModifiers();
			this.AddValidConditions();
			this.InitializeArenaModifiers(playerData, list, list2);
		}

		private void InitializeArenaModifiers(PlayerData player, List<int> allyIds, List<int> enemyIds)
		{
			for (int i = 0; i < this._modifierAppliers.Count; i++)
			{
				this._modifierAppliers[i].Init(player.CharacterInstance.ObjId, allyIds, enemyIds);
			}
		}

		private void AddValidConditions()
		{
			IGameArenaInfo gameArenaInfo = this._getCurrentArenaInfo.Get();
			foreach (ArenaModifierConfiguration arenaModifierConfiguration in gameArenaInfo.ModifiersToApply)
			{
				this._arenaModifierStorage.Set(arenaModifierConfiguration.Condition, ModifierData.CreateData(arenaModifierConfiguration.Modifier.Infos));
				this.AddConditionsToAppliers(arenaModifierConfiguration.Condition);
			}
		}

		private void AddConditionsToAppliers(ArenaModifierCondition condition)
		{
			for (int i = 0; i < this._modifierAppliers.Count; i++)
			{
				this._modifierAppliers[i].AddConditionIfValid(condition);
			}
		}

		private void InstantiateArenaModifiers()
		{
			IGameArenaInfo gameArenaInfo = this._getCurrentArenaInfo.Get();
			this._modifierAppliers.Add(new ArenaModifierApplyOnBombCarrierChange(this._bombManager, this._modifierService, this._combatControllerStorage, this._arenaModifierStorage));
			this._modifierAppliers.Add(new ArenaModifierApplyOnBombDeliverState(this._gameModeStateProvider, this._modifierService, this._combatControllerStorage, this._arenaModifierStorage));
			this._modifierAppliers.Add(new ArenaModifierApplyOnDistanceToGroundBomb(this._bombManager, this._modifierService, this._combatControllerStorage, this._arenaModifierStorage, this._sensorContextProvider, gameArenaInfo.ArenaModifierDistanceToBombToApply));
		}

		public void Dispose()
		{
			for (int i = 0; i < this._modifierAppliers.Count; i++)
			{
				this._modifierAppliers[i].Dispose();
			}
		}

		[Inject]
		private IGetCurrentArenaInfo _getCurrentArenaInfo;

		[Inject]
		private IArenaModifierStorage _arenaModifierStorage;

		[Inject]
		private IMatchPlayers _matchPlayers;

		[Inject]
		private IBombManager _bombManager;

		[Inject]
		private IScoreBoard _gameModeStateProvider;

		[Inject]
		private IModifierService _modifierService;

		[Inject]
		private ICombatControllerStorage _combatControllerStorage;

		[Inject]
		private ISensorContextProvider _sensorContextProvider;

		private List<IArenaModifierApplier> _modifierAppliers;
	}
}
