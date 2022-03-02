using System;
using Assets.ClientApiObjects;

namespace HeavyMetalMachines.Frontend
{
	public interface IShopGui
	{
		void ShowDriverDetails(IItemType charItem);

		void ShowSkinDetails(IItemType skinItem);

		void ShowItemTypeDetails(IItemType itemType);
	}
}
