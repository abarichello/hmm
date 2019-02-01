using System;
using HeavyMetalMachines.Character.PickServiceBehavior.Apis;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Character.PickServiceBehavior
{
	public class PlayerPickPickServiceBehavior : IPickServiceBehavior
	{
		public PlayerPickPickServiceBehavior(CharacterService pickService, MatchPlayers matchPlayers)
		{
			this._pickService = pickService;
			this._matchPlayers = matchPlayers;
		}

		public bool SelectCharacter(byte sender, int characterId)
		{
			PlayerData playerByAddress = this._matchPlayers.GetPlayerByAddress(sender);
			playerByAddress.SelectedChar = -1;
			if (this._pickService.HasCharacter(playerByAddress, characterId))
			{
				playerByAddress.SelectedChar = characterId;
			}
			else
			{
				PlayerPickPickServiceBehavior.Log.WarnFormat("Player {0} doesn't have the requested character. CharId={1}", new object[]
				{
					sender,
					characterId
				});
			}
			PlayerPickPickServiceBehavior.Log.InfoFormat("Player {0} requested character CharId={1}; Selected was CharId={2}", new object[]
			{
				sender,
				characterId,
				playerByAddress.SelectedChar
			});
			this._pickService.DispatchConfirmSelection(playerByAddress);
			return true;
		}

		public PickResult PickCharacter(byte sender)
		{
			PlayerData playerByAddress = this._matchPlayers.GetPlayerByAddress(sender);
			PickResult pickResult = (PickResult)this._pickService.PickCharacter(playerByAddress, false);
			if (pickResult != PickResult.Ok && pickResult != PickResult.PickPhaseOver)
			{
				PlayerPickPickServiceBehavior.Log.WarnFormat("PickCharacter failed. PickResult={0} Player={1} SelectedChar={2} PickedChar={3}", new object[]
				{
					pickResult,
					sender,
					playerByAddress.SelectedChar,
					playerByAddress.CharacterId
				});
			}
			return pickResult;
		}

		public void ConfirmSkin(byte sender, string characterGuid, string skinGuid)
		{
			PlayerData playerByAddress = this._matchPlayers.GetPlayerByAddress(sender);
			this._pickService.ServerConfirmSkin(playerByAddress, new Guid(characterGuid), new Guid(skinGuid));
			this._pickService.CheckAllConfirmed();
		}

		public void SelectGrid(byte sender, int gridIndex)
		{
			PlayerPickPickServiceBehavior.Log.WarnFormat("Received PickGrid request from {0} gridIndex={1} while in Player Picking.", new object[]
			{
				sender,
				gridIndex
			});
		}

		public PickGridResult PickGrid(byte sender)
		{
			PlayerPickPickServiceBehavior.Log.WarnFormat("Received PickGrid request from {0} while in Player Picking.", new object[]
			{
				sender
			});
			return PickGridResult.None;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PlayerPickPickServiceBehavior));

		private readonly CharacterService _pickService;

		private readonly MatchPlayers _matchPlayers;
	}
}
