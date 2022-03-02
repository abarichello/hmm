using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using Hoplon.Logging;
using Hoplon.Serialization;

namespace HeavyMetalMachines.Inventory.Business
{
	public class GetCharacterItemTypeFromSkinItemTypeId : IGetCharacterItemTypeFromSkinItemTypeId
	{
		public GetCharacterItemTypeFromSkinItemTypeId(ILogger<GetCharacterItemTypeFromSkinItemTypeId> logger, ICollectionScriptableObject getItemFromCollection)
		{
			this._logger = logger;
			this._getItemFromCollection = getItemFromCollection;
		}

		public IItemType GetFromSkinId(Guid skinItemTypeId)
		{
			IItemType itemType = this._getItemFromCollection.Get(skinItemTypeId);
			if (itemType == null)
			{
				this._logger.ErrorFormat("GetFromSkinId without skin! Guid={0}", new object[]
				{
					skinItemTypeId
				});
				return null;
			}
			if (string.IsNullOrEmpty(itemType.Bag))
			{
				this._logger.ErrorFormat("GetFromSkinId Item without bag! Guid={0}", new object[]
				{
					skinItemTypeId
				});
				return null;
			}
			SkinItemTypeBag skinItemTypeBag = JsonSerializeable<SkinItemTypeBag>.Deserialize(itemType.Bag);
			return this._getItemFromCollection.Get(skinItemTypeBag.CharacterItemTypeId);
		}

		private readonly ILogger<GetCharacterItemTypeFromSkinItemTypeId> _logger;

		private readonly ICollectionScriptableObject _getItemFromCollection;
	}
}
