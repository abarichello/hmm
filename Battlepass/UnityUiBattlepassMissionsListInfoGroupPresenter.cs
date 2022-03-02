using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassMissionsListInfoGroupPresenter : MonoBehaviour
	{
		public void Setup(bool isActiveMissions, int currentValue, int totalvalue)
		{
			this._titleText.text = Language.Get((!isActiveMissions) ? "BATTLEPASS_MISSIONS_COMPLETED" : "BATTLEPASS_MISSIONS_ACTIVE", TranslationContext.BattlepassMissions);
			string text = HudUtils.RGBToHex(this._activeMissionsTextColor);
			string text3;
			if (isActiveMissions)
			{
				string text2 = HudUtils.RGBToHex(this._activeTotalMissionsTextColor);
				text3 = string.Format("<color=#{0}>{1}</color><color=#{2}> / {3}</color>", new object[]
				{
					(currentValue != totalvalue) ? text : text2,
					currentValue,
					text2,
					totalvalue
				});
			}
			else
			{
				text3 = string.Format("<color=#{0}>{1}</color>", text, totalvalue);
			}
			HoplonUiUtils.SetPreferedWidth(text3, this._infoText, 0f);
			this._infoText.text = text3;
		}

		[SerializeField]
		private Color _activeMissionsTextColor;

		[SerializeField]
		private Color _activeTotalMissionsTextColor;

		[SerializeField]
		private Text _titleText;

		[SerializeField]
		private Text _infoText;
	}
}
