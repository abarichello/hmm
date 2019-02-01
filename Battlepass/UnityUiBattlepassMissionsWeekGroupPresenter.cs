using System;
using Commons.Swordfish.Battlepass;
using HeavyMetalMachines.Frontend;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	internal class UnityUiBattlepassMissionsWeekGroupPresenter : MonoBehaviour
	{
		public void Setup(BattlepassConfig battlepassConfig, DateTime utcNow)
		{
			DateTime endDate = battlepassConfig.GetEndDate();
			DateTime startDate = battlepassConfig.GetStartDate();
			int num = Mathf.CeilToInt((float)(endDate - startDate).TotalDays / 7f);
			float f = (float)(utcNow - startDate).TotalDays / 7f;
			int num2 = Mathf.CeilToInt(f);
			num2 = Mathf.Clamp(num2, 1, num);
			DateTime d = startDate + new TimeSpan(num2 * 7, 0, 0, 0);
			TimeSpan remainingTime = d - utcNow;
			this._weekNumberLabel.text = string.Format("{0} <color=#{2}>/ {1}</color>", num2, num, HudUtils.RGBToHex(this._highlightColor));
			this.SetNextWeekRemainingTime(num2 == num, remainingTime);
		}

		private void SetNextWeekRemainingTime(bool isLastWeek, TimeSpan remainingTime)
		{
			string text;
			if (!isLastWeek)
			{
				if (remainingTime.Days > 0)
				{
					text = string.Format("{0} {1}", remainingTime.Days, Language.Get("GUI_TIME_DAYS", TranslationSheets.GUI));
				}
				else if (remainingTime.Hours > 0)
				{
					text = string.Format("{0} {1}", remainingTime.Hours, Language.Get("GUI_TIME_HOURS", TranslationSheets.GUI));
				}
				else if (remainingTime.Minutes > 0)
				{
					text = string.Format("{0} {1}", remainingTime.Minutes, Language.Get("GUI_TIME_MINUTES", TranslationSheets.GUI));
				}
				else
				{
					text = Language.Get("GUI_TIME_LESS_THEN_A_MINUTE", TranslationSheets.GUI);
				}
			}
			else
			{
				text = "-";
			}
			this._newMissionNumberLabel.text = text;
		}

		[SerializeField]
		private Color _highlightColor;

		[SerializeField]
		private HmmUiText _weekNumberLabel;

		[SerializeField]
		private HmmUiText _newMissionNumberLabel;
	}
}
