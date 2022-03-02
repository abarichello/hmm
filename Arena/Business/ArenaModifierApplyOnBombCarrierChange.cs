using System;
using System.Collections.Generic;
using HeavyMetalMachines.Arena.Infra;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Business;
using HeavyMetalMachines.Combat.Infra;
using HeavyMetalMachines.Infra.Context;
using UniRx;

namespace HeavyMetalMachines.Arena.Business
{
	public class ArenaModifierApplyOnBombCarrierChange : IArenaModifierApplier
	{
		public ArenaModifierApplyOnBombCarrierChange(IBombManager bombManager, IModifierService modifierService, ICombatControllerStorage combatControllerStorage, IArenaModifierStorage arenaModifierStorage)
		{
			this._bombManager = bombManager;
			this._modifierService = modifierService;
			this._combatControllerStorage = combatControllerStorage;
			this._arenaModifierStorage = arenaModifierStorage;
			this._removableConditions = new List<ArenaModifierCondition>();
			this._possibleConditions = new List<ArenaModifierCondition>
			{
				ArenaModifierCondition.AllyCarryingBomb,
				ArenaModifierCondition.PlayerWithBombApplyToAllAllies,
				ArenaModifierCondition.EnemyWithBombApplyToAllAllies,
				ArenaModifierCondition.AllyWithBombApplyToAllAllies,
				ArenaModifierCondition.PlayerWithBombApplyToAllEnemies,
				ArenaModifierCondition.EnemyWithBombApplyToAllEnemies,
				ArenaModifierCondition.AllyWithBombApplyToAllEnemies,
				ArenaModifierCondition.EnemyWithBombApplyToPlayer,
				ArenaModifierCondition.AllyWithBombApplyToPlayer,
				ArenaModifierCondition.PlayerWithoutBombApplyToAllAllies,
				ArenaModifierCondition.EnemyWithoutBombApplyToAllAllies,
				ArenaModifierCondition.AllyWithoutBombApplyToAllAllies,
				ArenaModifierCondition.PlayerWithoutBombApplyToAllEnemies,
				ArenaModifierCondition.EnemyWithoutBombApplyToAllEnemies,
				ArenaModifierCondition.AllyWithoutBombApplyToAllEnemies,
				ArenaModifierCondition.EnemyWithoutBombApplyToPlayer,
				ArenaModifierCondition.AllyWithoutBombApplyToPlayer
			};
		}

		public void Dispose()
		{
			if (this._disposable != null)
			{
				this._disposable.Dispose();
			}
		}

		public void Init(int playerId, List<int> alliesIds, List<int> enemiesIds)
		{
			this._playerId = playerId;
			this._alliesIds = alliesIds;
			this._enemiesIds = enemiesIds;
			if (this._removableConditions.Count > 0)
			{
				this._disposable = ObservableExtensions.Subscribe<Unit>(this._bombManager.OnBombCarrierChanged(), delegate(Unit _)
				{
					this.ApplyModifiers();
				});
			}
		}

		public void AddConditionIfValid(ArenaModifierCondition condition)
		{
			if (this._possibleConditions.Contains(condition))
			{
				this._removableConditions.Add(condition);
			}
		}

		private void ClearConditionalModifiers()
		{
			for (int i = 0; i < this._removableConditions.Count; i++)
			{
				ArenaModifierCondition condition = this._removableConditions[i];
				this.RemovePassive(this._playerId, condition);
				this.RemovePassive(this._alliesIds, condition);
				this.RemovePassive(this._enemiesIds, condition);
			}
		}

		private void RemovePassive(int combatId, ArenaModifierCondition condition)
		{
			ModifierData[] byArenaCondition = this._arenaModifierStorage.GetByArenaCondition(condition);
			if (byArenaCondition.Length > 0)
			{
				ICombatController byObjId = this._combatControllerStorage.GetByObjId(combatId);
				this._modifierService.RemovePassiveModifiersFromEnvironment(byObjId, byArenaCondition);
			}
		}

		private void RemovePassive(List<int> list, ArenaModifierCondition condition)
		{
			for (int i = 0; i < list.Count; i++)
			{
				this.RemovePassive(list[i], condition);
			}
		}

		private void AddPassive(int combatId, ArenaModifierCondition condition)
		{
			ModifierData[] byArenaCondition = this._arenaModifierStorage.GetByArenaCondition(condition);
			if (byArenaCondition.Length > 0)
			{
				ICombatController byObjId = this._combatControllerStorage.GetByObjId(combatId);
				this._modifierService.AddPassiveModifiersFromEnvironment(byObjId, byArenaCondition);
			}
		}

		private void AddPassive(List<int> list, ArenaModifierCondition condition)
		{
			for (int i = 0; i < list.Count; i++)
			{
				this.AddPassive(list[i], condition);
			}
		}

		private void AddPassiveExcept(List<int> list, int exceptionId, ArenaModifierCondition condition)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != exceptionId)
				{
					this.AddPassive(list[i], condition);
				}
			}
		}

		private void ApplyModifiers()
		{
			this.ClearConditionalModifiers();
			int lastCarrierObjId = this._bombManager.GetLastCarrierObjId();
			if (lastCarrierObjId == this._playerId)
			{
				this.AddPassive(this._playerId, ArenaModifierCondition.PlayerCarryingBomb);
				this.AddPassive(this._alliesIds, ArenaModifierCondition.PlayerWithBombApplyToAllAllies);
				this.AddPassive(this._enemiesIds, ArenaModifierCondition.PlayerWithBombApplyToAllEnemies);
				this.ApplyEnemyWithoutBomb();
				this.ApplyAllyWithoutBomb();
				return;
			}
			if (this._alliesIds.Contains(lastCarrierObjId))
			{
				this.AddPassive(lastCarrierObjId, ArenaModifierCondition.AllyCarryingBomb);
				this.AddPassive(this._playerId, ArenaModifierCondition.AllyWithBombApplyToPlayer);
				this.AddPassiveExcept(this._alliesIds, lastCarrierObjId, ArenaModifierCondition.AllyWithBombApplyToAllAllies);
				this.AddPassive(this._enemiesIds, ArenaModifierCondition.AllyWithBombApplyToAllEnemies);
				this.ApplyPlayerWithoutBomb();
				this.ApplyEnemyWithoutBomb();
				return;
			}
			if (this._enemiesIds.Contains(lastCarrierObjId))
			{
				this.AddPassive(lastCarrierObjId, ArenaModifierCondition.EnemyCarryingBomb);
				this.AddPassive(this._playerId, ArenaModifierCondition.EnemyWithBombApplyToPlayer);
				this.AddPassive(this._alliesIds, ArenaModifierCondition.EnemyWithBombApplyToAllAllies);
				this.AddPassiveExcept(this._enemiesIds, lastCarrierObjId, ArenaModifierCondition.EnemyWithBombApplyToAllEnemies);
				this.ApplyPlayerWithoutBomb();
				this.ApplyAllyWithoutBomb();
				return;
			}
			this.ApplyEnemyWithoutBomb();
			this.ApplyAllyWithoutBomb();
			this.ApplyPlayerWithoutBomb();
		}

		private void ApplyPlayerWithoutBomb()
		{
			this.AddPassive(this._alliesIds, ArenaModifierCondition.PlayerWithoutBombApplyToAllAllies);
			this.AddPassive(this._enemiesIds, ArenaModifierCondition.PlayerWithoutBombApplyToAllEnemies);
		}

		private void ApplyAllyWithoutBomb()
		{
			this.AddPassive(this._playerId, ArenaModifierCondition.AllyWithoutBombApplyToPlayer);
			this.AddPassive(this._alliesIds, ArenaModifierCondition.AllyWithoutBombApplyToAllAllies);
			this.AddPassive(this._enemiesIds, ArenaModifierCondition.AllyWithoutBombApplyToAllEnemies);
		}

		private void ApplyEnemyWithoutBomb()
		{
			this.AddPassive(this._playerId, ArenaModifierCondition.EnemyWithoutBombApplyToPlayer);
			this.AddPassive(this._alliesIds, ArenaModifierCondition.EnemyWithoutBombApplyToAllAllies);
			this.AddPassive(this._enemiesIds, ArenaModifierCondition.EnemyWithoutBombApplyToAllEnemies);
		}

		private IBombManager _bombManager;

		private IModifierService _modifierService;

		private ICombatControllerStorage _combatControllerStorage;

		private IArenaModifierStorage _arenaModifierStorage;

		private List<int> _enemiesIds;

		private List<int> _alliesIds;

		private int _playerId;

		private List<ArenaModifierCondition> _removableConditions;

		private readonly List<ArenaModifierCondition> _possibleConditions;

		private IDisposable _disposable;
	}
}
