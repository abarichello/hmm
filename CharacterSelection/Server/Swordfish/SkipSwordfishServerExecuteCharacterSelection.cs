using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets.ClientApiObjects;
using HeavyMetalMachines.CharacterSelection.Server.API;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.Configuring.Instances;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using Hoplon.Logging;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.CharacterSelection.Server.Swordfish
{
	public class SkipSwordfishServerExecuteCharacterSelection : IExecuteCharacterSelection
	{
		public SkipSwordfishServerExecuteCharacterSelection(IConfigLoader configLoader, IMatchPlayers players, IGetSkinsFromCharacter getSkinsFromCharacter, ICollectionScriptableObject collectionScriptableObject, IWaitForAllClientsToBeReady waitForAllClientsToBeReady, IBroadcastCharacterSelectionResult broadcastCharacterSelectionResult, ILogger<SkipSwordfishServerExecuteCharacterSelection> logger)
		{
			this._configLoader = configLoader;
			this._players = players;
			this._getSkinsFromCharacter = getSkinsFromCharacter;
			this._collectionScriptableObject = collectionScriptableObject;
			this._waitForAllClientsToBeReady = waitForAllClientsToBeReady;
			this._broadcastCharacterSelectionResult = broadcastCharacterSelectionResult;
			this._logger = logger;
		}

		public IObservable<CharacterSelectionResult> Execute()
		{
			CharacterSelectionResult result = this.CreateCharacterSelectionResult();
			return Observable.Select<Unit, CharacterSelectionResult>(Observable.Do<Unit>(this._waitForAllClientsToBeReady.Wait(), delegate(Unit _)
			{
				this.BroadcastResult(result);
			}), (Unit _) => result);
		}

		private CharacterSelectionResult CreateCharacterSelectionResult()
		{
			CharacterSelectionResult characterSelectionResult = new CharacterSelectionResult();
			List<CharacterSelectionClientPick> list = new List<CharacterSelectionClientPick>();
			foreach (PlayerData playerData in this._players.PlayersAndBots)
			{
				Guid characterId = this.GetCharacterId(playerData);
				list.Add(new CharacterSelectionClientPick
				{
					Client = playerData.ToMatchClient(),
					CharacterId = characterId,
					SkinId = this.GetSkinId(playerData, characterId)
				});
			}
			characterSelectionResult.Picks = list.ToArray();
			return characterSelectionResult;
		}

		private void BroadcastResult(CharacterSelectionResult result)
		{
			IEnumerable<PlayerData> players = this._players.Players;
			if (SkipSwordfishServerExecuteCharacterSelection.<>f__mg$cache0 == null)
			{
				SkipSwordfishServerExecuteCharacterSelection.<>f__mg$cache0 = new Func<PlayerData, MatchClient>(PlayerDataExtension.ToMatchClient);
			}
			MatchClient[] array = players.Select(SkipSwordfishServerExecuteCharacterSelection.<>f__mg$cache0).ToArray<MatchClient>();
			this._broadcastCharacterSelectionResult.Broadcast(result, array);
		}

		private Guid GetSkinId(PlayerData playerData, Guid characterId)
		{
			ConfigInstance inst;
			if (playerData.Team.GetMatchTeam() == null)
			{
				inst = ConfigAccess.Team1Skins[playerData.TeamSlot];
			}
			else
			{
				inst = ConfigAccess.Team2Skins[playerData.TeamSlot];
			}
			string value = this._configLoader.GetValue(inst);
			IEnumerable<IItemType> enumerable = from skinId in this._getSkinsFromCharacter.GetSkins(characterId)
			select this._collectionScriptableObject.Get(skinId);
			foreach (IItemType itemType in enumerable)
			{
				if (string.CompareOrdinal(value, itemType.Name) == 0)
				{
					return itemType.Id;
				}
			}
			this._logger.InfoFormat("GetSkinId {0} {1} {2} skin {3}", new object[]
			{
				playerData.TeamSlot,
				playerData.Team,
				playerData.Name,
				value
			});
			return this._collectionScriptableObject.GetDefaultSkin(characterId).Id;
		}

		private Guid GetCharacterId(PlayerData playerData)
		{
			ConfigInstance inst;
			if (playerData.Team.GetMatchTeam() == null)
			{
				inst = ConfigAccess.Team1Characters[playerData.TeamSlot];
			}
			else
			{
				inst = ConfigAccess.Team2Characters[playerData.TeamSlot];
			}
			int intValue = this._configLoader.GetIntValue(inst);
			this._logger.InfoFormat("GetCharacterId {0} {1} {2} char {3}", new object[]
			{
				playerData.TeamSlot,
				playerData.Team,
				playerData.Name,
				intValue
			});
			return this._collectionScriptableObject.GetCharacterGuidId(intValue);
		}

		private readonly IConfigLoader _configLoader;

		private readonly IMatchPlayers _players;

		private readonly IGetSkinsFromCharacter _getSkinsFromCharacter;

		private readonly ICollectionScriptableObject _collectionScriptableObject;

		private readonly IWaitForAllClientsToBeReady _waitForAllClientsToBeReady;

		private readonly IBroadcastCharacterSelectionResult _broadcastCharacterSelectionResult;

		private readonly ILogger<SkipSwordfishServerExecuteCharacterSelection> _logger;

		[CompilerGenerated]
		private static Func<PlayerData, MatchClient> <>f__mg$cache0;
	}
}
