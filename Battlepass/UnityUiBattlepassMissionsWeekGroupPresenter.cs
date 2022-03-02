using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	internal class UnityUiBattlepassMissionsWeekGroupPresenter : MonoBehaviour
	{
		public void Setup(BattlepassViewData battlepassViewData)
		{
			DateTime endDateUtc = battlepassViewData.DataTime.EndDateUtc;
			DateTime startDateUtc = battlepassViewData.DataTime.StartDateUtc;
			DateTime utcNow = battlepassViewData.DataTime.UtcNow;
			int num = Mathf.CeilToInt((float)(endDateUtc - startDateUtc).TotalDays / 7f);
			float num2 = (float)(utcNow - startDateUtc).TotalDays / 7f;
			int num3 = Mathf.CeilToInt(num2);
			num3 = Mathf.Clamp(num3, 1, num);
			DateTime d = startDateUtc + new TimeSpan(num3 * 7, 0, 0, 0);
			TimeSpan remainingTime = d - utcNow;
			this._weekNumberLabel.text = string.Format("{0} <color=#{2}>/ {1}</color>", num3, num, HudUtils.RGBToHex(this._highlightColor));
			this.SetNextWeekRemainingTime(num3 == num, remainingTime);
		}

		private void SetNextWeekRemainingTime(bool isLastWeek, TimeSpan remainingTime)
		{
			string text;
			if (!isLastWeek)
			{
				if (remainingTime.Days > 0)
				{
					text = string.Format("{0} {1}", remainingTime.Days, Language.Get("GUI_TIME_DAYS", TranslationContext.GUI));
				}
				else if (remainingTime.Hours > 0)
				{
					text = string.Format("{0} {1}", remainingTime.Hours, Language.Get("GUI_TIME_HOURS", TranslationContext.GUI));
				}
				else if (remainingTime.Minutes > 0)
				{
					text = string.Format("{0} {1}", remainingTime.Minutes, Language.Get("GUI_TIME_MINUTES", TranslationContext.GUI));
				}
				else
				{
					text = Language.Get("GUI_TIME_LESS_THEN_A_MINUTE", TranslationContext.GUI);
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
