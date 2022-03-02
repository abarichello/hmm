using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Counselor;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.Counselor;
using HeavyMetalMachines.Utils.Bezier;
using Hoplon.SensorSystem;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.SensorSystem
{
	public class HMMSensorController : GameHubBehaviour
	{
		public SensorController SensorContext
		{
			get
			{
				return this._sensorContext;
			}
		}

		public CounselorConfig Config
		{
			get
			{
				return GameHubBehaviour.Hub.CounselorConfig;
			}
		}

		public bool GetParameter(int id, out float value)
		{
			if (this._sensorContext == null)
			{
				value = 0f;
				return false;
			}
			return this._sensorContext.GetParameter(id, ref value);
		}

		public float DistanceToRedGoal
		{
			get
			{
				float result;
				if (this._sensorContext != null && this._sensorContext.GetParameter(this._redBombDistanceToGoalId, ref result))
				{
					return result;
				}
				return float.MaxValue;
			}
		}

		public float DistanceToBlueGoal
		{
			get
			{
				float result;
				if (this._sensorContext != null && this._sensorContext.GetParameter(this._blueBombDistanceToGoalId, ref result))
				{
					return result;
				}
				return float.MaxValue;
			}
		}

		private void OnEnable()
		{
			HMMSensorController.Log.Debug("HMMSensorController enabled");
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				HMMSensorController.Log.Error("HMMSensorController on client. This is unexpected behaviour");
				Object.Destroy(this);
				return;
			}
			this._sensorContext = new SensorController(new Func<int>(GameHubBehaviour.Hub.GameTime.MatchTimer.GetTime));
			GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned += this.ListenToAllPlayersSpawned;
			GameHubBehaviour.Hub.Events.Bots.ListenToAllPlayersSpawned += this.ListenToAllPlayersSpawned;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnListenToPhaseChange;
		}

		private void OnDisable()
		{
			HMMSensorController.Log.Debug("HMMSensorController disabled");
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned -= this.ListenToAllPlayersSpawned;
			GameHubBehaviour.Hub.Events.Bots.ListenToAllPlayersSpawned -= this.ListenToAllPlayersSpawned;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnListenToPhaseChange;
		}

		public void Update()
		{
			if (this._sensorContext != null && GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.BombDelivery)
			{
				if (PauseController.Instance.IsGamePaused)
				{
					this._sensorContext.Reset();
				}
				else
				{
					this._sensorContext.Update();
				}
			}
		}

		private void ListenToAllPlayersSpawned()
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				if (this.TryInitialize())
				{
					this._counselorController = new ServerCounselorController();
					this._counselorController.Initialize(this._sensorContext, "RedBombDistanceToGoal", "BlueBombDistanceToGoal", this._counselorDispatcher);
				}
				return;
			}
			if (!GameHubBehaviour.Hub.Events.Players.CarCreationFinished || !GameHubBehaviour.Hub.Events.Bots.CarCreationFinished)
			{
				return;
			}
			if (this.TryInitialize())
			{
				this._counselorController = new ServerCounselorController();
				this._counselorController.Initialize(this._sensorContext, "RedBombDistanceToGoal", "BlueBombDistanceToGoal", this._counselorDispatcher);
			}
		}

		private bool TryInitialize()
		{
			ApproximatedPathDistance approximatedPathDistance = Object.FindObjectOfType<ApproximatedPathDistance>();
			if (approximatedPathDistance == null)
			{
				return false;
			}
			BombTargetTrigger[] targets = Object.FindObjectsOfType<BombTargetTrigger>();
			for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
			{
				BotAIGoalManager bitComponent = GameHubBehaviour.Hub.Players.PlayersAndBots[i].CharacterInstance.GetBitComponent<BotAIGoalManager>();
				bitComponent.Initialize();
			}
			this._sensorContext.AddScanner(new BombScanner(this._sensorContext, ServerCounselorController.ScannerParameters.BombDistanceToGoal.ToString(), "RedBombDistanceToGoal", "BlueBombDistanceToGoal", ServerCounselorController.CounselorConditions.CollisionWithBlocker.ToString(), ServerCounselorController.CounselorConditions.IsInOvertime.ToString(), approximatedPathDistance, targets));
			this._sensorContext.AddScanner(new BombAdvanceScanner(this._sensorContext, "RedBombDistanceToGoal", "BlueBombDistanceToGoal", "RedAdvance", "BlueAdvance"));
			this._sensorContext.AddScanner(new MatchScanner(this._sensorContext, ServerCounselorController.CounselorConditions.IsMatchPoint.ToString(), ServerCounselorController.ScannerParameters.RoundNumber.ToString(), ServerCounselorController.ScannerParameters.BombDeliveryDeltaTime.ToString(), ServerCounselorController.ScannerParameters.ArenaIndex.ToString()));
			this._redBombDistanceToGoalId = this._sensorContext.GetHash("RedBombDistanceToGoal");
			this._blueBombDistanceToGoalId = this._sensorContext.GetHash("BlueBombDistanceToGoal");
			return true;
		}

		private void BombManagerOnListenToPhaseChange(BombScoreboardState state)
		{
			if (this._sensorContext != null)
			{
				this._sensorContext.Reset();
			}
			if (state == BombScoreboardState.EndGame || (state == BombScoreboardState.BombDelivery && GameHubBehaviour.Hub.BombManager.Round == 0))
			{
				MatchLogWriter.WriteCounselorBI();
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HMMSensorController));

		[Inject]
		private ICounselorDispatcher _counselorDispatcher;

		private const string redBombDistanceToGoalParameterName = "RedBombDistanceToGoal";

		private const string blueBombDistanceToGoalParameterName = "BlueBombDistanceToGoal";

		private const string RedAdvance = "RedAdvance";

		private const string BlueAdvance = "BlueAdvance";

		private SensorController _sensorContext;

		private ServerCounselorController _counselorController;

		private int _redBombDistanceToGoalId;

		private int _blueBombDistanceToGoalId;
	}
}
