using System;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Customization.Skins;
using HeavyMetalMachines.Customizations.Skins;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ShopDetailsSkinInfo : GameHubBehaviour
	{
		public void Disable()
		{
			this._mainGameObject.SetActive(false);
		}

		public void Setup(SkinPrefabItemTypeComponent skinPrefabComponent)
		{
			SkinPrefabItemTypeComponent.SkinCustomizations skinCustomization = skinPrefabComponent.SkinCustomization;
			this._mainGameObject.SetActive(true);
			this.GetTitleDraft(skinPrefabComponent.Tier);
			this._customPortraitGameObject.SetActive(skinCustomization.CustomPortrait);
			this._customSfxGameObject.SetActive(skinCustomization.CustomSFX);
			this._customVfxGameObject.SetActive(skinCustomization.CustomVFX);
			this._iconsGrid.Reposition();
		}

		private void GetTitleDraft(TierKind tier)
		{
			SkinRarityInfo skinRarityInfo;
			if (this._skinRarityProvider.TryGetSkinRarityInfo(tier, out skinRarityInfo))
			{
				this._titleLabel.text = Language.Get(skinRarityInfo.LongDraftName, TranslationContext.MainMenuGui);
			}
		}

		public void GridReposition()
		{
			this._iconsGrid.Reposition();
		}

		[SerializeField]
		private GameObject _mainGameObject;

		[SerializeField]
		private UIGrid _iconsGrid;

		[SerializeField]
		private UILabel _titleLabel;

		[SerializeField]
		private GameObject _customPortraitGameObject;

		[SerializeField]
		private GameObject _customSfxGameObject;

		[SerializeField]
		private GameObject _customVfxGameObject;

		[InjectOnClient]
		private ISkinRarityProvider _skinRarityProvider;
	}
}
