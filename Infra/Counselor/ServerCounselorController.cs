using System;
using System.Collections.Generic;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Counselor;
using HeavyMetalMachines.Match;
using Hoplon.SensorSystem;
using Pocketverse;

namespace HeavyMetalMachines.Infra.Counselor
{
	public class ServerCounselorController : GameHubObject, IObserver
	{
		public void Initialize(SensorController sensorContext, string redBombDistanceToGoalParameterName, string blueBombDistanceToGoalParameterName, ICounselorDispatcher counselorDispatcher)
		{
			this._sensorContext = sensorContext;
			this._counselorDispatcher = counselorDispatcher;
			this._advices = new Dictionary<int, CounselorAdvice>();
			for (int i = 0; i < GameHubObject.Hub.Players.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = GameHubObject.Hub.Players.PlayersAndBots[i];
				CombatObject bitComponent = playerData.CharacterInstance.GetBitComponent<CombatObject>();
				this.CreatePlayerToBombScanner(bitComponent);
			}
			for (int j = 0; j < GameHubObject.Hub.Players.Players.Count; j++)
			{
				PlayerData playerData2 = GameHubObject.Hub.Players.Players[j];
				Dictionary<ServerCounselorController.CounselorConditions, SensorCondition> conditions = new Dictionary<ServerCounselorController.CounselorConditions, SensorCondition>();
				CombatObject bitComponent2 = playerData2.CharacterInstance.GetBitComponent<CombatObject>();
				int objId = bitComponent2.Id.ObjId;
				this.CreatePlayerScanner(bitComponent2);
				CarComponentHub componentHub = bitComponent2.Id.GetComponentHub<CarComponentHub>();
				this.ConfigureGadgets(componentHub, conditions, objId);
				for (int k = 0; k < GameHubObject.Hub.CounselorConfig._conditionalValues.Length; k++)
				{
					CounselorConfig.ConditionalConfig condition = GameHubObject.Hub.CounselorConfig._conditionalValues[k];
					this.AddCustomConditions(condition, conditions, bitComponent2);
				}
				this.AddGeneralConditions(conditions, bitComponent2, redBombDistanceToGoalParameterName, blueBombDistanceToGoalParameterName);
				NumericCondition numericCondition = new NumericCondition(this._sensorContext, ServerCounselorController.ScannerParameters.SpawnState.ToString() + objId, 0, 1f);
				int checkAliveId = this._sensorContext.AddCondition(numericCondition);
				this.ConfigureAdvicesForPlayer(playerData2, checkAliveId, conditions);
			}
		}

		private void ConfigureAdvicesForPlayer(PlayerData player, int checkAliveId, Dictionary<ServerCounselorController.CounselorConditions, SensorCondition> conditions)
		{
			for (int i = 0; i < GameHubObject.Hub.CounselorConfig.Advices.Length; i++)
			{
				CounselorConfig.AdvicesConfig advicesConfig = GameHubObject.Hub.CounselorConfig.Advices[i];
				if (this.IsCharacterAllowed(advicesConfig, player))
				{
					ISensor sensor = this._sensorContext.AddSensor();
					CounselorAdvice counselorAdvice = new CounselorAdvice();
					counselorAdvice.TargetPlayerAddress = player.PlayerAddress;
					counselorAdvice.ConfigIndex = i;
					if (advicesConfig.CheckAlive)
					{
						sensor.AddConditionId(checkAliveId);
					}
					this.ConfigureAdvice(sensor, advicesConfig, conditions);
					this._advices.Add(sensor.Id, counselorAdvice);
				}
			}
		}

		private void ConfigureAdvice(ISensor advice, CounselorConfig.AdvicesConfig adviceConfig, Dictionary<ServerCounselorController.CounselorConditions, SensorCondition> conditions)
		{
			for (int i = 0; i < adviceConfig.Conditions.Length; i++)
			{
				ServerCounselorController.CounselorConditions counselorConditions = adviceConfig.Conditions[i];
				SensorCondition sensorCondition;
				if (!conditions.TryGetValue(counselorConditions, out sensorCondition))
				{
					ServerCounselorController.Log.ErrorFormat("unconfigured advice: Name: {0} Condition {1}", new object[]
					{
						adviceConfig.ToString(),
						counselorConditions
					});
				}
				else
				{
					int num = this._sensorContext.AddCondition(sensorCondition);
					advice.AddConditionId(num);
				}
			}
			if (adviceConfig.WarmupSeconds > 0f)
			{
				advice.SetWarmup(adviceConfig.WarmupSeconds);
			}
			advice.AddObserver(this);
		}

		private void AddGeneralConditions(Dictionary<ServerCounselorController.CounselorConditions, SensorCondition> conditions, CombatObject currentCombat, string redBombDistanceToGoalParameterName, string blueBombDistanceToGoalParameterName)
		{
			int objId = currentCombat.Id.ObjId;
			conditions.Add(ServerCounselorController.CounselorConditions.BombWrongDirection, new NumericCondition(this._sensorContext, (currentCombat.Team != TeamKind.Red) ? "BlueAdvance" : "RedAdvance", 1, 0f));
			IGameArenaInfo currentArena = GameHubObject.Hub.ArenaConfig.GetCurrentArena();
			conditions.Add(ServerCounselorController.CounselorConditions.NearAttackGoal, new NumericCondition(this._sensorContext, (currentCombat.Team != TeamKind.Red) ? blueBombDistanceToGoalParameterName : redBombDistanceToGoalParameterName, 1, currentArena.NearGoalDistance));
			conditions.Add(ServerCounselorController.CounselorConditions.CollisionWithBlocker, new NumericCondition(this._sensorContext, ServerCounselorController.CounselorConditions.CollisionWithBlocker.ToString(), 2, 0.5f));
			conditions.Add(ServerCounselorController.CounselorConditions.DropBombByBrokenLink, new NumericCondition(this._sensorContext, ServerCounselorController.CounselorConditions.DropBombByBrokenLink.ToString() + objId, 2, 0f));
			conditions.Add(ServerCounselorController.CounselorConditions.DropBombByYellow, new NumericCondition(this._sensorContext, ServerCounselorController.CounselorConditions.DropBombByYellow.ToString() + objId, 2, 0f));
			conditions.Add(ServerCounselorController.CounselorConditions.DropBombByDeath, new NumericCondition(this._sensorContext, ServerCounselorController.CounselorConditions.DropBombByDeath.ToString() + objId, 2, 0f));
			conditions.Add(ServerCounselorController.CounselorConditions.FullLife, new NumericCondition(this._sensorContext, ServerCounselorController.ScannerParameters.PlayerHP.ToString() + objId, 0, 1f));
			conditions.Add(ServerCounselorController.CounselorConditions.IsMatchPoint, new NumericCondition(this._sensorContext, ServerCounselorController.CounselorConditions.IsMatchPoint.ToString(), 2, 0f));
			conditions.Add(ServerCounselorController.CounselorConditions.IsNotTutorial, new NumericCondition(this._sensorContext, ServerCounselorController.ScannerParameters.ArenaIndex.ToString(), 2, 0f));
			conditions.Add(ServerCounselorController.CounselorConditions.IsNotTrainingMode1, new NumericCondition(this._sensorContext, ServerCounselorController.ScannerParameters.ArenaIndex.ToString(), 3, 6f));
			conditions.Add(ServerCounselorController.CounselorConditions.IsNotTrainingMode2, new NumericCondition(this._sensorContext, ServerCounselorController.ScannerParameters.ArenaIndex.ToString(), 3, 7f));
			conditions.Add(ServerCounselorController.CounselorConditions.IsTutorial, new NumericCondition(this._sensorContext, ServerCounselorController.ScannerParameters.ArenaIndex.ToString(), 1, 1f));
		}

		private void AddCustomConditions(CounselorConfig.ConditionalConfig condition, Dictionary<ServerCounselorController.CounselorConditions, SensorCondition> conditions, CombatObject currentCombat)
		{
			ServerCounselorController.CounselorConditions condition2 = condition.condition;
			if (condition2 != ServerCounselorController.CounselorConditions.NearAnyGoal)
			{
				if (condition2 != ServerCounselorController.CounselorConditions.IsInOvertime)
				{
					if (condition2 != ServerCounselorController.CounselorConditions.JustBeginRound)
					{
						conditions.Add(condition.condition, new NumericCondition(this._sensorContext, condition.scanner.ToString() + currentCombat.Id.ObjId, condition.numericType, condition.value));
					}
					else
					{
						conditions.Add(condition.condition, new NumericCondition(this._sensorContext, ServerCounselorController.ScannerParameters.BombDeliveryDeltaTime.ToString(), condition.numericType, condition.value));
					}
				}
				else
				{
					conditions.Add(condition.condition, new NumericCondition(this._sensorContext, ServerCounselorController.CounselorConditions.IsInOvertime.ToString(), condition.numericType, condition.value));
				}
			}
			else
			{
				conditions.Add(condition.condition, new NumericCondition(this._sensorContext, ServerCounselorController.ScannerParameters.BombDistanceToGoal.ToString(), condition.numericType, condition.value));
			}
		}

		private void CreatePlayerToBombScanner(CombatObject currentCombat)
		{
			int objId = currentCombat.Id.ObjId;
			this._sensorContext.AddScanner(new PlayerToBombScanner(this._sensorContext, ServerCounselorController.ScannerParameters.PlayerDistanceToBomb.ToString() + objId, ServerCounselorController.ScannerParameters.PlayerCarryingBomb.ToString() + objId, currentCombat));
		}

		private void CreatePlayerScanner(CombatObject currentCombat)
		{
			int objId = currentCombat.Id.ObjId;
			this._sensorContext.AddScanner(new CombatScanner(this._sensorContext, ServerCounselorController.ScannerParameters.PlayerHP.ToString() + objId, ServerCounselorController.ScannerParameters.PlayerHasHealed.ToString() + objId, ServerCounselorController.ScannerParameters.HeavyDmg.ToString() + objId, ServerCounselorController.CounselorConditions.DropBombByBrokenLink.ToString() + objId, ServerCounselorController.CounselorConditions.DropBombByYellow.ToString() + objId, ServerCounselorController.CounselorConditions.DropBombByDeath.ToString() + objId, ServerCounselorController.ScannerParameters.Deaths.ToString() + objId, ServerCounselorController.ScannerParameters.SpawnState.ToString() + objId, ServerCounselorController.ScannerParameters.PlayerRole.ToString() + objId, ServerCounselorController.ScannerParameters.NotMovingSeconds.ToString() + objId, currentCombat));
		}

		private void ConfigureGadgets(CarComponentHub carhub, Dictionary<ServerCounselorController.CounselorConditions, SensorCondition> conditions, int combatObjId)
		{
			this.ConfigureGadgetScannerAndCondition(carhub, conditions, GadgetSlot.CustomGadget0, ServerCounselorController.CounselorConditions.BotShouldUseG0, ServerCounselorController.ScannerParameters.BotShouldUseG0.ToString() + combatObjId, ServerCounselorController.CounselorConditions.NotUsingG0, ServerCounselorController.ScannerParameters.IdleG0.ToString() + combatObjId);
			this.ConfigureGadgetScannerAndCondition(carhub, conditions, GadgetSlot.CustomGadget1, ServerCounselorController.CounselorConditions.BotShouldUseG1, ServerCounselorController.ScannerParameters.BotShouldUseG1.ToString() + combatObjId, ServerCounselorController.CounselorConditions.NotUsingG1, ServerCounselorController.ScannerParameters.IdleG1.ToString() + combatObjId);
			this.ConfigureGadgetScannerAndCondition(carhub, conditions, GadgetSlot.CustomGadget2, ServerCounselorController.CounselorConditions.BotShouldUseG2, ServerCounselorController.ScannerParameters.BotShouldUseG2.ToString() + combatObjId, ServerCounselorController.CounselorConditions.NotUsingG2, ServerCounselorController.ScannerParameters.IdleG2.ToString() + combatObjId);
			this.ConfigureGadgetScannerAndCondition(carhub, conditions, GadgetSlot.BoostGadget, ServerCounselorController.CounselorConditions.BotShouldUseGBoost, ServerCounselorController.ScannerParameters.BotShouldUseGBoost.ToString() + combatObjId, ServerCounselorController.CounselorConditions.NotUsingGBoost, ServerCounselorController.ScannerParameters.IdleGBoost.ToString() + combatObjId);
			this.ConfigureGadgetScannerAndCondition(carhub, conditions, GadgetSlot.BombGadget, ServerCounselorController.CounselorConditions.BotShouldUseGBomb, ServerCounselorController.ScannerParameters.BotShouldUseGBomb.ToString() + combatObjId, ServerCounselorController.CounselorConditions.NotUsingGBomb, ServerCounselorController.ScannerParameters.IdleGBomb.ToString() + combatObjId);
		}

		private void ConfigureGadgetScannerAndCondition(CarComponentHub carhub, Dictionary<ServerCounselorController.CounselorConditions, SensorCondition> conditionsDictionary, GadgetSlot slot, ServerCounselorController.CounselorConditions shouldUseCondition, string shouldUseParameterName, ServerCounselorController.CounselorConditions isIdleCondition, string isIdleParameterName)
		{
			this._sensorContext.AddScanner(new BotGadgetScanner(this._sensorContext, shouldUseParameterName, isIdleParameterName, carhub.AIAgent.GoalManager.GetGadgetState(slot)));
			conditionsDictionary.Add(shouldUseCondition, new NumericCondition(this._sensorContext, shouldUseParameterName, 0, 1f));
			conditionsDictionary.Add(isIdleCondition, new NumericCondition(this._sensorContext, isIdleParameterName, 0, 1f));
		}

		private bool IsCharacterAllowed(CounselorConfig.AdvicesConfig config, PlayerData player)
		{
			if (config.AllCharactersAllowed)
			{
				return true;
			}
			for (int i = 0; i < config.AllowedCharactersId.Length; i++)
			{
				if (player.CharacterId == config.AllowedCharactersId[i])
				{
					return true;
				}
			}
			return false;
		}

		public void Notify(int id, bool isActive, int count)
		{
			CounselorAdvice counselorAdvice;
			if (this._advices.TryGetValue(id, out counselorAdvice))
			{
				this._counselorDispatcher.Send(counselorAdvice.TargetPlayerAddress, counselorAdvice.ConfigIndex, isActive);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ServerCounselorController));

		private SensorController _sensorContext;

		private ICounselorDispatcher _counselorDispatcher;

		private Dictionary<int, CounselorAdvice> _advices;

		public enum CounselorConditions
		{
			None,
			LowLife,
			FullLife,
			DropBombByBrokenLink,
			DropBombByYellow,
			DropBombByDeath,
			CarryingBomb,
			NotCarryingBomb,
			NoHeal,
			NearBomb,
			FarBomb,
			NotUsingG0,
			NotUsingG1,
			NotUsingG2,
			NotUsingGBoost,
			NotUsingGBomb,
			BotShouldUseG0,
			BotShouldUseG1,
			BotShouldUseG2,
			BotShouldUseGBoost,
			BotShouldUseGBomb,
			CanRespawn,
			NearAttackGoal,
			NearAnyGoal = 24,
			BombWrongDirection,
			IsSupport,
			IsTransporter,
			IsInterceptor,
			NotInterceptor,
			NotTransporter,
			NotSupport,
			IsInOvertime,
			CollisionWithBlocker,
			IsDead,
			IsMatchPoint,
			JustBeginRound,
			NotMoving,
			FirstRound,
			HeavyDmgTaken,
			IsNotTutorial,
			IsTutorial,
			IsNotTrainingMode1,
			IsNotTrainingMode2
		}

		public enum ScannerParameters
		{
			None,
			PlayerHP,
			BombDistanceToGoal,
			PlayerCarryingBomb,
			PlayerHasHealed,
			PlayerRole,
			PlayerDistanceToBomb,
			IdleG0,
			IdleG1,
			IdleG2,
			IdleGBoost,
			IdleGBomb,
			BotShouldUseG0,
			BotShouldUseG1,
			BotShouldUseG2,
			BotShouldUseGBoost,
			BotShouldUseGBomb,
			HeavyDmg,
			Deaths,
			SpawnState,
			BombDeliveryDeltaTime,
			NotMovingSeconds,
			RoundNumber,
			ArenaIndex
		}
	}
}
