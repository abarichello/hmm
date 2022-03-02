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
	public class ArenaModifierApplyOnBombDeliverState : IArenaModifierApplier
	{
		public ArenaModifierApplyOnBombDeliverState(IScoreBoard gameModeStateProvider, IModifierService modifierService, ICombatControllerStorage combatControllerStorage, IArenaModifierStorage arenaModifierStorage)
		{
			this._gameModeStateProvider = gameModeStateProvider;
			this._modifierService = modifierService;
			this._combatControllerStorage = combatControllerStorage;
			this._arenaModifierStorage = arenaModifierStorage;
			this._removableConditions = new List<ArenaModifierCondition>();
			this._possibleConditions = new List<ArenaModifierCondition>
			{
				ArenaModifierCondition.PlayerAllTime,
				ArenaModifierCondition.AlliesAllTime,
				ArenaModifierCondition.EnemiesAllTime
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
				this._disposable = ObservableExtensions.Subscribe<ScoreBoardState>(this._gameModeStateProvider.StateChangedObservation, delegate(ScoreBoardState state)
				{
					this.ApplyModifiers(state);
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

		private void ApplyModifiers(ScoreBoardState state)
		{
			if (state.CurrentState == BombScoreboardState.BombDelivery)
			{
				this.AddPassive(this._playerId, ArenaModifierCondition.PlayerAllTime);
				this.AddPassive(this._alliesIds, ArenaModifierCondition.AlliesAllTime);
				this.AddPassive(this._enemiesIds, ArenaModifierCondition.EnemiesAllTime);
			}
			else
			{
				this.ClearConditionalModifiers();
			}
		}

		private IScoreBoard _gameModeStateProvider;

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
