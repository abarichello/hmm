using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;

namespace HeavyMetalMachines.Characters
{
	public class LegacyIsCharacterAvailableToBot : IIsCharacterAvailableToBot
	{
		public LegacyIsCharacterAvailableToBot(ICollectionScriptableObject collectionScriptableObject)
		{
			this._collectionScriptableObject = collectionScriptableObject;
		}

		public bool Check(Guid characterId)
		{
			BotItemTypeComponent component = this._collectionScriptableObject.Get(characterId).GetComponent<BotItemTypeComponent>();
			return component.IsAnAvailableBot;
		}

		private readonly ICollectionScriptableObject _collectionScriptableObject;
	}
}
