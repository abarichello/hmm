using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuProfileStatisticsSlot : MonoBehaviour
	{
		public void SetInfo(MainMenuProfileStatisticsSlot.StatisticsSlotInfo statisticsSlotInfo)
		{
			this.Show = statisticsSlotInfo.Show;
			this.IconSprite.sprite2D = statisticsSlotInfo.IconSprite;
			this.TitleLabel.text = statisticsSlotInfo.Title;
			this.TotalLabel.text = statisticsSlotInfo.Total.ToString("0");
			this.AverageLabel.text = statisticsSlotInfo.Average.ToString("0.0");
		}

		[SerializeField]
		protected bool Show;

		[SerializeField]
		protected UI2DSprite IconSprite;

		[SerializeField]
		protected UILabel TitleLabel;

		[SerializeField]
		protected UILabel TotalLabel;

		[SerializeField]
		protected UILabel AverageLabel;

		public struct StatisticsSlotInfo
		{
			public float GetAverage(int matchCount)
			{
				if (matchCount == 0)
				{
					return 0f;
				}
				return Convert.ToSingle(this.Total) / (float)matchCount;
			}

			public bool Show;

			public Sprite IconSprite;

			public string Title;

			public int Total;

			public float Average;
		}
	}
}
