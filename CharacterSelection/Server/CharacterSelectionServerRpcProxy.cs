using System;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Banning;
using HeavyMetalMachines.CharacterSelection.Client.Infra;
using HeavyMetalMachines.CharacterSelection.Communication;
using HeavyMetalMachines.CharacterSelection.Picking;
using HeavyMetalMachines.CharacterSelection.Server.Banning;
using HeavyMetalMachines.CharacterSelection.Server.Bots;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.CharacterSelection.Skins;
using HeavyMetalMachines.Matches;
using Hoplon.DependencyInjection;
using UniRx;

namespace HeavyMetalMachines.CharacterSelection.Server
{
	public class CharacterSelectionServerRpcProxy : IListenForClientReady, ISendInitializationData, IListenForBanVoteConfirmation, IBroadcastBanVoteConfirmation, IBroadcastBanStepResult, IListenForPickConfirmation, IBroadcastPickConfirmation, IBroadcastPickStepResult, IBroadcastCharacterSelectionStarted, IBroadcastCharacterSelectionResult, IListenForClientCharacterChoiceChanges, IBroadcastCharacterChoiceChanges, ISendPickConfirmationRejection, IBroadcastClientsConnection, IListenForSkinEquips, IBroadcastEquipSkinConfirmation, IListenForEquipSkinConfirmation
	{
		public CharacterSelectionServerRpcProxy(CharacterSelectionRpcStorage rpcStorage, IInjectionResolver injectionResolver)
		{
			this._rpcStorage = rpcStorage;
			this._injectionResolver = injectionResolver;
		}

		public IObservable<MatchClient> ListenForClientsReady()
		{
			LocalClientReadyCommunication localClientReadyCommunication = this._injectionResolver.Resolve<LocalClientReadyCommunication>();
			return Observable.Merge<MatchClient>(this._rpcStorage.ClientReady.ListenForClientsReady(), new IObservable<MatchClient>[]
			{
				localClientReadyCommunication.ListenForClientsReady()
			});
		}

		public void Send(CharacterSelectionInitializationData data, MatchClient client)
		{
			if (client.IsBot)
			{
				this._injectionResolver.Resolve<LocalInitializationDataCommunication>().Send(data, client);
				return;
			}
			this._rpcStorage.Initialization.Send(data, client);
		}

		public IObservable<ClientPickConfirmation> ListenForPickConfirmations()
		{
			LocalPickConfirmationCommunication localPickConfirmationCommunication = this._injectionResolver.Resolve<LocalPickConfirmationCommunication>();
			return Observable.Merge<ClientPickConfirmation>(this._rpcStorage.PickConfirmation.ListenForPickConfirmations(), new IObservable<ClientPickConfirmation>[]
			{
				localPickConfirmationCommunication.ListenForPickConfirmations()
			});
		}

		public void Broadcast(PickConfirmation pickConfirmation, MatchClient[] clients)
		{
			this._injectionResolver.Resolve<LocalOthersPickConfirmationsCommunication>().Broadcast(pickConfirmation, this.GetBots(clients));
			this._rpcStorage.OthersPickConfirmation.Broadcast(pickConfirmation, this.GetHumans(clients));
		}

		public void Broadcast(PickStepResult pickStepResult, MatchClient[] clients)
		{
			this._injectionResolver.Resolve<LocalPickStepResultCommunication>().Broadcast(pickStepResult, this.GetBots(clients));
			this._rpcStorage.PickStepResult.Broadcast(pickStepResult, this.GetHumans(clients));
		}

		public void Broadcast(MatchClient[] clients)
		{
			this._injectionResolver.Resolve<LocalCharacterSelectionStartedCommunication>().Broadcast(this.GetBots(clients));
			MatchClient[] humans = this.GetHumans(clients);
			this._rpcStorage.Start.Broadcast(humans);
		}

		public void Broadcast(CharacterSelectionResult result, MatchClient[] clients)
		{
			this._injectionResolver.Resolve<LocalCharacterSelectionResultCommunication>().Broadcast(result, this.GetBots(clients));
			this._rpcStorage.Result.Broadcast(result, this.GetHumans(clients));
		}

		public IObservable<ClientCharacterChoice> ListenForClientCharacterChoiceChanges()
		{
			LocalClientCharacterChoiceChangesCommunication localClientCharacterChoiceChangesCommunication = this._injectionResolver.Resolve<LocalClientCharacterChoiceChangesCommunication>();
			return Observable.Merge<ClientCharacterChoice>(this._rpcStorage.CharacterChoiceChangesClientToServer.ListenForClientCharacterChoiceChanges(), new IObservable<ClientCharacterChoice>[]
			{
				localClientCharacterChoiceChangesCommunication.ListenForClientCharacterChoiceChanges()
			});
		}

		public void Broadcast(CharacterChoice characterChoice, MatchClient[] clients)
		{
			this._rpcStorage.CharacterChoiceChangesServerToClients.Broadcast(characterChoice, this.GetHumans(clients));
			this._injectionResolver.Resolve<LocalOthersCharacterChoiceChangesCommunication>().Broadcast(characterChoice, this.GetBots(clients));
		}

		private MatchClient[] GetBots(MatchClient[] clients)
		{
			return (from client in clients
			where client.IsBot
			select client).ToArray<MatchClient>();
		}

		private MatchClient[] GetHumans(MatchClient[] clients)
		{
			return (from client in clients
			where !client.IsBot
			select client).ToArray<MatchClient>();
		}

		public void Send(MatchClient client, PickConfirmationRejectionReason reason)
		{
			if (client.IsBot)
			{
				this._injectionResolver.Resolve<LocalPickConfirmationRejectionCommunication>().Send(client, reason);
				return;
			}
			this._rpcStorage.PickConfirmationRejection.Send(client, reason);
		}

		public IObservable<ClientVoteBanConfirmation> ListenForBanVoteConfirmations()
		{
			LocalBanVoteConfirmationCommunication localBanVoteConfirmationCommunication = this._injectionResolver.Resolve<LocalBanVoteConfirmationCommunication>();
			return Observable.Merge<ClientVoteBanConfirmation>(this._rpcStorage.BanConfirmation.ListenForBanVoteConfirmations(), new IObservable<ClientVoteBanConfirmation>[]
			{
				localBanVoteConfirmationCommunication.ListenForBanVoteConfirmations()
			});
		}

		public void Broadcast(ServerBanVoteConfirmation voter, MatchClient[] receivers)
		{
			this._rpcStorage.OthersBanVoteConfirmation.Broadcast(voter, receivers);
		}

		public void Broadcast(BanStepResult banStepResult, MatchClient[] clients)
		{
			this._injectionResolver.Resolve<LocalBanStepResultCommunication>().Broadcast(banStepResult, this.GetBots(clients));
			this._rpcStorage.BanStepResult.Broadcast(banStepResult, this.GetHumans(clients));
		}

		public void BroadcastClientDisconnected(MatchClient disconnectedClient, MatchClient[] recipientClients)
		{
			this._injectionResolver.Resolve<LocalClientsConnectionCommunication>().BroadcastClientDisconnected(disconnectedClient, this.GetBots(recipientClients));
			this._rpcStorage.ClientsConnection.BroadcastClientDisconnected(disconnectedClient, this.GetHumans(recipientClients));
		}

		public void BroadcastClientReconnected(MatchClient reconnectedClient, MatchClient[] recipientClients)
		{
			this._injectionResolver.Resolve<LocalClientsConnectionCommunication>().BroadcastClientReconnected(reconnectedClient, this.GetBots(recipientClients));
			this._rpcStorage.ClientsConnection.BroadcastClientDisconnected(reconnectedClient, this.GetHumans(recipientClients));
		}

		public IObservable<EquipSkinRequest> ListenForEquipSkinRequests()
		{
			return this._rpcStorage.SkinEquipped.ListenForEquipSkinRequests();
		}

		public void Broadcast(EquipSkinConfirmation equipSkinConfirmation, MatchClient receiver)
		{
			this._rpcStorage.EquippedSkinsConfirmations.Broadcast(equipSkinConfirmation, receiver);
		}

		public IObservable<EquipSkinConfirmation> ListenForEquipSkinConfirmations()
		{
			return this._rpcStorage.EquippedSkinsConfirmations.ListenForEquipSkinConfirmations();
		}

		private readonly CharacterSelectionRpcStorage _rpcStorage;

		private readonly IInjectionResolver _injectionResolver;
	}
}
