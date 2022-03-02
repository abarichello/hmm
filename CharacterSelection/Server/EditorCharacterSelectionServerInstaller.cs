using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.CharacterSelection.Client;
using HeavyMetalMachines.CharacterSelection.Client.Infra;
using HeavyMetalMachines.CharacterSelection.Client.Presenting;
using HeavyMetalMachines.CharacterSelection.Server.Banning;
using HeavyMetalMachines.CharacterSelection.Server.Bots;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Matches.API;
using Hoplon;
using Infra;
using Standard_Assets.Scripts.HMM.Match;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Server
{
	public class EditorCharacterSelectionServerInstaller : MonoInstaller<EditorCharacterSelectionServerInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.BindInstance<ICharacterSelectionConfigurationsProvider>(new DefaultCharacterSelectionConfigurationProvider());
			base.Container.Bind<IProceedToServerGameState>().To<EditorCharacterSelectionServerInstaller.EditorProceedToServerGameState>().AsTransient();
			base.Container.Bind<ISetMatchPlayersPicks>().To<EditorCharacterSelectionServerInstaller.EditorSetMatchPlayersPicks>().AsTransient();
			base.Container.Bind<IInitializeClientsCommunication>().To<EditorCharacterSelectionServerInstaller.EditorInitializeClientsCommunication>().AsTransient();
			base.Container.Bind<INotifyCharacterSelectionReady>().To<EditorCharacterSelectionServerInstaller.EditorNotifyCharacterSelectionReady>().AsTransient();
			this.Bind<IListenForClientReady, LocalClientReadyCommunication>();
			this.Bind<ISendInitializationData, LocalInitializationDataCommunication>();
			this.Bind<IListenForPickConfirmation, LocalPickConfirmationCommunication>();
			this.Bind<IBroadcastPickConfirmation, LocalOthersPickConfirmationsCommunication>();
			this.Bind<IBroadcastPickStepResult, LocalPickStepResultCommunication>();
			this.Bind<IBroadcastCharacterSelectionStarted, LocalCharacterSelectionStartedCommunication>();
			this.Bind<IBroadcastCharacterSelectionResult, LocalCharacterSelectionResultCommunication>();
			this.Bind<IListenForClientCharacterChoiceChanges, LocalClientCharacterChoiceChangesCommunication>();
			this.Bind<IBroadcastCharacterChoiceChanges, LocalOthersCharacterChoiceChangesCommunication>();
			this.Bind<IListenForBanVoteConfirmation, LocalBanVoteConfirmationCommunication>();
			this.Bind<IBroadcastBanVoteConfirmation, LocalOthersBanConfirmationsCommunication>();
			this.Bind<IBroadcastBanStepResult, LocalBanStepResultCommunication>();
			this.Bind<ISendPickConfirmationRejection, LocalPickConfirmationRejectionCommunication>();
			this.Bind<ISendSkinEquipped, LocalEquipSkinCommunication>();
			this.Bind<IListenForSkinEquips, LocalEquipSkinCommunication>();
			this.Bind<IBroadcastEquipSkinConfirmation, LocalEquipSkinConfirmationCommunication>();
			this.Bind<IListenForEquipSkinConfirmation, LocalEquipSkinConfirmationCommunication>();
			base.Container.Bind<IObserveClientsConnection>().To<EditorCharacterSelectionClientInstaller.ManualObserveClientsConnection>().AsTransient();
			base.Container.Bind<IGetCurrentMatchCharactersAvailability>().To<EditorCharacterSelectionServerInstaller.GetRandomCurrentMatchCharactersAvailability>().AsTransient();
			PreStart.InitializeNativePlugins();
		}

		private void Bind<T1, T2>() where T2 : T1
		{
			base.Container.Bind<T1>().FromMethod((InjectContext context) => (T1)((object)context.Container.Resolve<T2>()));
		}

		private class EditorProceedToServerGameState : IProceedToServerGameState
		{
			public void Proceed()
			{
			}
		}

		private class EditorSetMatchPlayersPicks : ISetMatchPlayersPicks
		{
			public void Set(PlayerMatchPick[] picks)
			{
			}
		}

		private class EditorNotifyCharacterSelectionReady : INotifyCharacterSelectionReady
		{
			public void Notify()
			{
			}
		}

		public class EditorInitializeClientsCommunication : IInitializeClientsCommunication
		{
			public IDisposable Initialize()
			{
				return Disposable.Empty;
			}
		}

		public class GetRandomCurrentMatchCharactersAvailability : IGetCurrentMatchCharactersAvailability
		{
			public GetRandomCurrentMatchCharactersAvailability(IGetCurrentMatch getCurrentMatch, IGetCharacterData getCharacterData, IRandom random)
			{
				this._getCurrentMatch = getCurrentMatch;
				this._getCharacterData = getCharacterData;
				this._random = random;
			}

			public IObservable<CharactersAvailability> Get()
			{
				CharacterData[] all = this._getCharacterData.GetAll();
				CharacterData bannedFromModeCharacter = RandomExtensions.GetRandomItem<CharacterData>(all, this._random);
				IEnumerable<CharacterData> allCharacters = from character in all
				where character.Id != bannedFromModeCharacter.Id
				select character;
				CharactersAvailability charactersAvailability = new CharactersAvailability
				{
					ForPlayers = this.GetPlayersAvailability(allCharacters),
					BannedByModeCharacterIds = new Guid[]
					{
						bannedFromModeCharacter.Id
					}
				};
				return Observable.Return<CharactersAvailability>(charactersAvailability);
			}

			private PlayerCharactersAvailability[] GetPlayersAvailability(IEnumerable<CharacterData> allCharacters)
			{
				Match match = GetCurrentMatchExtensions.Get(this._getCurrentMatch);
				List<PlayerCharactersAvailability> list = new List<PlayerCharactersAvailability>();
				foreach (MatchClient client in MatchExtensions.GetPlayers(match))
				{
					Guid[] ownedCharacterIds = (from character in RandomExtensions.Shuffle<CharacterData>(allCharacters, this._random).Take(12)
					select character.Id).ToArray<Guid>();
					Guid[] availableFromRotationCharacterIds = (from character in RandomExtensions.Shuffle<CharacterData>(allCharacters, this._random).Take(5)
					select character.Id).ToArray<Guid>();
					list.Add(new PlayerCharactersAvailability
					{
						Client = client,
						OwnedCharacterIds = ownedCharacterIds,
						AvailableFromRotationCharacterIds = availableFromRotationCharacterIds
					});
				}
				return list.ToArray();
			}

			private readonly IGetCurrentMatch _getCurrentMatch;

			private readonly IGetCharacterData _getCharacterData;

			private readonly IRandom _random;
		}
	}
}
