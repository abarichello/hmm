using System;
using System.Collections.Generic;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Server.Pick.Apis;
using Pocketverse;

namespace HeavyMetalMachines.Server.Pick
{
	public class GridSelectionPickState : IPickModeState
	{
		public GridSelectionPickState(CharacterService pickService, MatchPlayers matchPlayers, AddressGroupHelper addressGroups)
		{
			this._pickService = pickService;
			this._matchPlayers = matchPlayers;
			this._addressGroups = addressGroups;
		}

		public bool IsInitialized
		{
			get
			{
				return true;
			}
		}

		public void Initialize()
		{
			throw new NotImplementedException();
		}

		public bool Update()
		{
			this.AutoSelectGridAll();
			return true;
		}

		public void AutoSelectGridAll()
		{
			this.DispatchGridSelectionAndConfirm(this._matchPlayers.BlueTeamPlayersAndBots);
			this.DispatchGridSelectionAndConfirm(this._matchPlayers.RedTeamPlayersAndBots);
		}

		private void DispatchGridSelectionAndConfirm(List<PlayerData> unsortedPlayersAndBotsList)
		{
			List<PlayerData> list = new List<PlayerData>(unsortedPlayersAndBotsList);
			list.Sort((PlayerData a, PlayerData b) => a.autoDesiredGrid.CompareTo(b.autoDesiredGrid));
			for (int i = 0; i < list.Count; i++)
			{
				PlayerData playerData = list[i];
				playerData.autoDesiredGrid = (playerData.SelectedGridIndex = (playerData.GridIndex = i));
				this._pickService.DispatchConfirmGridSelection(playerData.PlayerAddress, playerData.SelectedGridIndex, this._addressGroups.GetGroup(0));
				this._pickService.DispatchConfirmGridPick(playerData.PlayerAddress, playerData.SelectedGridIndex, this._addressGroups.GetGroup(0));
			}
		}

		private readonly CharacterService _pickService;

		private readonly MatchPlayers _matchPlayers;

		private readonly AddressGroupHelper _addressGroups;
	}
}
