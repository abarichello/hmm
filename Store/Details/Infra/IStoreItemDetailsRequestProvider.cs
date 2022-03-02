using System;
using Assets.ClientApiObjects;

namespace HeavyMetalMachines.Store.Details.Infra
{
	public interface IStoreItemDetailsRequestProvider
	{
		IItemType ConsumeItemToShow();

		void SetItemToShow(IItemType itemType);
	}
}
