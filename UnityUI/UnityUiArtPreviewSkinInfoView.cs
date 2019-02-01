using System;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUiArtPreviewSkinInfoView : MonoBehaviour
	{
		public void Show(SkinPrefabItemTypeComponent.SkinCustomizations skinCustomizations)
		{
			this._mainGameObject.SetActive(skinCustomizations.CustomPortrait || skinCustomizations.CustomSFX || skinCustomizations.CustomVFX);
			this._portraitIconGameObject.SetActive(skinCustomizations.CustomPortrait);
			this._audioIconGameObject.SetActive(skinCustomizations.CustomSFX);
			this._effectsIconGameObject.SetActive(skinCustomizations.CustomVFX);
			this._portraitTranslatedText = Language.Get("TYPE_POSE_TITLE", TranslationSheets.MainMenuGui);
			this._audioTranslatedText = Language.Get("TYPE_SOUND_TITLE", TranslationSheets.MainMenuGui);
			this._effectsTranslatedText = Language.Get("TYPE_EFFECTS_TITLE", TranslationSheets.MainMenuGui);
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

		[SerializeField]
		private float _tooltipCrossFadeAlpha = 0.1f;

		[Header("[Components]")]
		[SerializeField]
		private GameObject _mainGameObject;

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
	}
}
