using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.EndMatch.Battlepass
{
	public class EndMatchBattlepassHeaderBonusSlot : GameHubBehaviour
	{
		public void Setup(EndMatchBattlepassViewHeader.HeaderData headerData)
		{
			this._xpLabel.text = string.Empty;
			switch (this._bonusType)
			{
			case EndMatchBattlepassHeaderBonusSlot.HeaderBonusType.Missions:
				this._bonusXp = ((!headerData.HasMissionCompleted) ? 0 : 1);
				break;
			case EndMatchBattlepassHeaderBonusSlot.HeaderBonusType.Match:
				this._bonusXp = headerData.MatchBonusXp;
				break;
			case EndMatchBattlepassHeaderBonusSlot.HeaderBonusType.Performance:
				this._bonusXp = headerData.PerformanceBonusXp;
				break;
			case EndMatchBattlepassHeaderBonusSlot.HeaderBonusType.Event:
				this._bonusXp = headerData.EventBonusXp;
				break;
			case EndMatchBattlepassHeaderBonusSlot.HeaderBonusType.Founders:
				this._bonusXp = headerData.FoundersBonusXp;
				break;
			case EndMatchBattlepassHeaderBonusSlot.HeaderBonusType.Booster:
				this._bonusXp = headerData.BoosterBonusXp;
				base.gameObject.SetActive(true);
				return;
			}
			base.gameObject.SetActive(this._bonusXp > 0);
		}

		public EndMatchBattlepassHeaderBonusSlot.HeaderBonusType GetBonusType()
		{
			return this._bonusType;
		}

		public UILabel GetXpLabel()
		{
			return this._xpLabel;
		}

		public int GetBonusXp()
		{
			return this._bonusXp;
		}

		public void PlayGlowAnimation()
		{
			this._glowAnimation.Play();
		}

		[SerializeField]
		private EndMatchBattlepassHeaderBonusSlot.HeaderBonusType _bonusType;

		[SerializeField]
		private Animation _glowAnimation;

		[SerializeField]
		private UILabel _xpLabel;

		private int _bonusXp;

		public enum HeaderBonusType
		{
			Missions,
			Match,
			Performance,
			Event,
			Founders,
			Booster
		}
	}
}
