﻿using System;
using HeavyMetalMachines.Characters.PickServiceBehavior.Apis;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Characters.PickServiceBehavior
{
	public class GridSelectionPickServiceBehavior : IPickServiceBehavior
	{
		public GridSelectionPickServiceBehavior(CharacterService pickService, MatchPlayers matchPlayers)
		{
			this._pickService = pickService;
			this._matchPlayers = matchPlayers;
		}

		public bool SelectCharacter(byte sender, int characterId)
		{
			GridSelectionPickServiceBehavior.Log.WarnFormat("Received SelectCharacter request from {0} for CharacterId={1} while in Grid Selection.", new object[]
			{
				sender,
				characterId
			});
			return false;
		}

		public PickResult PickCharacter(byte sender)
		{
			GridSelectionPickServiceBehavior.Log.WarnFormat("Received PickCharacter request from {0} while in Grid Selection.", new object[]
			{
				sender
			});
			return PickResult.PickPhaseOver;
		}

		public void ConfirmSkin(byte sender, string characterGuid, string skinGuid)
		{
			GridSelectionPickServiceBehavior.Log.WarnFormat("Received ConfirmSkin request from {0} while in Grid Selection.", new object[]
			{
				sender
			});
			PlayerData playerByAddress = this._matchPlayers.GetPlayerByAddress(sender);
			this._pickService.ServerConfirmSkin(playerByAddress, new Guid(characterGuid), new Guid(skinGuid));
		}

		public void SelectGrid(byte sender, int gridIndex)
		{
			GridSelectionPickServiceBehavior.Log.WarnFormat("Received SelectGrid request from {0} while in Grid Selection. Grid selection is currently disabled!", new object[]
			{
				sender
			});
		}

		public PickGridResult PickGrid(byte sender)
		{
			GridSelectionPickServiceBehavior.Log.WarnFormat("Received PickGrid request from {0} while in Grid Selection. Grid selection is currently disabled!", new object[]
			{
				sender
			});
			return PickGridResult.None;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GridSelectionPickServiceBehavior));

		private readonly CharacterService _pickService;

		private readonly MatchPlayers _matchPlayers;
	}
}
