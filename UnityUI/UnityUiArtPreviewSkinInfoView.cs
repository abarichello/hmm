using System;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Customization.Skins;
using HeavyMetalMachines.Customizations.Skins;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUiArtPreviewSkinInfoView : MonoBehaviour
	{
		public void Show(SkinPrefabItemTypeComponent skinPrefabComponent)
		{
			SkinPrefabItemTypeComponent.SkinCustomizations skinCustomization = skinPrefabComponent.SkinCustomization;
			this._mainGameObject.SetActive(skinCustomization.CustomPortrait || skinCustomization.CustomSFX || skinCustomization.CustomVFX);
			this.GetTitleDraft(skinPrefabComponent.Tier);
			this._portraitIconGameObject.SetActive(skinCustomization.CustomPortrait);
			this._audioIconGameObject.SetActive(skinCustomization.CustomSFX);
			this._effectsIconGameObject.SetActive(skinCustomization.CustomVFX);
			this._portraitTranslatedText = Language.Get("TYPE_POSE_TITLE", TranslationContext.MainMenuGui);
			this._audioTranslatedText = Language.Get("TYPE_SOUND_TITLE", TranslationContext.MainMenuGui);
			this._effectsTranslatedText = Language.Get("TYPE_EFFECTS_TITLE", TranslationContext.MainMenuGui);
			this._tooltipText.CrossFadeAlpha(0f, 0f, true);
		}

		public void Hide()
		{
			this._mainGameObject.SetActive(false);
		}

		[UnityUiComponentCall]
		public void TooltipPointerEnter(GameObject targetGameObject)
		{
			this._tooltipText.text = this.GetTooltipText(targetGameObject.GetInstanceID());
			this._tooltipText.CrossFadeAlpha(1f, this._tooltipCrossFadeAlpha, true);
		}

		[UnityUiComponentCall]
		public void TooltipPointerExit(GameObject targetGameObject)
		{
			this._tooltipText.CrossFadeAlpha(0f, this._tooltipCrossFadeAlpha, true);
		}

		private string GetTooltipText(int instanceId)
		{
			if (instanceId == this._portraitIconGameObject.GetInstanceID())
			{
				return this._portraitTranslatedText;
			}
			if (instanceId == this._audioIconGameObject.GetInstanceID())
			{
				return this._audioTranslatedText;
			}
			return this._effectsTranslatedText;
		}

		private void GetTitleDraft(TierKind tier)
		{
			SkinRarityInfo skinRarityInfo;
			if (this._skinRarityProvider.TryGetSkinRarityInfo(tier, out skinRarityInfo))
			{
				this._titleText.text = Language.Get(skinRarityInfo.LongDraftName, TranslationContext.MainMenuGui);
			}
		}

		[SerializeField]
		private float _tooltipCrossFadeAlpha = 0.1f;

		[Header("[Components]")]
		[SerializeField]
		private GameObject _mainGameObject;

		[SerializeField]
		private Text _titleText;

		[SerializeField]
		private GameObject _portraitIconGameObject;

		[SerializeField]
		private GameObject _audioIconGameObject;

		[SerializeField]
		private GameObject _effectsIconGameObject;

		[SerializeField]
		private Text _tooltipText;

		private string _portraitTranslatedText;

		private string _audioTranslatedText;

		private string _effectsTranslatedText;

		[InjectOnClient]
		private ISkinRarityProvider _skinRarityProvider;
	}
}
