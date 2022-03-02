using System;
using HeavyMetalMachines.CharacterSelection.Banning;
using HeavyMetalMachines.CharacterSelection.Client.Infra;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
using HeavyMetalMachines.CharacterSelection.Communication;
using HeavyMetalMachines.CharacterSelection.Picking;
using HeavyMetalMachines.CharacterSelection.Skins;
using HeavyMetalMachines.Matches;
using UniRx;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class CharacterSelectionClientRpcProxy : IListenForInitializationData, ISendLocalPlayerReady, IListenForCharacterSelectionToStart, IListenForPickConfirmations, ISendBanVoteConfirmation, IListenForBanVoteConfirmations, IListenForBanStepResult, ISendPickConfirmation, IListenForPickStepResult, IListenForCharacterSelectionResult, ISendCharacterChoiceChanges, IListenForCharacterChoiceChanges, IListenForPickConfirmationRejection, IListenForClientsConnection, ISendSkinEquipped, IListenForEquipSkinConfirmation
	{
		public CharacterSelectionClientRpcProxy(CharacterSelectionRpcStorage rpcStorage)
		{
			this._rpcStorage = rpcStorage;
		}

		public IObservable<CharacterSelectionInitializationData> Listen(MatchClient localClient)
		{
			return this._rpcStorage.Initialization.Listen(localClient);
		}

		public void Send(MatchClient matchClient)
		{
			this._rpcStorage.ClientReady.Send(matchClient);
		}

		IObservable<Unit> IListenForCharacterSelectionToStart.Listen(MatchClient localClient)
		{
			return this._rpcStorage.Start.Listen(localClient);
		}

		IObservable<PickConfirmation> IListenForPickConfirmations.Listen(MatchClient localClient)
		{
			return this._rpcStorage.OthersPickConfirmation.Listen(localClient);
		}

		public void Send(ClientPickConfirmation pickConfirmation)
		{
			this._rpcStorage.PickConfirmation.Send(pickConfirmation);
		}

		IObservable<PickStepResult> IListenForPickStepResult.Listen(MatchClient localClient)
		{
			return this._rpcStorage.PickStepResult.Listen(localClient);
		}

		IObservable<CharacterSelectionResult> IListenForCharacterSelectionResult.Listen(MatchClient localClient)
		{
			return this._rpcStorage.Result.Listen(localClient);
		}

		public void Send(ClientCharacterChoice characterChoice)
		{
			this._rpcStorage.CharacterChoiceChangesClientToServer.Send(characterChoice);
		}

		IObservable<CharacterChoice> IListenForCharacterChoiceChanges.Listen(MatchClient localClient)
		{
			return this._rpcStorage.CharacterChoiceChangesServerToClients.Listen(localClient);
		}

		IObservable<PickConfirmationRejectionReason> IListenForPickConfirmationRejection.Listen(MatchClient localClient)
		{
			return this._rpcStorage.PickConfirmationRejection.Listen(localClient);
		}

		public void Send(ClientVoteBanConfirmation confirmation)
		{
			this._rpcStorage.BanConfirmation.Send(confirmation);
		}

		IObservable<ServerBanVoteConfirmation> IListenForBanVoteConfirmations.Listen(MatchClient localClient)
		{
			return this._rpcStorage.OthersBanVoteConfirmation.Listen(localClient);
		}

		IObservable<BanStepResult> IListenForBanStepResult.Listen(MatchClient localClient)
		{
			return this._rpcStorage.BanStepResult.Listen(localClient);
		}

		public IObservable<MatchClient> OnClientDisconnected(MatchClient localClient)
		{
			return this._rpcStorage.ClientsConnection.OnClientDisconnected(localClient);
		}

		public IObservable<MatchClient> OnClientReconnected(MatchClient localClient)
		{
			return this._rpcStorage.ClientsConnection.OnClientReconnected(localClient);
		}

		public void Send(EquipSkinRequest clientPickConfirmation)
		{
			this._rpcStorage.SkinEquipped.Send(clientPickConfirmation);
		}

		public IObservable<EquipSkinConfirmation> ListenForEquipSkinConfirmations()
		{
			return this._rpcStorage.EquippedSkinsConfirmations.ListenForEquipSkinConfirmations();
		}

		private readonly CharacterSelectionRpcStorage _rpcStorage;
	}
}
