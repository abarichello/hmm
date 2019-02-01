using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Server.Pick.Apis;
using UnityEngine;

namespace HeavyMetalMachines.Server.Pick
{
	public class PlayerPickState : IPickModeState
	{
		public PlayerPickState(CharacterService pickService, BotPickController botPick, MatchPlayers players, float availableBotPickTime)
		{
			this._pickService = pickService;
			this._botPick = botPick;
			this._players = players;
			this._availableBotPickTime = availableBotPickTime;
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
			float deltaTime = Time.deltaTime;
			this._pickService.PickTime -= deltaTime;
			if (this._pickService.PickTime < this._availableBotPickTime && (this._pickService.AllConfirmed || this._pickService.PickTime <= 0f))
			{
				this.FinalizePlayerPickPhase();
				return true;
			}
			this._botPick.UpdateBotFakeSelection(deltaTime);
			return false;
		}

		private void FinalizePlayerPickPhase()
		{
			List<PlayerData> players = this._players.Players;
			for (int i = 0; i < players.Count; i++)
			{
				PlayerData playerData = players[i];
				if (playerData.CharacterId < 0)
				{
					this._pickService.SelectRandomCharacterByPriority(playerData, true);
					this._pickService.PickCharacter(playerData, false);
				}
				if (playerData.GridIndex < 0)
				{
					playerData.autoDesiredGrid = playerData.Character.PreferedGridPosition;
					playerData.autoGridPriority = playerData.Character.PreferedGridPosition;
					this._pickService.ServerConfirmSkin(playerData, playerData.Character.CharacterItemTypeGuid, playerData.Customizations.SelectedSkin);
				}
			}
			this._pickService.CheckAllConfirmed();
		}

		private readonly CharacterService _pickService;

		private readonly BotPickController _botPick;

		private readonly MatchPlayers _players;

		private readonly float _availableBotPickTime;
	}
}
