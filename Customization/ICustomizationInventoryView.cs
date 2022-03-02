using System;

namespace HeavyMetalMachines.Customization
{
	public interface ICustomizationInventoryView
	{
		void SetVisibility(bool isVisible, bool imediate);

		bool IsVisible();

		void Setup(CustomizationInventoryCategoryData data);

		void OnEquipItemResponse(bool success);

		void RegisterCategoriesView(ICustomizationInventoryCategoriesView categoriesView);

		int GetNewItemsCount(Guid categoryId);

		void MarkAllItemsAsSeen(Guid[] categoriesWithNewItems);

		IGetCustomizationChange GetCustomizationInventory();
	}
}
