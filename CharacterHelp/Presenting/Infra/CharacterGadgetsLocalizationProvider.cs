using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Characters;

namespace HeavyMetalMachines.CharacterHelp.Presenting.Infra
{
	public class CharacterGadgetsLocalizationProvider : ICharacterGadgetsLocalizationProvider
	{
		public CharacterGadgetsLocalizationProvider(ICollectionScriptableObject collectionScriptableObject)
		{
			this._collectionScriptableObject = collectionScriptableObject;
		}

		public CharacterGadgetLocalizationInfo GetGadgetPassiveLocalizationInfo(Guid characterId)
		{
			CharacterItemTypeComponent itemTypeComponent = this.GetItemTypeComponent(characterId);
			if (!itemTypeComponent.HasPassive)
			{
				CharacterGadgetLocalizationInfo result = default(CharacterGadgetLocalizationInfo);
				result.IsValid = false;
				return result;
			}
			return this.ConvertToInfo(itemTypeComponent.PassiveGadgetDrafts, itemTypeComponent.PassiveGadgetIconName);
		}

		public CharacterGadgetLocalizationInfo GetGadgetBasicLocalizationInfo(Guid characterId)
		{
			CharacterItemTypeComponent itemTypeComponent = this.GetItemTypeComponent(characterId);
			return this.ConvertToInfo(itemTypeComponent.GadgetBasicDrafts, itemTypeComponent.GadgetBasicIconName);
		}

		public CharacterGadgetLocalizationInfo GetGadget0LocalizationInfo(Guid characterId)
		{
			CharacterItemTypeComponent itemTypeComponent = this.GetItemTypeComponent(characterId);
			return this.ConvertToInfo(itemTypeComponent.Gadget0Drafts, itemTypeComponent.Gadget0IconName);
		}

		public CharacterGadgetLocalizationInfo GetGadgetNitroLocalizationInfo(Guid characterId)
		{
			CharacterItemTypeComponent itemTypeComponent = this.GetItemTypeComponent(characterId);
			return this.ConvertToInfo(itemTypeComponent.GadgetNitroDrafts, itemTypeComponent.GadgetNitroIconName);
		}

		public CharacterGadgetLocalizationInfo GetGadget1LocalizationInfo(Guid characterId)
		{
			CharacterItemTypeComponent itemTypeComponent = this.GetItemTypeComponent(characterId);
			return this.ConvertToInfo(itemTypeComponent.Gadget1Drafts, itemTypeComponent.Gadget1IconName);
		}

		private CharacterItemTypeComponent GetItemTypeComponent(Guid characterId)
		{
			IItemType itemType = this._collectionScriptableObject.Get(characterId);
			return itemType.GetComponent<CharacterItemTypeComponent>();
		}

		private CharacterGadgetLocalizationInfo ConvertToInfo(CharacterGadgetDrafts characterGadgetDrafts, string iconName)
		{
			CharacterGadgetLocalizationInfo result = default(CharacterGadgetLocalizationInfo);
			result.IsValid = true;
			result.NameDraft = characterGadgetDrafts.NameDraft;
			result.CooldownDraft = characterGadgetDrafts.CooldownDraft;
			result.DescriptionDraft = characterGadgetDrafts.DescriptionDraft;
			result.IconName = iconName;
			return result;
		}

		private readonly ICollectionScriptableObject _collectionScriptableObject;
	}
}
