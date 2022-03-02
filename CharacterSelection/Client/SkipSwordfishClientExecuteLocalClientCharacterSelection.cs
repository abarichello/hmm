using System;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Client.API;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
using HeavyMetalMachines.CharacterSelection.Client.Presenting;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.API;
using HeavyMetalMachines.Login;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Players.Business;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class SkipSwordfishClientExecuteLocalClientCharacterSelection : IExecuteLocalClientCharacterSelection
	{
		public SkipSwordfishClientExecuteLocalClientCharacterSelection(IInitializeClientsCommunication initializeClientsCommunication, IGetLocalPlayer getLocalPlayer, IGetCurrentMatch getCurrentMatch, IListenForCharacterSelectionResult listenForCharacterSelectionResult, LocalCharacterSelectionLifetimeNotifications lifetimeNotifications, IProceedToClientGameState proceedToClientGameState, IEndSession endSession, ISendLocalPlayerReady sendLocalPlayerReady, ILogger<SkipSwordfishClientExecuteLocalClientCharacterSelection> logger)
		{
			this._initializeClientsCommunication = initializeClientsCommunication;
			this._getLocalPlayer = getLocalPlayer;
			this._getCurrentMatch = getCurrentMatch;
			this._listenForCharacterSelectionResult = listenForCharacterSelectionResult;
			this._lifetimeNotifications = lifetimeNotifications;
			this._proceedToClientGameState = proceedToClientGameState;
			this._endSession = endSession;
			this._sendLocalPlayerReady = sendLocalPlayerReady;
			this._logger = logger;
		}

		public IObservable<Unit> Execute()
		{
			return Observable.Defer<Unit>(delegate()
			{
				IDisposable communicationsDisposable = this._initializeClientsCommunication.Initialize();
				return Observable.AsUnitObservable<Unit>(Observable.Catch<Unit, Exception>(Observable.ContinueWith<Unit, Unit>(Observable.Finally<Unit>(Observable.ContinueWith<ClientCharacterSelection, Unit>(Observable.Do<ClientCharacterSelection>(Observable.Return<ClientCharacterSelection>(this.CreateExecution()), delegate(ClientCharacterSelection c)
				{
					this._sendLocalPlayerReady.Send(c.LocalClient);
				}), new Func<ClientCharacterSelection, IObservable<Unit>>(this.WaitForServerResult)), delegate()
				{
					communicationsDisposable.Dispose();
				}), (Unit _) => this._proceedToClientGameState.Proceed()), new Func<Exception, IObservable<Unit>>(this.EndSession)));
			});
		}

		private IObservable<Unit> EndSession(Exception exception)
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._logger.ErrorFormat("An error occurred on the client character selection. {0}", new object[]
				{
					exception
				});
				this._endSession.End("CharacterSelection unexpected error.");
			});
		}

		private ClientCharacterSelection CreateExecution()
		{
			IPlayer localPlayer = this._getLocalPlayer.Get();
			MatchClient localClient = GetCurrentMatchExtensions.Get(this._getCurrentMatch).Clients.First((MatchClient client) => client.PlayerId == localPlayer.PlayerId && !client.IsBot);
			return this.Execute(localClient);
		}

		public ClientCharacterSelection Execute(MatchClient localClient)
		{
			CharacterSelectionContext characterSelectionContext = this.CreateContext(localClient);
			ClientCharacterSelection clientCharacterSelection = new ClientCharacterSelection
			{
				LocalClient = localClient,
				Notifications = characterSelectionContext.Notifications,
				ExecuteOperation = this.ListenForResult(characterSelectionContext)
			};
			characterSelectionContext.CharacterSelection = clientCharacterSelection;
			return clientCharacterSelection;
		}

		private CharacterSelectionContext CreateContext(MatchClient localClient)
		{
			return new CharacterSelectionContext
			{
				LocalClient = localClient,
				CharacterConfirmation = Observable.Empty<Guid>(),
				CharacterChoiceChanges = Observable.Empty<Guid>(),
				BanVoteConfirmations = Observable.Empty<Guid>(),
				SkinEquipped = Observable.Empty<Guid>(),
				Notifications = new CharacterSelectionNotifications(),
				State = null,
				Configuration = null
			};
		}

		private IObservable<Unit> WaitForServerResult(ClientCharacterSelection characterSelection)
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._lifetimeNotifications.NotifyStarted();
				return characterSelection.ExecuteOperation;
			});
		}

		private IObservable<Unit> ListenForResult(CharacterSelectionContext context)
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (context.Result != null)
				{
					return Observable.Empty<Unit>();
				}
				return Observable.AsUnitObservable<CharacterSelectionResult>(Observable.DoOnCompleted<CharacterSelectionResult>(Observable.Do<CharacterSelectionResult>(Observable.First<CharacterSelectionResult>(this._listenForCharacterSelectionResult.Listen(context.LocalClient)), delegate(CharacterSelectionResult result)
				{
					context.Result = result;
				}), delegate()
				{
					this._logger.Info("Received character selection result from server.");
				}));
			});
		}

		private readonly IInitializeClientsCommunication _initializeClientsCommunication;

		private readonly IGetLocalPlayer _getLocalPlayer;

		private readonly IGetCurrentMatch _getCurrentMatch;

		private readonly IListenForCharacterSelectionResult _listenForCharacterSelectionResult;

		private readonly LocalCharacterSelectionLifetimeNotifications _lifetimeNotifications;

		private readonly IProceedToClientGameState _proceedToClientGameState;

		private readonly IEndSession _endSession;

		private readonly ISendLocalPlayerReady _sendLocalPlayerReady;

		private readonly ILogger<SkipSwordfishClientExecuteLocalClientCharacterSelection> _logger;
	}
}
