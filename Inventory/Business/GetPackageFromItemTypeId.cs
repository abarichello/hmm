using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using Hoplon.Logging;
using Hoplon.Serialization;

namespace HeavyMetalMachines.Inventory.Business
{
	public class GetPackageFromItemTypeId : IGetPackageFromItemTypeId
	{
		public GetPackageFromItemTypeId(ILogger<GetPackageFromItemTypeId> logger, ICollectionScriptableObject getItemFromCollection)
		{
			this._logger = logger;
			this._getItemFromCollection = getItemFromCollection;
		}

		public PackageItemTypeBag Get(Guid itemTypeId)
		{
			PackageItemTypeScriptableObject packageItemTypeScriptableObject = this._getItemFromCollection.Get(itemTypeId) as PackageItemTypeScriptableObject;
			if (packageItemTypeScriptableObject == null)
			{
				this._logger.ErrorFormat("GetPackageFromItemTypeId without package! Guid={0}", new object[]
				{
					itemTypeId
				});
				return null;
			}
			return JsonSerializeable<PackageItemTypeBag>.Deserialize(packageItemTypeScriptableObject.Bag);
		}

		private readonly ILogger<GetPackageFromItemTypeId> _logger;

		private readonly ICollectionScriptableObject _getItemFromCollection;
	}
}
