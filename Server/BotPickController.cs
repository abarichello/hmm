using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.AI;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Pick;
using HeavyMetalMachines.Server.Apis;
using HeavyMetalMachines.Server.BotPick;
using HeavyMetalMachines.Server.Pick.Rules;
using HeavyMetalMachines.Server.Pick.Rules.Apis;
using Pocketverse;

namespace HeavyMetalMachines.Server
{
	public class BotPickController : IBotPickController
	{
		public BotPickController(CharacterService pickService, MatchPlayers matchPlayers, IConfigLoader configLoader, BotAIMatchRules botAIMatchRules, CollectionScriptableObject inventoryCollection, MatchPickModeConfig pickConfig)
		{
			this._pickService = pickService;
			this._matchPlayers = matchPlayers;
			this._configLoader = configLoader;
			this._botAIMatchRules = botAIMatchRules;
			this._inventoryCollection = inventoryCollection;
			this._pickConfig = pickConfig;
			this.InitializeRules();
		}

		public bool IsBotPicking { get; private set; }

		private void InitializeRules()
		{
			IItemType[] allAvailableCharactersItemTypes = this._inventoryCollection.GetAllAvailableCharactersItemTypes();
			PriorityRolesProvider priorityRolesProvider = new PriorityRolesProvider(this._matchPlayers, this._botAIMatchRules.BotPickConfig.roles, new PriorityRolesProvider.GetBotDesiredPickCb(this.BotDesiredPickGetter), new PriorityRolesProvider.GetDriverRoleKindByCharIdCb(this.DriverRoleKindByCharacterIdGetter));
			BotsCharacterFilter botsCharacterFilter = new BotsCharacterFilter();
			IItemType[] validCharactersForBots = botsCharacterFilter.FilterCharacters(allAvailableCharactersItemTypes);
			this._priorityBotCharacterSelector = new PriorityBotCharacterSelector(validCharactersForBots, this._matchPlayers, new PriorityBotCharacterSelector.GetBotDesiredPickCb(this.BotDesiredPickGetter), priorityRolesProvider);
			this._randomBotCharacterSelector = new RandomBotCharacterSelector(validCharactersForBots, this._matchPlayers.Players, this._matchPlayers.Bots);
		}

		public int BotDesiredPickGetter(byte address)
		{
			return this._botsPickData[address].DesiredPick;
		}

		public DriverRoleKind DriverRoleKindByCharacterIdGetter(int characterId)
		{
			IItemType itemType = this._inventoryCollection.AllCharactersByCharacterId[characterId];
			CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
			return component.Role;
		}

		public void Initialize()
		{
			this._botsPickData = new Dictionary<byte, BotPickData>();
			for (int i = 0; i < this._matchPlayers.Bots.Count; i++)
			{
				PlayerData playerData = this._matchPlayers.Bots[i];
				BotPickData botPickData = new BotPickData(this._pickConfig.MinBotCharSelectionTime, this._pickConfig.MaxBotCharSelectionTime, this._pickConfig.MinBotCharConfirmationTime, this._pickConfig.MaxBotCharConfirmationTime);
				botPickData.SetupRandomSelectionDelay();
				this._botsPickData.Add(playerData.PlayerAddress, botPickData);
			}
			this._botPickDesiresDefiner = new BotPickDesiresDefiner(this._matchPlayers, new BotPickDesiresDefiner.SelectPriorityCharacterForBotCb(this.SelectPriorityCharacterForBot), this._configLoader, this._botsPickData);
		}

		public void DefineBotsDesires()
		{
			this._botPickDesiresDefiner.DefineBotsDesires();
		}

		public void UpdateAllBot(float deltaTime)
		{
			this.IsBotPicking = false;
			for (int i = 0; i < this._matchPlayers.Bots.Count; i++)
			{
				PlayerData playerData = this._matchPlayers.Bots[i];
				if (!(playerData == null) && !playerData.CharConfirmed)
				{
					this.IsBotPicking = true;
					this.UpdateBot(playerData, deltaTime);
				}
			}
		}

		private void UpdateBot(PlayerData botEntry, float deltaTime)
		{
			BotPickData botPickData = this._botsPickData[botEntry.PlayerAddress];
			botPickData.DecrementeDelay(deltaTime);
			bool flag = botPickData.IsWaiting(this._pickService.PickTime);
			if (flag)
			{
				return;
			}
			int desiredPick = this._botsPickData[botEntry.PlayerAddress].DesiredPick;
			if (botEntry.SelectedChar != desiredPick)
			{
				if (this._pickService.CharAlreadyPicked(botEntry, botEntry.Team, desiredPick))
				{
					this.SelectPriorityCharacterForBot(botEntry);
				}
				botEntry.SelectedChar = desiredPick;
				this._pickService.DispatchConfirmSelection(botEntry);
				if (this._pickService.PickTime > this._pickConfig.MinBotCharConfirmationTime)
				{
					botPickData.SetupRandomConfirmationDelay();
					return;
				}
			}
			if (botEntry.SelectedChar != -1)
			{
				this._pickService.PickCharacter(botEntry, true);
				botEntry.SelectedChar = botEntry.CharacterId;
				Guid guid = botEntry.Customizations.GetGuidBySlot(59);
				if (guid == Guid.Empty)
				{
					ItemTypeScriptableObject defaultSkin = this._inventoryCollection.GetDefaultSkin(botEntry.CharacterItemType.Id);
					guid = defaultSkin.Id;
					botEntry.Customizations.SetGuidAndSlot(59, guid);
				}
				this._pickService.DispatchConfirmSkinCallback(botEntry.PlayerAddress, botEntry.Team, true, guid);
			}
		}

		public void UpdateBotFakeSelection(float deltaTime)
		{
			for (int i = 0; i < this._matchPlayers.Bots.Count; i++)
			{
				PlayerData playerData = this._matchPlayers.Bots[i];
				BotPickData botPickData = this._botsPickData[playerData.PlayerAddress];
				botPickData.DecrementeDelay(deltaTime);
				if (!botPickData.IsWaiting(this._pickService.PickTime))
				{
					this.SelectRandomCharacterForBot(playerData);
				}
			}
		}

		private void SelectRandomCharacterForBot(PlayerData bot)
		{
			int num = this._randomBotCharacterSelector.SelectCharacter(bot);
			if (num < 0)
			{
				BotPickController.Log.ErrorFormat("No possible character to randomly select for bot={0} team={1}", new object[]
				{
					bot.PlayerAddress,
					bot.Team
				});
				return;
			}
			bot.SelectedChar = num;
			this._pickService.DispatchConfirmSelection(bot);
			this._botsPickData[bot.PlayerAddress].SetupRandomSelectionDelay();
		}

		public void UpdateAllGridDesires()
		{
			List<PlayerData> list = new List<PlayerData>();
			for (int i = 0; i < this._matchPlayers.PlayersAndBots.Count; i++)
			{
				list.Add(this._matchPlayers.PlayersAndBots[i]);
			}
			list.Sort((PlayerData entryA, PlayerData entryB) => entryA.autoGridPriority.CompareTo(entryB.autoGridPriority));
			for (int j = 0; j < list.Count; j++)
			{
				this.UpdateGridDesire(list[j]);
			}
		}

		private void UpdateGridDesire(PlayerData botEntry)
		{
			if (botEntry.CharSkinGridConfirmed || !this.CheckGridPickAndPriority(botEntry, botEntry.Team, botEntry.autoDesiredGrid))
			{
				return;
			}
			for (int i = botEntry.autoDesiredGrid; i < this._pickService.MaxGridSlots; i++)
			{
				if (!this.CheckGridPickAndPriority(botEntry, botEntry.Team, i))
				{
					botEntry.autoDesiredGrid = i;
					return;
				}
			}
			for (int j = 0; j < this._pickService.MaxGridSlots; j++)
			{
				if (!this.CheckGridPickAndPriority(botEntry, botEntry.Team, j))
				{
					botEntry.autoDesiredGrid = j;
					return;
				}
			}
		}

		private void SelectPriorityCharacterForBot(PlayerData bot)
		{
			int num = this._priorityBotCharacterSelector.SelectCharacter(bot);
			if (num == -1)
			{
				BotPickController.Log.ErrorFormat("No possible character for bot to select. player={0} team={1}", new object[]
				{
					bot.PlayerAddress,
					bot.Team
				});
				return;
			}
			IItemType itemType = this._inventoryCollection.AllCharactersByCharacterId[num];
			CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
			BotPickData botPickData = this._botsPickData[bot.PlayerAddress];
			botPickData.DesiredPick = component.CharacterId;
			bot.autoDesiredGrid = component.PreferredGridPosition;
			bot.autoGridPriority = component.PreferredGridPosition;
			BotPickController.Log.InfoFormat("Bot={0} team={1} botDesiredPick={2}", new object[]
			{
				bot.PlayerAddress,
				bot.Team,
				botPickData.DesiredPick
			});
		}

		private bool CheckGridPickAndPriority(PlayerData player, TeamKind team, int targetGridIndex)
		{
			if (targetGridIndex == -1)
			{
				return true;
			}
			for (int i = 0; i < this._matchPlayers.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = this._matchPlayers.PlayersAndBots[i];
				if (!(playerData == null) && playerData.PlayerAddress != player.PlayerAddress && playerData.Team == team)
				{
					if (playerData.GridIndex == targetGridIndex)
					{
						return true;
					}
					if (playerData.autoDesiredGrid == targetGridIndex)
					{
						if (playerData.autoGridPriority < player.autoGridPriority)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BotPickController));

		private readonly CharacterService _pickService;

		private readonly MatchPlayers _matchPlayers;

		private readonly IConfigLoader _configLoader;

		private readonly BotAIMatchRules _botAIMatchRules;

		private readonly CollectionScriptableObject _inventoryCollection;

		private readonly MatchPickModeConfig _pickConfig;

		private Dictionary<byte, BotPickData> _botsPickData;

		private BotPickDesiresDefiner _botPickDesiresDefiner;

		private IBotCharacterSelector _priorityBotCharacterSelector;

		private IBotCharacterSelector _randomBotCharacterSelector;
	}
}
