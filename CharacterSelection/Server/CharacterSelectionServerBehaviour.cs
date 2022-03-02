using System;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Client.Presenting;
using HeavyMetalMachines.CharacterSelection.Server.API;
using HeavyMetalMachines.CharacterSelection.Server.Bots;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.GameServer;
using Hoplon.Logging;
using Standard_Assets.Scripts.HMM.Match;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Server
{
	public class CharacterSelectionServerBehaviour : MonoBehaviour
	{
		private void Start()
		{
			IDisposable communicationDisposable = this._initializeClientsCommunication.Initialize();
			ObservableExtensions.Subscribe<Unit>(Observable.Finally<Unit>(Observable.DoOnError<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.DoOnCompleted<Unit>(this.Initialize(), delegate()
			{
				this._notifyCharacterSelectionReady.Notify();
			}), this.ExecuteCharacterSelection()), delegate(Exception exception)
			{
				this._logger.Error(exception);
			}), delegate()
			{
				communicationDisposable.Dispose();
			}));
		}

		private IObservable<Unit> Initialize()
		{
			GameServerStartRequest gameServerStartRequest = this._gameServerStartRequestStorage.Get();
			if (gameServerStartRequest.DrafterConfigurationId != null)
			{
				return this._initializeCharacterSelectionConfiguration.InitializeWithConfigurationId(gameServerStartRequest.DrafterConfigurationId.Value);
			}
			return this._initializeCharacterSelectionConfiguration.Initialize();
		}

		private IObservable<Unit> ExecuteCharacterSelection()
		{
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				Observable.AsUnitObservable<CharacterSelectionResult>(Observable.Do<CharacterSelectionResult>(this._executeCharacterSelection.Execute(), new Action<CharacterSelectionResult>(this.ProceedToGameState))),
				this._executeBotsCharacterSelection.Execute(new CharacterSelectionBotsConfiguration())
			});
		}

		private void ProceedToGameState(CharacterSelectionResult result)
		{
			PlayerMatchPick[] picks = (from pick in result.Picks
			select new PlayerMatchPick
			{
				Client = pick.Client,
				CharacterId = pick.CharacterId,
				SkinId = pick.SkinId
			}).ToArray<PlayerMatchPick>();
			this._setMatchPlayersPicks.Set(picks);
			this._proceedToServerGameState.Proceed();
		}

		[Inject]
		private IInitializeCharacterSelectionConfiguration _initializeCharacterSelectionConfiguration;

		[Inject]
		private IInitializeClientsCommunication _initializeClientsCommunication;

		[Inject]
		private IExecuteCharacterSelection _executeCharacterSelection;

		[Inject]
		private INotifyCharacterSelectionReady _notifyCharacterSelectionReady;

		[Inject]
		private ILogger<CharacterSelectionServerBehaviour> _logger;

		[Inject]
		private IExecuteBotsCharacterSelection _executeBotsCharacterSelection;

		[Inject]
		private ISetMatchPlayersPicks _setMatchPlayersPicks;

		[Inject]
		private IProceedToServerGameState _proceedToServerGameState;

		[Inject]
		private IGameServerStartRequestStorage _gameServerStartRequestStorage;
	}
}
