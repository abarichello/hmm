using System;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Specializations;

namespace HeavyMetalMachines.Customization
{
	[Serializable]
	public class CustomizationInventoryCellItemData
	{
		public string ItemName;

		public string ItemDescription;

		public string IconName;

		public string PreviewName;

		public ItemPreviewKind PreviewKind;

		public bool IsSelected;

		public bool IsDefault;

		public bool IsNew;

		public long ItemId;

		public Guid ItemTypeId;

		public Guid ItemCategoryId;

		public DateTime DateAcquired;

		public string LoreTitleDraft;

		public string LoreSubtitleDraft;

		public string LoreDescriptionDraft;

		public string ArtPreviewBackGroundAssetName;

		public SkinPrefabItemTypeComponent SkinPrefabComponent;
	}
}
