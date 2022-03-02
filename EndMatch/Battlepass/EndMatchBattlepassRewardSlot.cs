using System;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.EndMatch.Battlepass
{
	public class EndMatchBattlepassRewardSlot : MonoBehaviour
	{
		public void InitialSetup(EndMatchBattlepassLevelUpWindow.LevelUpData data, EndMatchBattlepassLevelUpWindow.LevelViewInfo[] levelInfo, EndMatchBattlepassLevelUpWindow.RewardViewBorderInfo borderInfo, EndMatchBattlepassLevelUpWindow.AlphaImageConfig alpha, EndMatchBattlepassLevelUpWindow.RewardLocksConfig locksConfig)
		{
			this._levelInfo = levelInfo;
			this._borderInfo = borderInfo;
			this._alphaImageConfig = alpha;
			this._rewardLocksConfig = locksConfig;
			this.Setup(data);
		}

		public void Setup(EndMatchBattlepassLevelUpWindow.LevelUpData data)
		{
			this._level = data.Level;
			this.FillLockInfo(data);
			this.FillBorderInfo(data.SlotLevelType, data.IsPremium);
			this.FillLevelsData(data);
			this._premiumRewards = this.UpdateInfoByRewardKind(this._premiumRewards, data.PremiumSlot, data.SlotLevelType);
			this._freeRewards = this.UpdateInfoByRewardKind(this._freeRewards, data.FreeSlot, data.SlotLevelType);
		}

		private EndMatchBattlepassRewardSlot.RewardSlots UpdateInfoByRewardKind(EndMatchBattlepassRewardSlot.RewardSlots slots, EndMatchBattlepassLevelUpWindow.LevelUpDataSlot data, EndMatchBattlepassLevelUpWindow.SlotLevelType slotType)
		{
			slots.IconImage.SpriteName = data.RewardIcon;
			slots.rewardKind = data.RewardKind;
			slots.FameText.gameObject.SetActive(false);
			switch (slots.rewardKind)
			{
			case 0:
				slots.Border.color = this._borderInfo.UnlockedColorWithoutItem;
				slots.LockImage.gameObject.SetActive(false);
				break;
			case 1:
				slots.FameText.gameObject.SetActive(true);
				slots.FameText.color = ((slotType == EndMatchBattlepassLevelUpWindow.SlotLevelType.Locked) ? this._alphaImageConfig.FamaColorDisabled : this._alphaImageConfig.FamaColorEnabled);
				slots.FameText.effectColor = this._alphaImageConfig.FamaOutlineColor;
				slots.FameText.text = data.FameAmount.ToString();
				break;
			case 4:
				slots.FameText.gameObject.SetActive(true);
				slots.FameText.color = ((slotType == EndMatchBattlepassLevelUpWindow.SlotLevelType.Locked) ? this._alphaImageConfig.HardCoinColorDisabled : this._alphaImageConfig.HardCointColorEnabled);
				slots.FameText.effectColor = this._alphaImageConfig.HardCoinOutlineColor;
				slots.FameText.text = data.FameAmount.ToString();
				break;
			}
			return slots;
		}

		private void FillLockInfo(EndMatchBattlepassLevelUpWindow.LevelUpData data)
		{
			if (data.SlotLevelType != EndMatchBattlepassLevelUpWindow.SlotLevelType.Locked)
			{
				this._freeRewards.LockImage.gameObject.SetActive(false);
				if (data.IsPremium)
				{
					this._premiumRewards.LockImage.gameObject.SetActive(false);
				}
				else
				{
					this._premiumRewards.LockImage.widget.mainTexture = this._rewardLocksConfig.PremiumLockedTexture;
				}
				return;
			}
			this._freeRewards.LockImage.Alpha = 1f;
			this._premiumRewards.LockImage.Alpha = 1f;
			this._freeRewards.LockImage.gameObject.SetActive(true);
			this._premiumRewards.LockImage.gameObject.SetActive(true);
			this._freeRewards.LockImage.widget.mainTexture = this._rewardLocksConfig.FreeTexture;
			this._premiumRewards.LockImage.widget.mainTexture = this._rewardLocksConfig.PremiumUnlockableTexture;
		}

		private void FillBorderInfo(EndMatchBattlepassLevelUpWindow.SlotLevelType data, bool isPremium)
		{
			if (data != EndMatchBattlepassLevelUpWindow.SlotLevelType.Locked)
			{
				if (data == EndMatchBattlepassLevelUpWindow.SlotLevelType.Current || data == EndMatchBattlepassLevelUpWindow.SlotLevelType.Unlocked)
				{
					this._freeRewards.IconImage.alpha = this._alphaImageConfig.EnabledIconImageAlpha;
					this._freeRewards.Border.mainTexture = this._borderInfo.UnlockedFreeSprite;
					this._freeRewards.Border.color = this._borderInfo.UnlockedFreeColor;
					this._premiumRewards.IconImage.alpha = ((!isPremium) ? this._alphaImageConfig.DisabledIconImageAlpha : this._alphaImageConfig.EnabledIconImageAlpha);
					this._premiumRewards.Border.mainTexture = ((!isPremium) ? this._borderInfo.LockedPremiumSprite : this._borderInfo.UnlockedPremiumSprite);
					this._premiumRewards.Border.color = ((!isPremium) ? this._borderInfo.LockedPremiumColor : this._borderInfo.UnlockedPremiumColor);
				}
			}
			else
			{
				this._freeRewards.IconImage.alpha = this._alphaImageConfig.DisabledIconImageAlpha;
				this._premiumRewards.IconImage.alpha = this._alphaImageConfig.DisabledIconImageAlpha;
				this._freeRewards.Border.mainTexture = this._borderInfo.LockedFreeSprite;
				this._freeRewards.Border.color = this._borderInfo.LockedFreeColor;
				this._premiumRewards.Border.mainTexture = this._borderInfo.LockedPremiumSprite;
				this._premiumRewards.Border.color = this._borderInfo.LockedPremiumColor;
			}
		}

		private void FillLevelsData(EndMatchBattlepassLevelUpWindow.LevelUpData data)
		{
			int slotLevelType = (int)data.SlotLevelType;
			this._levels.LevelRawImage.mainTexture = this._levelInfo[slotLevelType].BgTexture;
			this._levels.LevelText.fontSize = this._levelInfo[slotLevelType].FontSize;
			this._levels.LevelText.color = this._levelInfo[slotLevelType].TextColor;
			this._levels.LevelText.effectStyle = this._levelInfo[slotLevelType].EffectOutline;
			this._levels.LevelText.text = (data.Level + 1).ToString();
		}

		public void UpdateLevelsData(EndMatchBattlepassLevelUpWindow.SlotLevelType data, EndMatchBattlepassLevelUpWindow.LevelViewInfo[] levelInfo)
		{
			this._levels.LevelRawImage.mainTexture = levelInfo[(int)data].BgTexture;
			this._levels.LevelText.fontSize = levelInfo[(int)data].FontSize;
			this._levels.LevelText.color = levelInfo[(int)data].TextColor;
			this._levels.LevelText.effectStyle = levelInfo[(int)data].EffectOutline;
		}

		public void PlayUnlockRewardAnimation(bool isPremium)
		{
			if (this._freeRewards.rewardKind != null)
			{
				this._freeRewards.UnlockReward.Play("LevelUp_UnlockReward");
			}
			if (isPremium)
			{
				this._premiumRewards.UnlockReward.Play("LevelUp_UnlockReward");
			}
			else
			{
				this._premiumRewards.UnlockReward.Play("unlock_transition");
				this._premiumRewards.LockImage.widget.mainTexture = this._rewardLocksConfig.PremiumLockedTexture;
			}
		}

		public void PlayLevelGlowAnimation()
		{
			this._levels.LevelAnimation.Play("level_transition");
		}

		public void UpdateSlotInfo(bool isPremium)
		{
			this._freeRewards.IconImage.alpha = this._alphaImageConfig.EnabledIconImageAlpha;
			this._freeRewards.Border.mainTexture = this._borderInfo.UnlockedFreeSprite;
			if (this._freeRewards.rewardKind == null)
			{
				this._freeRewards.Border.color = this._borderInfo.UnlockedColorWithoutItem;
			}
			else
			{
				this._freeRewards.Border.color = this._borderInfo.UnlockedFreeColor;
			}
			if (!isPremium)
			{
				return;
			}
			this._premiumRewards.IconImage.alpha = this._alphaImageConfig.EnabledIconImageAlpha;
			this._premiumRewards.Border.mainTexture = this._borderInfo.UnlockedPremiumSprite;
			if (this._premiumRewards.rewardKind == null)
			{
				this._premiumRewards.Border.color = this._borderInfo.UnlockedColorWithoutItem;
			}
			else
			{
				this._premiumRewards.Border.color = this._borderInfo.UnlockedPremiumColor;
			}
		}

		public void EnableColorFameText()
		{
			switch (this._freeRewards.rewardKind)
			{
			case 0:
			case 2:
			case 3:
				break;
			case 1:
				this._freeRewards.FameText.color = this._alphaImageConfig.FamaColorEnabled;
				this._freeRewards.FameText.effectColor = this._alphaImageConfig.FamaOutlineColor;
				break;
			case 4:
				this._freeRewards.FameText.color = this._alphaImageConfig.HardCointColorEnabled;
				this._freeRewards.FameText.effectColor = this._alphaImageConfig.HardCoinOutlineColor;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			switch (this._premiumRewards.rewardKind)
			{
			case 0:
			case 2:
			case 3:
				break;
			case 1:
				this._premiumRewards.FameText.color = this._alphaImageConfig.FamaColorEnabled;
				this._premiumRewards.FameText.effectColor = this._alphaImageConfig.FamaOutlineColor;
				break;
			case 4:
				this._premiumRewards.FameText.color = this._alphaImageConfig.HardCointColorEnabled;
				this._premiumRewards.FameText.effectColor = this._alphaImageConfig.HardCoinOutlineColor;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public int GetSlotLevel()
		{
			return this._level;
		}

		public EndMatchBattlepassRewardSlot.RewardSlots GetFreeSlots()
		{
			return this._freeRewards;
		}

		public EndMatchBattlepassRewardSlot.RewardSlots GetPremiumSlots()
		{
			return this._premiumRewards;
		}

		private int _level;

		[SerializeField]
		private EndMatchBattlepassRewardSlot.RewardSlots _freeRewards;

		[SerializeField]
		private EndMatchBattlepassRewardSlot.RewardSlots _premiumRewards;

		[SerializeField]
		private EndMatchBattlepassRewardSlot.RewardViewLevel _levels;

		private EndMatchBattlepassLevelUpWindow.LevelViewInfo[] _levelInfo;

		private EndMatchBattlepassLevelUpWindow.RewardViewBorderInfo _borderInfo;

		private EndMatchBattlepassLevelUpWindow.AlphaImageConfig _alphaImageConfig;

		private EndMatchBattlepassLevelUpWindow.RewardLocksConfig _rewardLocksConfig;

		[Serializable]
		public struct RewardSlots
		{
			[Header("[Configs]")]
			public HMMUI2DDynamicSprite IconImage;

			public UITexture Border;

			public NGUIWidgetAlpha LockImage;

			public UILabel FameText;

			public Animation UnlockReward;

			[HideInInspector]
			public ProgressionInfo.RewardKind rewardKind;
		}

		[Serializable]
		private struct RewardViewLevel
		{
			[Header("[UI Components]")]
			public UITexture LevelRawImage;

			public UILabel LevelText;

			public Animation LevelAnimation;
		}
	}
}
