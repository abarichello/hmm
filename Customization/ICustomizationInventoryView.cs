using System;
using Commons.Swordfish.Battlepass;

namespace HeavyMetalMachines.Customization
{
	public interface ICustomizationInventoryView
	{
		void SetVisibility(bool isVisible, bool imediate);

		bool IsVisible();

		void Setup(CustomizationInventoryCategoryData data);

		void SelectCategory(Guid categoryId, PlayerCustomizationSlot customizationSlot);

		void OnEquipItemResponse(bool success);

		void RegisterCategoriesView(ICustomizationInventoryCategoriesView categoriesView);

		int GetNewItemsCount(Guid categoryId);

		void MarkAllItemsAsSeen(Guid[] categoriesWithNewItems);
	}
}
