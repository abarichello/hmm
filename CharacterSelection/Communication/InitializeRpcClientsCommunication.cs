using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting;
using HeavyMetalMachines.DependencyInjection;
using HeavyMetalMachines.Infra.GameObjects;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class InitializeRpcClientsCommunication : IInitializeClientsCommunication
	{
		public InitializeRpcClientsCommunication(CharacterSelectionRpcStorage rpcStorage, CharacterSelectionRpcPrefabs rpcPrefabs, IInjectionInstantiator injectionInstantiator)
		{
			this._rpcStorage = rpcStorage;
			this._rpcPrefabs = rpcPrefabs;
			this._injectionInstantiator = injectionInstantiator;
		}

		public IDisposable Initialize()
		{
			CharacterSelectionRpcPrefabs instantiatedRpc = this._injectionInstantiator.Instantiate<CharacterSelectionRpcPrefabs>(this._rpcPrefabs);
			instantiatedRpc.Identifiable.Register(ObjectId.New(7, 0));
			this._rpcStorage.ClientReady = instantiatedRpc.ClientReadyRpc;
			this._rpcStorage.Initialization = instantiatedRpc.InitializationRpc;
			this._rpcStorage.BanConfirmation = instantiatedRpc.BanConfirmation;
			this._rpcStorage.OthersBanVoteConfirmation = instantiatedRpc.OthersBanConfirmation;
			this._rpcStorage.BanStepResult = instantiatedRpc.BanStepResult;
			this._rpcStorage.PickConfirmation = instantiatedRpc.PickConfirmation;
			this._rpcStorage.OthersPickConfirmation = instantiatedRpc.OthersPickConfirmation;
			this._rpcStorage.PickStepResult = instantiatedRpc.PickStepResult;
			this._rpcStorage.Start = instantiatedRpc.Start;
			this._rpcStorage.Result = instantiatedRpc.Result;
			this._rpcStorage.CharacterChoiceChangesClientToServer = instantiatedRpc.CharacterChoiceChangesClientToServer;
			this._rpcStorage.CharacterChoiceChangesServerToClients = instantiatedRpc.CharacterChoiceChangesServerToClients;
			this._rpcStorage.PickConfirmationRejection = instantiatedRpc.PickConfirmationRejection;
			this._rpcStorage.ClientsConnection = instantiatedRpc.ClientsConnection;
			this._rpcStorage.SkinEquipped = instantiatedRpc.EquipSkin;
			this._rpcStorage.EquippedSkinsConfirmations = instantiatedRpc.EquipSkinConfirmations;
			return Disposable.Create(delegate()
			{
				instantiatedRpc.DestroySafe();
			});
		}

		private readonly CharacterSelectionRpcStorage _rpcStorage;

		private readonly CharacterSelectionRpcPrefabs _rpcPrefabs;

		private readonly IInjectionInstantiator _injectionInstantiator;
	}
}
