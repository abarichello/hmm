using System;
using System.Collections.Generic;
using HeavyMetalMachines.Arena.Infra;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Business;
using HeavyMetalMachines.Combat.Infra;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.Counselor;
using HeavyMetalMachines.SensorSystem;
using Hoplon.Assertions;
using Hoplon.SensorSystem;
using UniRx;

namespace HeavyMetalMachines.Arena.Business
{
	public class ArenaModifierApplyOnDistanceToGroundBomb : IArenaModifierApplier, IObserver
	{
		public ArenaModifierApplyOnDistanceToGroundBomb(IBombManager bombManager, IModifierService modifierService, ICombatControllerStorage combatControllerStorage, IArenaModifierStorage arenaModifierStorage, ISensorContextProvider sensorContextProvider, int distanceToApply)
		{
			this._distanceToApply = distanceToApply;
			Assert.IsNotNull<IBombManager>(bombManager, "Can't create ArenaModifierApplyOnBombCarrierChange with null bombManager");
			Assert.IsNotNull<IModifierService>(modifierService, "Can't create ArenaModifierApplyOnBombCarrierChange with null modifierService");
			Assert.IsNotNull<ICombatControllerStorage>(combatControllerStorage, "Can't create ArenaModifierApplyOnBombCarrierChange with null combatControllerStorage");
			Assert.IsNotNull<IArenaModifierStorage>(arenaModifierStorage, "Can't create ArenaModifierApplyOnBombCarrierChange with null arenaModifierStorage");
			this._bombManager = bombManager;
			this._modifierService = modifierService;
			this._combatControllerStorage = combatControllerStorage;
			this._arenaModifierStorage = arenaModifierStorage;
			this._sensorContextProvider = sensorContextProvider;
			this._removableConditions = new List<ArenaModifierCondition>();
			this._possibleConditions = new List<ArenaModifierCondition>
			{
				ArenaModifierCondition.PlayerWithoutBombAndDistanceToBombApplyToAllies,
				ArenaModifierCondition.PlayerWithoutBombAndDistanceToBombApplyToEnemies
			};
			this._disposables = new CompositeDisposable();
		}

		public void Dispose()
		{
			this._disposables.Dispose();
		}

		public void Init(int playerId, List<int> alliesIds, List<int> enemiesIds)
		{
			this._playerId = playerId;
			this._alliesIds = alliesIds;
			this._enemiesIds = enemiesIds;
			if (this._removableConditions.Contains(ArenaModifierCondition.PlayerWithoutBombAndDistanceToBombApplyToAllies))
			{
				this.AddSensorFor(this._alliesIds, ArenaModifierCondition.PlayerWithoutBombAndDistanceToBombApplyToAllies);
			}
			if (this._removableConditions.Contains(ArenaModifierCondition.PlayerWithoutBombAndDistanceToBombApplyToEnemies))
			{
				this.AddSensorFor(this._enemiesIds, ArenaModifierCondition.PlayerWithoutBombAndDistanceToBombApplyToEnemies);
			}
		}

		public void AddConditionIfValid(ArenaModifierCondition condition)
		{
			if (this._possibleConditions.Contains(condition))
			{
				this._removableConditions.Add(condition);
			}
		}

		private void AddSensorFor(List<int> combatIdList, ArenaModifierCondition modifierCondition)
		{
			for (int i = 0; i < combatIdList.Count; i++)
			{
				int combatId = combatIdList[i];
				ISensor sensor = this.CreateSensor(combatId, modifierCondition);
				this._disposables.Add(ObservableExtensions.Subscribe<bool>(this.ObserveBombCarryingAndSensor(sensor.Id), delegate(bool result)
				{
					this.CheckAndChangeModifiers(sensor.Id, result);
				}));
			}
		}

		private ISensor CreateSensor(int combatId, ArenaModifierCondition modifierCondition)
		{
			NumericCondition numericCondition = new NumericCondition(this._sensorContextProvider.SensorContext, ServerCounselorController.ScannerParameters.PlayerDistanceToBomb + combatId.ToString(), 1, (float)this._distanceToApply);
			int num = this._sensorContextProvider.SensorContext.AddCondition(numericCondition);
			ISensor sensor = this._sensorContextProvider.SensorContext.AddSensor();
			sensor.AddConditionId(num);
			sensor.AddObserver(this);
			this._sensorToCombat.Add(sensor.Id, new Tuple2<int, ArenaModifierCondition>(combatId, modifierCondition));
			this._sensorObservations.Add(sensor.Id, new Subject<bool>());
			return sensor;
		}

		private void CheckAndChangeModifiers(int id, bool isAdd)
		{
			Tuple2<int, ArenaModifierCondition> tuple = this._sensorToCombat[id];
			if (isAdd)
			{
				this.AddPassive(tuple.First, tuple.Second);
			}
			else
			{
				this.RemovePassive(tuple.First, tuple.Second);
			}
		}

		private IObservable<bool> ObserveBombCarryingAndSensor(int id)
		{
			return Observable.DistinctUntilChanged<bool>(Observable.Select<IList<bool>, bool>(Observable.CombineLatest<bool>(new IObservable<bool>[]
			{
				this.IsPlayerNotCarryingBomb(),
				this.IsSensorActive(id)
			}), new Func<IList<bool>, bool>(this.IsAllTrue)));
		}

		private bool IsAllTrue(IList<bool> conditions)
		{
			return conditions[0] && conditions[1];
		}

		private IObservable<bool> IsSensorActive(int id)
		{
			return this._sensorObservations[id];
		}

		private IObservable<bool> IsPlayerNotCarryingBomb()
		{
			return Observable.Select<Unit, bool>(this._bombManager.OnBombCarrierChanged(), (Unit _) => this.CheckPlayerIsNotLastCarrier());
		}

		public bool CheckPlayerIsNotLastCarrier()
		{
			int lastCarrierObjId = this._bombManager.GetLastCarrierObjId();
			return lastCarrierObjId != this._playerId;
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

		private void AddPassive(int combatId, ArenaModifierCondition condition)
		{
			ModifierData[] byArenaCondition = this._arenaModifierStorage.GetByArenaCondition(condition);
			if (byArenaCondition.Length > 0)
			{
				ICombatController byObjId = this._combatControllerStorage.GetByObjId(combatId);
				this._modifierService.AddPassiveModifiersFromEnvironment(byObjId, byArenaCondition);
			}
		}

		public void Notify(int id, bool isActive, int count)
		{
			Subject<bool> subject;
			if (this._sensorObservations.TryGetValue(id, out subject))
			{
				subject.OnNext(isActive);
			}
		}

		private IBombManager _bombManager;

		private IModifierService _modifierService;

		private ICombatControllerStorage _combatControllerStorage;

		private IArenaModifierStorage _arenaModifierStorage;

		private ISensorContextProvider _sensorContextProvider;

		private List<int> _enemiesIds;

		private List<int> _alliesIds;

		private int _playerId;

		private List<ArenaModifierCondition> _removableConditions;

		private readonly List<ArenaModifierCondition> _possibleConditions;

		private CompositeDisposable _disposables;

		private int _distanceToApply;

		private Dictionary<int, Tuple2<int, ArenaModifierCondition>> _sensorToCombat = new Dictionary<int, Tuple2<int, ArenaModifierCondition>>();

		private Dictionary<int, Subject<bool>> _sensorObservations = new Dictionary<int, Subject<bool>>();
	}
}
