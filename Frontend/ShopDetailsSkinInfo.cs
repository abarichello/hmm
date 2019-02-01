using System;
using Assets.ClientApiObjects.Components;
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

		public void Setup(SkinPrefabItemTypeComponent.SkinCustomizations skinCustomizations)
		{
			bool flag = skinCustomizations.CustomPortrait || skinCustomizations.CustomSFX || skinCustomizations.CustomVFX;
			this._mainGameObject.SetActive(flag);
			if (flag)
			{
				this._customPortraitGameObject.SetActive(skinCustomizations.CustomPortrait);
				this._customSfxGameObject.SetActive(skinCustomizations.CustomSFX);
				this._customVfxGameObject.SetActive(skinCustomizations.CustomVFX);
				this._iconsGrid.Reposition();
			}
			this.HideTooltip();
		}

		public void ShowTooltip(GameObject iconGameObject)
		{
			string key = "TYPE_POSE_TITLE";
			if (iconGameObject == this._customSfxGameObject)
			{
				key = "TYPE_SOUND_TITLE";
			}
			else if (iconGameObject == this._customVfxGameObject)
			{
				key = "TYPE_EFFECTS_TITLE";
			}
			this._tooltipLabel.text = Language.Get(key, TranslationSheets.MainMenuGui);
			this._tooltipLabel.gameObject.SetActive(true);
		}

		public void HideTooltip()
		{
			this._tooltipLabel.gameObject.SetActive(false);
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
		private GameObject _customPortraitGameObject;

		[SerializeField]
		private GameObject _customSfxGameObject;

		[SerializeField]
		private GameObject _customVfxGameObject;

		[SerializeField]
		private UILabel _tooltipLabel;
	}
}
