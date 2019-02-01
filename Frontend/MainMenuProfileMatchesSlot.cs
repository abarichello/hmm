using System;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuProfileMatchesSlot : MonoBehaviour
	{
		public MatchData.MatchResult GetResultType()
		{
			return this.ResultType;
		}

		public void SetInfo(MainMenuProfileMatchesSlot.MatchSlotInfo matchSlotInfo)
		{
			this.CharacterIconSprite.SpriteName = matchSlotInfo.CharacterIconSpriteName;
			this.CharacterNameLabel.text = matchSlotInfo.CharacterNameText;
			this.GameModeIconSprite.SpriteName = matchSlotInfo.GameModeIconSpriteName;
			this.GameModeLabel.text = matchSlotInfo.GameModeName;
			this.DateLabel.text = matchSlotInfo.DateText;
			this.TimeLabel.text = matchSlotInfo.TimeText;
			this.ResultLabel.text = matchSlotInfo.ResultText;
			this.ResultLabel.color = matchSlotInfo.ResultColor;
			UIRect performanceIconSprite = this.PerformanceIconSprite;
			float performanceEnabledAlpha = this.PerformanceEnabledAlpha;
			this.PerformanceLabel.alpha = performanceEnabledAlpha;
			performanceIconSprite.alpha = performanceEnabledAlpha;
			if (!matchSlotInfo.HasPerformance)
			{
				this.PerformanceLabel.alpha = this.PerformanceDisabledLabelAlpha;
				this.PerformanceIconSprite.alpha = this.PerformanceDisabledSpriteAlpha;
			}
			this.PerformanceIconSprite.SpriteName = matchSlotInfo.PerformanceIconSpriteName;
			this.MatchDateTime = matchSlotInfo.MatchDateTime;
			this.arenaLabel.text = matchSlotInfo.ArenaName;
		}

		public int Compare(MainMenuProfileMatchesSlot otherSlot)
		{
			return -this.MatchDateTime.CompareTo(otherSlot.MatchDateTime);
		}

		public float PerformanceEnabledAlpha = 1f;

		public float PerformanceDisabledLabelAlpha = 0.14f;

		public float PerformanceDisabledSpriteAlpha = 0.23f;

		[SerializeField]
		protected MatchData.MatchResult ResultType;

		[SerializeField]
		protected HMMUI2DDynamicSprite CharacterIconSprite;

		[SerializeField]
		protected UILabel CharacterNameLabel;

		[SerializeField]
		protected HMMUI2DDynamicSprite PerformanceIconSprite;

		[SerializeField]
		protected UILabel PerformanceLabel;

		[SerializeField]
		protected HMMUI2DDynamicSprite GameModeIconSprite;

		[SerializeField]
		protected UILabel GameModeLabel;

		[SerializeField]
		protected UILabel DateLabel;

		[SerializeField]
		protected UILabel TimeLabel;

		[SerializeField]
		protected UILabel ResultLabel;

		protected DateTime MatchDateTime;

		[Header("Arena Info")]
		[SerializeField]
		protected UILabel arenaLabel;

		public struct MatchSlotInfo
		{
			public string CharacterIconSpriteName;

			public string CharacterNameText;

			public bool HasPerformance;

			public string PerformanceIconSpriteName;

			public string GameModeIconSpriteName;

			public string GameModeName;

			public string DateText;

			public string TimeText;

			public DateTime MatchDateTime;

			public string ResultText;

			public Color ResultColor;

			public string ArenaName;
		}
	}
}
