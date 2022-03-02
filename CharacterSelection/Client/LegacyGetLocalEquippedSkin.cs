using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.CharacterSelection.Client.API;
using HeavyMetalMachines.CharacterSelection.Client.Skins;
using HeavyMetalMachines.CharacterSelection.Skins;
using Hoplon.Logging;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class LegacyGetLocalEquippedSkin : IGetLocalEquippedSkin
	{
		public LegacyGetLocalEquippedSkin(ICollectionScriptableObject collectionScriptableObject, ILogger<LegacyGetLocalEquippedSkin> logger)
		{
			this._collectionScriptableObject = collectionScriptableObject;
			this._logger = logger;
		}

		public Guid Get(ClientCharacterSelection characterSelection, Guid characterId)
		{
			bool flag = characterSelection.LocalPlayerState != null;
			if (flag)
			{
				bool flag2 = characterSelection.LocalPlayerState.EquippedSkins != null;
				if (flag2)
				{
					foreach (CharacterEquippedSkin characterEquippedSkin in characterSelection.LocalPlayerState.EquippedSkins)
					{
						if (characterEquippedSkin.CharacterId == characterId && this._collectionScriptableObject.Exists(characterEquippedSkin.SkinId))
						{
							return characterEquippedSkin.SkinId;
						}
					}
				}
				this._logger.WarnFormat("equippedSkin not found for characterId={0}. ", new object[]
				{
					characterId
				});
			}
			return this._collectionScriptableObject.GetDefaultSkin(characterId).Id;
		}

		private readonly ILogger<LegacyGetLocalEquippedSkin> _logger;

		private readonly ICollectionScriptableObject _collectionScriptableObject;
	}
}
