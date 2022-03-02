using System;
using HeavyMetalMachines.Characters.PickServiceBehavior.Apis;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Characters.PickServiceBehavior
{
	public class BotPickPickServiceBehavior : IPickServiceBehavior
	{
		public BotPickPickServiceBehavior(CharacterService pickService, MatchPlayers matchPlayers, AddressGroupHelper addressGroups)
		{
			this._pickService = pickService;
			this._matchPlayers = matchPlayers;
			this._addressGroups = addressGroups;
		}

		public bool SelectCharacter(byte sender, int characterId)
		{
			BotPickPickServiceBehavior.Log.WarnFormat("Received SelectCharacter request from {0} for CharacterId={1} while in Bot Picking.", new object[]
			{
				sender,
				characterId
			});
			return false;
		}

		public PickResult PickCharacter(byte sender)
		{
			BotPickPickServiceBehavior.Log.WarnFormat("Received PickCharacter request from {0} while in Bot Picking.", new object[]
			{
				sender
			});
			return PickResult.PickPhaseOver;
		}

		public void ConfirmSkin(byte sender, string characterGuid, string skinGuid)
		{
			PlayerData playerByAddress = this._matchPlayers.GetPlayerByAddress(sender);
			this._pickService.ServerConfirmSkin(playerByAddress, new Guid(characterGuid), new Guid(skinGuid));
		}

		public void SelectGrid(byte sender, int gridIndex)
		{
			this._matchPlayers.GetPlayerByAddress(sender).SelectedGridIndex = gridIndex;
			this._pickService.DispatchConfirmGridSelection(sender, gridIndex, this._addressGroups.GetGroup(0));
		}

		public PickGridResult PickGrid(byte sender)
		{
			PlayerData playerByAddress = this._matchPlayers.GetPlayerByAddress(sender);
			if (playerByAddress.SelectedGridIndex == -1)
			{
				return PickGridResult.InvalidGridIndex;
			}
			bool flag = this.IsGridAlreadyPicked(playerByAddress.PlayerAddress, playerByAddress.Team, playerByAddress.SelectedGridIndex);
			if (flag)
			{
				return PickGridResult.GridAlreadyPicked;
			}
			playerByAddress.GridIndex = playerByAddress.SelectedGridIndex;
			this._pickService.DispatchConfirmGridPick(sender, playerByAddress.SelectedGridIndex, this._addressGroups.GetGroup(0));
			return PickGridResult.Ok;
		}

		private bool IsGridAlreadyPicked(byte targetPlayerAddress, TeamKind team, int targetGridIndex)
		{
			if (targetGridIndex == -1)
			{
				return true;
			}
			for (int i = 0; i < this._matchPlayers.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = this._matchPlayers.PlayersAndBots[i];
				if (!(playerData == null) && playerData.PlayerAddress != targetPlayerAddress && playerData.Team == team)
				{
					if (playerData.GridIndex == targetGridIndex)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BotPickPickServiceBehavior));

		private readonly CharacterService _pickService;

		private readonly MatchPlayers _matchPlayers;

		public readonly AddressGroupHelper _addressGroups;
	}
}
