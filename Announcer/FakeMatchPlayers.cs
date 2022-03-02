using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Components.Testable;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Match;
using UnityEngine;

namespace HeavyMetalMachines.Announcer
{
	public class FakeMatchPlayers : IMatchPlayers
	{
		public FakeMatchPlayers()
		{
			this.PlayersAndBots = new List<PlayerData>();
			this.players = new List<PlayerData>();
			this.Bots = new List<PlayerData>();
			this.Narrators = new List<PlayerData>();
			this.BotData = new Dictionary<byte, int>();
		}

		public List<PlayerData> Players
		{
			get
			{
				return this.players;
			}
		}

		public List<PlayerData> Bots { get; private set; }

		public List<PlayerData> Narrators { get; private set; }

		public List<PlayerData> PlayersAndBots { get; private set; }

		public PlayerData GetPlayerByObjectId(int playerId)
		{
			throw new NotImplementedException();
		}

		public PlayerData GetPlayerByAddress(byte address)
		{
			throw new NotImplementedException();
		}

		public PlayerData GetPlayerOrBotsByObjectId(int playerId)
		{
			return this.players[playerId];
		}

		public PlayerData GetAnyByPlayerId(long playerId)
		{
			throw new NotImplementedException();
		}

		public PlayerData CurrentPlayerData { get; private set; }

		public TeamKind GetTeamKindById(int playerId)
		{
			return this.Players[playerId].Team;
		}

		public int RedMMR { get; private set; }

		public int BlueMMR { get; private set; }

		public List<PlayerData> BlueTeamPlayersAndBots { get; private set; }

		public List<PlayerData> RedTeamPlayersAndBots { get; private set; }

		public Dictionary<byte, int> BotData { get; private set; }

		public bool IsTeamBotOnly(List<PlayerData> playerDatas)
		{
			throw new NotImplementedException();
		}

		public void UpdatePlayers()
		{
			throw new NotImplementedException();
		}

		public List<PlayerData> GetPlayersAndBotsByTeam(TeamKind team)
		{
			return this.PlayersAndBots.FindAll((PlayerData playerData) => playerData.Team == team);
		}

		public void AddMatchMember(PlayerData player)
		{
			this.PlayersAndBots.Add(player);
			if (player.IsBot)
			{
				this.Bots.Add(player);
			}
			else
			{
				this.players.Add(player);
			}
		}

		public PlayerData GetBot(int idx)
		{
			return this.Bots[idx];
		}

		public PlayerData GetPlayer(int idx)
		{
			return this.players[idx];
		}

		public void CreateBot(TeamKind team, int count = 1, int desiredCharacterId = -1)
		{
			for (int i = 0; i < count; i++)
			{
				PlayerData playerData = new PlayerData(string.Empty, 0, team, this.addressIdCounter, 0, true, i, false, null);
				this.BotData[playerData.PlayerAddress] = desiredCharacterId;
				this.AddMatchMember(playerData);
				this.addressIdCounter += 1;
			}
		}

		public void FakeInit()
		{
			this.SetupMockInventoryCollection(null);
			this.CurrentPlayerData = this.CreatePlayer("Filipe", TeamKind.Blue, -1);
			this.CreatePlayers(TeamKind.Blue, 3, -1);
			this.CreatePlayers(TeamKind.Red, 4, -1);
		}

		public PlayerData CreatePlayer(string name, TeamKind team, int characterId = -1)
		{
			PlayerData playerData = new PlayerData(string.Empty, 0, team, this.addressIdCounter, 0, false, -1, false, null);
			playerData.Name = name;
			playerData.SetCharacter(characterId, this._collection);
			this.AddMatchMember(playerData);
			this.addressIdCounter += 1;
			return playerData;
		}

		public void CreatePlayers(TeamKind team, int count = 1, int characterId = -1)
		{
			for (int i = 0; i < count; i++)
			{
				PlayerData playerData = new PlayerData(string.Empty, 0, team, this.addressIdCounter, 0, false, -1, false, null);
				playerData.Name = this._names[i];
				playerData.SetCharacter(characterId, this._collection);
				this.AddMatchMember(playerData);
				this.addressIdCounter += 1;
			}
		}

		public void SetupMockInventoryCollection(ItemTypeScriptableObject[] charactersInfo = null)
		{
			this._collection = ScriptableObject.CreateInstance<CollectionScriptableObject>();
			charactersInfo = (charactersInfo ?? FakeMatchPlayers.CreateCharacterInfos(30));
			CharacterInfoHandler characterInfoHandler = new CharacterInfoHandler(charactersInfo);
			this._collection.SetupCharacterInfoHandler(characterInfoHandler);
		}

		private static ItemTypeScriptableObject[] CreateCharacterInfos(int count)
		{
			List<ItemTypeScriptableObject> list = new List<ItemTypeScriptableObject>();
			for (int i = 0; i < count; i++)
			{
				CharacterItemTypeComponentTestable characterItemTypeComponentTestable = ScriptableObject.CreateInstance<CharacterItemTypeComponentTestable>();
				characterItemTypeComponentTestable.SetCharacterId(i);
				characterItemTypeComponentTestable.SetRole(FakeMatchPlayers.GetDriverRoleKindByCharId(i));
				BotItemTypeComponentTestable botItemTypeComponentTestable = ScriptableObject.CreateInstance<BotItemTypeComponentTestable>();
				botItemTypeComponentTestable.SetIsAnAvailableBot(true);
				ItemTypeScriptableObject itemTypeScriptableObject = ScriptableObject.CreateInstance<ItemTypeScriptableObject>();
				itemTypeScriptableObject.ReplaceItemTypeComponents(new ItemTypeComponent[0]);
				itemTypeScriptableObject.AddItemTypeComponent(characterItemTypeComponentTestable);
				itemTypeScriptableObject.AddItemTypeComponent(botItemTypeComponentTestable);
				list.Add(itemTypeScriptableObject);
			}
			return list.ToArray();
		}

		private static DriverRoleKind GetDriverRoleKindByCharId(int charId)
		{
			switch (charId % 3)
			{
			case 0:
				return 1;
			case 1:
				return 2;
			case 2:
				return 0;
			default:
				return 1;
			}
		}

		private CollectionScriptableObject _collection;

		private readonly List<PlayerData> players;

		private byte addressIdCounter;

		private string[] _names = new string[]
		{
			"Alisson",
			"Allan",
			"Bruno",
			"Caio",
			"Eduardo",
			"Fabio",
			"Fernando"
		};
	}
}
