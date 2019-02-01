using System;
using HeavyMetalMachines.Character.PickServiceBehavior.Apis;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Character.PickServiceBehavior
{
	public class CustomizationPickServiceBehavior : IPickServiceBehavior
	{
		public CustomizationPickServiceBehavior(CharacterService pickService, MatchPlayers matchPlayers)
		{
			this._pickService = pickService;
			this._matchPlayers = matchPlayers;
		}

		public bool SelectCharacter(byte sender, int characterId)
		{
			CustomizationPickServiceBehavior.Log.WarnFormat("Received SelectCharacter request from {0} for CharacterId={1} while in Customization.", new object[]
			{
				sender,
				characterId
			});
			return false;
		}

		public PickResult PickCharacter(byte sender)
		{
			CustomizationPickServiceBehavior.Log.WarnFormat("Received PickCharacter request from {0} while in Customization.", new object[]
			{
				sender
			});
			return PickResult.PickPhaseOver;
		}

		public void ConfirmSkin(byte sender, string characterGuid, string skinGuid)
		{
			CustomizationPickServiceBehavior.Log.WarnFormat("Received ConfirmSkin request from {0} while in Customization.", new object[]
			{
				sender
			});
			PlayerData playerByAddress = this._matchPlayers.GetPlayerByAddress(sender);
			this._pickService.ServerConfirmSkin(playerByAddress, new Guid(characterGuid), new Guid(skinGuid));
		}

		public void SelectGrid(byte sender, int gridIndex)
		{
			CustomizationPickServiceBehavior.Log.WarnFormat("Received SelectGrid request from {0} while in Customization.", new object[]
			{
				sender
			});
		}

		public PickGridResult PickGrid(byte sender)
		{
			CustomizationPickServiceBehavior.Log.WarnFormat("Received PickGrid request from {0} while in Customization.", new object[]
			{
				sender
			});
			return PickGridResult.None;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CustomizationPickServiceBehavior));

		private readonly CharacterService _pickService;

		private readonly MatchPlayers _matchPlayers;
	}
}
