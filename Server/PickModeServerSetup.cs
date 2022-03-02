using System;
using System.Collections.Generic;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Characters.PickServiceBehavior;
using HeavyMetalMachines.Characters.PickServiceBehavior.Apis;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matchmaking.Configuration;
using HeavyMetalMachines.Pick;
using HeavyMetalMachines.Regions.Business;
using HeavyMetalMachines.Server.Apis;
using HeavyMetalMachines.Server.Pick;
using HeavyMetalMachines.Server.Pick.Apis;
using Pocketverse;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Server
{
	public class PickModeServerSetup : GameState
	{
		private IPickModeState _currPickState
		{
			get
			{
				return this._pickStates[this._currPickStateIdx].First;
			}
		}

		private IPickServiceBehavior _currPickServiceBehavior
		{
			get
			{
				return this._pickStates[this._currPickStateIdx].Second;
			}
		}

		private void Awake()
		{
			this._hub = GameHubBehaviour.Hub;
			this._pickService = this._hub.Characters;
			this._currentPickConfig = this._pickConfigProvider.Get(this._hub.Match.Kind);
			this._botPick = new BotPickController(this._pickService, this._hub.Players, this._hub.Config, this._hub.BotAIMatchRules, this._hub.InventoryColletion, this._currentPickConfig);
			this.CreatePickStates();
			this.LogCurrentPickState();
			this._pickService.SetPickServiceBehavior(this._currPickServiceBehavior);
		}

		private void CreatePickStates()
		{
			this._pickStates = new List<Tuple2<IPickModeState, IPickServiceBehavior>>
			{
				PickModeServerSetup.CreatePickStateEntry(new PlayerPickState(this._pickService, this._botPick, this._hub.Players, this._currentPickConfig.BotPickTimeAllowedSeconds), new PlayerPickPickServiceBehavior(this._pickService, this._hub.Players)),
				PickModeServerSetup.CreatePickStateEntry(new BotPickState(this._botPick), new BotPickPickServiceBehavior(this._pickService, this._hub.Players, this._hub.AddressGroups)),
				PickModeServerSetup.CreatePickStateEntry(new GridSelectionPickState(this._pickService, this._hub.Players, this._hub.AddressGroups), new GridSelectionPickServiceBehavior(this._pickService, this._hub.Players)),
				PickModeServerSetup.CreatePickStateEntry(new CustomizationPickState(this._pickService), new CustomizationPickServiceBehavior(this._pickService, this._hub.Players))
			};
			this._currPickStateIdx = 0;
		}

		private static Tuple2<IPickModeState, IPickServiceBehavior> CreatePickStateEntry(IPickModeState state, IPickServiceBehavior behavior)
		{
			return new Tuple2<IPickModeState, IPickServiceBehavior>(state, behavior);
		}

		protected override void OnStateEnabled()
		{
			PickModeServerSetup.Log.Debug("Pick Mode Server State enabled.");
			this.SetupPickService();
			this._botPick.Initialize();
			this.UpdateMatchState(MatchData.MatchState.CharacterPick);
		}

		protected override IObservable<Unit> OnStateEnabledAsync()
		{
			if (GameHubBehaviour.Hub.Match.Kind != 3)
			{
				return Observable.ReturnUnit();
			}
			return Observable.AsUnitObservable<QueueConfiguration>(Observable.Do<QueueConfiguration>(this._diContainer.Resolve<IGetCompetitiveQueueConfiguration>().GetForRegion(this._getServerRegion.GetRegionName()), delegate(QueueConfiguration config)
			{
				this._pickService.CompetitiveQueueConfiguration = config;
			}));
		}

		private void SetupPickService()
		{
			this._pickService.PickTime = this._currentPickConfig.PickTime;
			this._pickService.CustomizationTime = ((GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.FastTestChar) == -1) ? this._currentPickConfig.CustomizationTime : 0f);
			this._pickService.InitPickMode();
		}

		private void Update()
		{
			if (!this._currPickState.IsInitialized)
			{
				this._currPickState.Initialize();
				return;
			}
			if (!this._currPickState.Update())
			{
				return;
			}
			this.LoadNextPickState();
		}

		private void LoadNextPickState()
		{
			this._currPickStateIdx++;
			if (this._currPickStateIdx >= this._pickStates.Count)
			{
				PickModeServerSetup.Log.Debug("Pick Mode has finished executing all its states.");
				this.FinalizePickMode();
				return;
			}
			this._pickService.SetPickServiceBehavior(this._currPickServiceBehavior);
			this.LogCurrentPickState();
		}

		private void FinalizePickMode()
		{
			this.LogPlayersByAddress();
			if (this._hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this.GotoNextState();
				return;
			}
			this._hub.Players.ReloadPlayerCustomizations(new Action(this.GotoNextState));
			base.enabled = false;
		}

		private void GotoNextState()
		{
			PickModeServerSetup.Log.Debug("Pick Mode has finished. Go to Loading state.");
			this.UpdateMatchState(MatchData.MatchState.PreMatch);
			this.LogMatchPlayers();
			base.GoToState(this.loadingState, false);
		}

		private void UpdateMatchState(MatchData.MatchState newState)
		{
			this._hub.Match.State = newState;
			this._hub.Server.SpreadInfo();
			this._playersDispatcher.UpdatePlayers();
			this._teamsDispatcher.UpdateTeams();
		}

		private void LogMatchPlayers()
		{
			List<PlayerData> playersAndBots = this._hub.Players.PlayersAndBots;
			for (int i = 0; i < playersAndBots.Count; i++)
			{
				PlayerData playerData = playersAndBots[i];
				string characterBiName = playerData.GetCharacterBiName();
				MatchLogWriter.CharacterSelected(playerData.UserId, characterBiName);
				PickModeServerSetup.Log.InfoFormat("Player Name={0} ObjId={1} Character={2} SFId={3} UserId={4}", new object[]
				{
					playerData.Name,
					playerData.PlayerCarId,
					characterBiName,
					playerData.PlayerId,
					playerData.UserId
				});
			}
		}

		private void LogPlayersByAddress()
		{
			for (int i = 0; i < this._hub.Players.AllDatas.Count; i++)
			{
				PlayerData playerData = this._hub.Players.AllDatas[i];
				PickModeServerSetup.Log.DebugFormat("Player={0} Char={1}", new object[]
				{
					playerData.PlayerAddress,
					playerData.CharacterId
				});
			}
		}

		private void LogCurrentPickState()
		{
			PickModeServerSetup.Log.InfoFormat("Pick Mode is currently processing {0} with {1}.", new object[]
			{
				this._currPickState.GetType().Name,
				this._currPickServiceBehavior.GetType().Name
			});
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PickModeServerSetup));

		[Inject]
		private DiContainer _diContainer;

		[Inject]
		private IMatchTeamsDispatcher _teamsDispatcher;

		[Inject]
		private IMatchPlayersDispatcher _playersDispatcher;

		[Inject]
		private IGetServerRegion _getServerRegion;

		[Inject]
		private IPickModeConfigProvider _pickConfigProvider;

		public LoadingState loadingState;

		private HMMHub _hub;

		private IBotPickController _botPick;

		private CharacterService _pickService;

		private int _currPickStateIdx;

		private List<Tuple2<IPickModeState, IPickServiceBehavior>> _pickStates;

		private MatchPickModeConfig _currentPickConfig;
	}
}
