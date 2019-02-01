using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;
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
			List<PlayerData> list = new List<PlayerData>(this._matchPlayers.BlueTeamPlayersAndBots);
			list.Sort((PlayerData a, PlayerData b) => a.autoDesiredGrid.CompareTo(b.autoDesiredGrid));
			List<PlayerData> list2 = new List<PlayerData>(this._matchPlayers.RedTeamPlayersAndBots);
			list2.Sort((PlayerData a, PlayerData b) => a.autoDesiredGrid.CompareTo(b.autoDesiredGrid));
			for (int i = 0; i < this._matchPlayers.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = this._matchPlayers.PlayersAndBots[i];
				int gridIndex = (playerData.Team != TeamKind.Blue) ? list2.IndexOf(playerData) : list.IndexOf(playerData);
				playerData.autoDesiredGrid = (playerData.SelectedGridIndex = (playerData.GridIndex = gridIndex));
				this._pickService.DispatchConfirmGridSelection(playerData.PlayerAddress, playerData.SelectedGridIndex, this._addressGroups.GetGroup(0));
				this._pickService.DispatchConfirmGridPick(playerData.PlayerAddress, playerData.SelectedGridIndex, playerData.Customizations.SelectedSkin, this._addressGroups.GetGroup(0));
			}
		}

		private readonly CharacterService _pickService;

		private readonly MatchPlayers _matchPlayers;

		private readonly AddressGroupHelper _addressGroups;
	}
}
