using System;
using Assets.ClientApiObjects;

namespace HeavyMetalMachines.Store.Details.Infra
{
	public class StoreItemDetailsRequest : IStoreItemDetailsRequestProvider
	{
		public IItemType ConsumeItemToShow()
		{
			IItemType itemType = this._itemType;
			this._itemType = null;
			return itemType;
		}

		public void SetItemToShow(IItemType itemType)
		{
			this._itemType = itemType;
		}

		private IItemType _itemType;
	}
}
