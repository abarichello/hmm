using System;

namespace HeavyMetalMachines.Customization
{
	[Serializable]
	public class CustomizationInventoryCellItemSkinTabData
	{
		public Guid CharacterId;

		public string CharacterName;

		public string IconAssetName;

		public bool IsNew;

		public bool IsExpanded;

		public bool HasCharacter;
	}
}
