using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Customization;
using Hoplon.Logging;

namespace HeavyMetalMachines.Social.Avatar.Business
{
	public class GetPortraitIconName : IGetPortraitBorderIconName
	{
		public GetPortraitIconName(ICollectionScriptableObject collection, ICourtesyItemCollection courtesyItemCollection, ILogger<GetPortraitIconName> logger)
		{
			this._courtesyItemCollection = courtesyItemCollection;
			this._logger = logger;
			this._collection = collection;
			this._courtesyItemCollection = courtesyItemCollection;
		}

		private PortraitItemTypeComponent GetPortraitComponent(Guid itemTypeId)
		{
			IItemType defaultItem;
			if (!this._collection.TryGet(itemTypeId, out defaultItem))
			{
				this._logger.WarnFormat("Portrait ItemType could not be found. Fallbacking to default portrait. ItemTypeID={0}", new object[]
				{
					itemTypeId
				});
				defaultItem = this._courtesyItemCollection.GetDefaultItem(60);
			}
			PortraitItemTypeComponent component;
			if (!defaultItem.TryGetComponent<PortraitItemTypeComponent>(out component))
			{
				this._logger.WarnFormat("Portrait ItemType has no Portrait Component. Fallbacking to default portrait. ItemTypeID={0}", new object[]
				{
					itemTypeId
				});
				defaultItem = this._courtesyItemCollection.GetDefaultItem(60);
				component = defaultItem.GetComponent<PortraitItemTypeComponent>();
			}
			return component;
		}

		public string GetSmallSquare(Guid itemTypeId)
		{
			return this.GetPortraitComponent(itemTypeId).SmallBorderName;
		}

		public string GetMediumSquare(Guid itemTypeId)
		{
			return this.GetPortraitComponent(itemTypeId).ProfileBorderName;
		}

		public string GetCircular(Guid itemTypeId)
		{
			return this.GetPortraitComponent(itemTypeId).CircularBorderName;
		}

		public string GetRectangle(Guid itemTypeId)
		{
			return this.GetPortraitComponent(itemTypeId).LoadingBorderName;
		}

		private readonly ICollectionScriptableObject _collection;

		private readonly ICourtesyItemCollection _courtesyItemCollection;

		private readonly ILogger<GetPortraitIconName> _logger;
	}
}
