using System;
using HeavyMetalMachines.DataTransferObjects.Inventory;

namespace HeavyMetalMachines.Inventory.Business
{
	public interface IGetPackageFromItemTypeId
	{
		PackageItemTypeBag Get(Guid itemTypeId);
	}
}
