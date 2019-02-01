using System;

namespace HeavyMetalMachines.Customization
{
	public interface ICustomizationInventoryCategoriesView
	{
		void UpdateNewItemsMarker(Guid categoryId, int newItemsCount);
	}
}
