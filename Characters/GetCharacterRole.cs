using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Hoplon.Logging;

namespace HeavyMetalMachines.Characters
{
	public class GetCharacterRole : IGetCharacterRole
	{
		public GetCharacterRole(ILogger<GetCharacterRole> logger, ICollectionScriptableObject collectionScriptableObject)
		{
			this._logger = logger;
			this._collectionScriptableObject = collectionScriptableObject;
		}

		public DriverRoleKind Get(Guid characterId)
		{
			IItemType itemType;
			if (this._collectionScriptableObject.TryGet(characterId, out itemType))
			{
				return itemType.GetComponent<CharacterItemTypeComponent>().Role;
			}
			this._logger.ErrorStackTrace(string.Format("Skipping character {0}, data not found in collection. Is this a unreleased character? The QA database must be reset after testing a new character.", characterId));
			return 0;
		}

		private readonly ILogger<GetCharacterRole> _logger;

		private readonly ICollectionScriptableObject _collectionScriptableObject;
	}
}
