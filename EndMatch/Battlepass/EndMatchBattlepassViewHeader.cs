using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.GameStates.MainMenu.Progression;
using FMod;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.EndMatch.Battlepass
{
	public class EndMatchBattlepassViewHeader : GameHubBehaviour
	{
		public int GetTotalXpGain()
		{
			return this._totalXpGain;
		}

		public void Setup(EndMatchBattlepassViewHeader.HeaderData headerData, IAnimationSync animationSync)
		{
			int num = (headerData.OldLevel + 1 >= headerData.MaxXpPerLevel.Count) ? headerData.OldLevel : (headerData.OldLevel + 1);
			bool flag = headerData.OldLevel >= headerData.MaxXpPerLevel.Count - 1;
			this._nextLevelLabel.transform.parent.gameObject.SetActive(!flag);
			this._maxLevelLabel.gameObject.SetActive(flag);
			this._currentLevelLabel.text = (headerData.OldLevel + 1).ToString("0");
			this._nextLevelLabel.text = (num + 1).ToString("0");
			float num2 = 1f;
			if (flag)
			{
				this._levelProgressLabel.text = string.Format("{0} / {1}", headerData.MaxXpPerLevel[num], headerData.MaxXpPerLevel[num]);
			}
			else
			{
				this._levelProgressLabel.text = string.Format("{0} / {1}", headerData.OldLevelProgressXp, headerData.MaxXpPerLevel[num]);
				num2 = (float)headerData.OldLevelProgressXp / (float)headerData.MaxXpPerLevel[num];
			}
			this._totalRewardXpLabel.text = "0";
			this._levelProgressTexture.width = (int)Mathf.Lerp((float)this._levelProgressMin, (float)this._levelProgressMax, num2);
			this.ResetTransfusionComponents();
			this._headerBonusesGrid.hideInactive = false;
			List<Transform> childList = this._headerBonusesGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				childList[i].GetComponent<EndMatchBattlepassHeaderBonusSlot>().Setup(headerData);
			}
			this._headerBonusesGrid.hideInactive = true;
			this._headerBonusesGrid.Reposition();
			this._rewardGameObject.SetActive(!headerData.IsPlayerLeaver);
			this._progressBarTransform.localPosition = new Vector3((float)((!headerData.IsPlayerLeaver) ? this._progressBarDefaultPosition : this._progressBarCenteredPosition), 0f, 0f);
			this._separatorGameObject.SetActive(!headerData.IsPlayerLeaver);
			this._rewardListGameObject.SetActive(!headerData.IsPlayerLeaver);
			GUIUtils.AnimationSetFirstFrame(this._hideTotalXpAnimation);
			this._totalXpGain = 0;
			this._headerData = headerData;
			this._animationSync = animationSync;
		}

		public IEnumerator PlayMissionXpAnimationCoroutine(int xpValue, float durationInSec, float delayAfterBonusInSec)
		{
			List<Transform> headerBonuses = this._headerBonusesGrid.GetChildList();
			for (int i = 0; i < headerBonuses.Count; i++)
			{
				EndMatchBattlepassHeaderBonusSlot bonusSlot = headerBonuses[i].GetComponent<EndMatchBattlepassHeaderBonusSlot>();
				if (bonusSlot.GetBonusType() == EndMatchBattlepassHeaderBonusSlot.HeaderBonusType.Missions)
				{
					base.StartCoroutine(this.CountXp(this._totalXpGain, this._totalXpGain + xpValue, durationInSec, bonusSlot.GetXpLabel(), this._xpAccumulationAsset, "{0}", -1));
					yield return base.StartCoroutine(this.CountXp(this._totalXpGain, this._totalXpGain + xpValue, durationInSec, this._totalRewardXpLabel, this._xpAccumulationAsset, "{0}", -1));
					this._totalXpGain += xpValue;
					bonusSlot.PlayGlowAnimation();
					yield return new WaitForSeconds(delayAfterBonusInSec);
				}
			}
			yield break;
		}

		public IEnumerator PlayBonusXpRewardAnimationCoroutine(float durationInSec, float delayAfterBonusInSec)
		{
			List<Transform> headerBonuses = this._headerBonusesGrid.GetChildList();
			for (int i = 0; i < headerBonuses.Count; i++)
			{
				EndMatchBattlepassHeaderBonusSlot bonusSlot = headerBonuses[i].GetComponent<EndMatchBattlepassHeaderBonusSlot>();
				if (bonusSlot.GetBonusType() != EndMatchBattlepassHeaderBonusSlot.HeaderBonusType.Missions)
				{
					yield return base.StartCoroutine(this.ShowBonusCoroutine(bonusSlot, durationInSec));
					if (i + 1 < headerBonuses.Count)
					{
						yield return new WaitForSeconds(delayAfterBonusInSec);
					}
				}
			}
			this._totalXpAnimation.Play("mission_complete_xp_feedback");
			yield break;
		}

		private IEnumerator ShowBonusCoroutine(EndMatchBattlepassHeaderBonusSlot bonusSlot, float durationInSec)
		{
			int amountXp = bonusSlot.GetBonusXp();
			base.StartCoroutine(this.CountXp(0, amountXp, durationInSec, bonusSlot.GetXpLabel(), this._xpAccumulationAsset, "{0}", -1));
			yield return base.StartCoroutine(this.CountXp(this._totalXpGain, this._totalXpGain + amountXp, durationInSec, this._totalRewardXpLabel, this._xpAccumulationAsset, "{0}", -1));
			this._totalXpGain += amountXp;
			bonusSlot.PlayGlowAnimation();
			yield break;
		}

		public IEnumerator PlayRewardTransferAnimationCoroutine(float timeInSec, EndMatchBattlepassViewHeader.OnLevelUpDetected onLevelUpDetected)
		{
			int currentLevelProgressXp = this._headerData.OldLevelProgressXp;
			bool isMaxLevel = this._headerData.OldLevel >= this._headerData.MaxXpPerLevel.Count - 1;
			int nextLevel = (this._headerData.OldLevel + 1 >= this._headerData.MaxXpPerLevel.Count) ? this._headerData.OldLevel : (this._headerData.OldLevel + 1);
			int currentLevelProgressMaxXp = this._headerData.MaxXpPerLevel[nextLevel];
			if (isMaxLevel)
			{
				yield break;
			}
			base.StartCoroutine(this.CountXp(currentLevelProgressXp, currentLevelProgressXp + this._totalXpGain, timeInSec, this._levelProgressLabel, this._xpBarProgressAsset, "{0} / " + currentLevelProgressMaxXp, currentLevelProgressMaxXp));
			yield return base.StartCoroutine(this.CountProgressXp(this._headerData, this._totalXpGain, timeInSec, onLevelUpDetected));
			this.ResetTransfusionComponents();
			yield break;
		}

		private void ResetTransfusionComponents()
		{
			this._transfusionAnimation.Stop();
			NGUIWidgetAlpha[] componentsInChildren = this._transfusionAnimation.GetComponentsInChildren<NGUIWidgetAlpha>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Alpha = 0f;
			}
		}

		private IEnumerator CountXp(int startXp, int targetXp, float duration, UILabel label, AudioEventAsset asset, string formatedString = "{0}", int maxCapXp = -1)
		{
			if (this._audioToken != null && !this._audioToken.IsInvalidated())
			{
				this._audioToken.Stop();
			}
			this._audioToken = FMODAudioManager.PlayAtVolume(asset, base.transform, 1f, true);
			for (float timer = 0f; timer < duration; timer += Time.deltaTime)
			{
				float progress = timer / duration;
				int progressXp = (int)Mathf.Lerp((float)startXp, (float)targetXp, progress);
				label.text = string.Format(formatedString, progressXp);
				if (maxCapXp != -1 && progressXp >= maxCapXp)
				{
					label.text = string.Format(formatedString, maxCapXp);
					if (this._audioToken != null && !this._audioToken.IsInvalidated())
					{
						this._audioToken.Stop();
					}
					yield break;
				}
				yield return null;
			}
			if (maxCapXp != -1 && targetXp > maxCapXp)
			{
				targetXp = maxCapXp;
			}
			label.text = string.Format(formatedString, targetXp);
			if (this._audioToken != null && !this._audioToken.IsInvalidated())
			{
				this._audioToken.Stop();
			}
			yield break;
		}

		private IEnumerator CountProgressXp(EndMatchBattlepassViewHeader.HeaderData headerData, int totalXpGain, float duration, EndMatchBattlepassViewHeader.OnLevelUpDetected onLevelUpDetected)
		{
			this._transfusionAnimation.Play("mission_complete_transfusion_animation");
			int currentLevel = headerData.OldLevel;
			int nextLevel = (currentLevel + 1 >= headerData.MaxXpPerLevel.Count) ? currentLevel : (currentLevel + 1);
			int currentLevelMaxXp = headerData.MaxXpPerLevel[nextLevel];
			int start = headerData.OldLevelProgressXp;
			int targetXp = headerData.OldLevelProgressXp + totalXpGain;
			bool targetMaxOverflow = targetXp >= currentLevelMaxXp;
			float timer = 0f;
			while (timer < duration || targetMaxOverflow)
			{
				float normalizedTime = timer / duration;
				float progressXp = Mathf.Lerp((float)start, (float)targetXp, normalizedTime);
				this._levelProgressTexture.width = (int)Mathf.Lerp((float)this._levelProgressMin, (float)this._levelProgressMax, progressXp / (float)currentLevelMaxXp);
				int totalProgressXp = (int)Mathf.Lerp((float)totalXpGain, 0f, normalizedTime);
				this._totalRewardXpLabel.text = totalProgressXp.ToString("0");
				if (progressXp >= (float)currentLevelMaxXp)
				{
					targetXp = totalXpGain - (currentLevelMaxXp - start);
					totalXpGain = targetXp;
					this._totalRewardXpLabel.text = totalXpGain.ToString("0");
					currentLevel++;
					nextLevel++;
					this.ResetTransfusionComponents();
					onLevelUpDetected(currentLevel);
					if (this._animationSync != null)
					{
						yield return new WaitForSeconds(this._animationSync.GetWaitTimeToPlay());
					}
					this._levelUpAnimation.Play("mission_complete_level_up");
					yield return new WaitForSeconds(this._levelUpAnimation.GetClip("mission_complete_level_up").length - this._levelUpEarlyTimeInSec);
					this._currentLevelLabel.text = (currentLevel + 1).ToString("0");
					if (currentLevel + 1 >= headerData.MaxXpPerLevel.Count)
					{
						this._nextLevelLabel.transform.parent.gameObject.SetActive(false);
						this._nextLevelGroup.gameObject.SetActive(false);
						this._maxLevelLabel.gameObject.SetActive(true);
						GUIUtils.PlayAnimation(this._hideTotalXpAnimation, false, 1f, string.Empty);
						yield break;
					}
					this._transfusionAnimation.Play("mission_complete_transfusion_animation");
					currentLevelMaxXp = headerData.MaxXpPerLevel[nextLevel];
					targetMaxOverflow = (targetXp >= currentLevelMaxXp);
					start = 0;
					this._nextLevelLabel.text = (nextLevel + 1).ToString("0");
					duration -= timer;
					base.StartCoroutine(this.CountXp(0, targetXp, duration, this._levelProgressLabel, this._xpBarProgressAsset, "{0} / " + currentLevelMaxXp, currentLevelMaxXp));
					timer = 0f;
				}
				yield return null;
				timer += Time.deltaTime;
			}
			this._totalRewardXpLabel.text = "0";
			this._levelProgressTexture.width = (int)Mathf.Lerp((float)this._levelProgressMin, (float)this._levelProgressMax, (float)targetXp / (float)currentLevelMaxXp);
			GUIUtils.PlayAnimation(this._hideTotalXpAnimation, false, 1f, string.Empty);
			yield break;
		}

		public void Dispose()
		{
			base.StopAllCoroutines();
			if (this._audioToken != null && !this._audioToken.IsInvalidated())
			{
				this._audioToken.Stop();
			}
		}

		[Header("[Main Components]")]
		[SerializeField]
		private GameObject _rewardGameObject;

		[SerializeField]
		private Transform _progressBarTransform;

		[SerializeField]
		private int _progressBarDefaultPosition = 70;

		[SerializeField]
		private int _progressBarCenteredPosition = -158;

		[SerializeField]
		private GameObject _separatorGameObject;

		[SerializeField]
		private GameObject _rewardListGameObject;

		[Header("[Level Info]")]
		[SerializeField]
		private Animation _levelUpAnimation;

		[SerializeField]
		private UILabel _currentLevelLabel;

		[SerializeField]
		private UILabel _levelProgressLabel;

		[SerializeField]
		private UILabel _nextLevelLabel;

		[SerializeField]
		private UILabel _maxLevelLabel;

		[SerializeField]
		private GameObject _nextLevelGroup;

		[SerializeField]
		private UITexture _levelProgressTexture;

		[SerializeField]
		private int _levelProgressMin = 40;

		[SerializeField]
		private int _levelProgressMax = 266;

		[Header("[Total Reward]")]
		[SerializeField]
		private UILabel _totalRewardXpLabel;

		[SerializeField]
		private Animation _totalXpAnimation;

		[SerializeField]
		private Animation _transfusionAnimation;

		[SerializeField]
		private Animation _hideTotalXpAnimation;

		[Header("[Bonus]")]
		[SerializeField]
		private UIGrid _headerBonusesGrid;

		[Header("[Config]")]
		[SerializeField]
		private float _levelUpEarlyTimeInSec = 0.2f;

		[Header("[Audio]")]
		[SerializeField]
		private AudioEventAsset _xpAccumulationAsset;

		[SerializeField]
		private AudioEventAsset _xpBarProgressAsset;

		private FMODAudioManager.FMODAudio _audioToken;

		private EndMatchBattlepassViewHeader.HeaderData _headerData;

		private int _totalXpGain;

		private IAnimationSync _animationSync;

		public delegate void OnLevelUpDetected(int level);

		[Serializable]
		public struct HeaderData
		{
			public bool IsPlayerLeaver;

			public int OldLevel;

			public int OldLevelProgressXp;

			public List<int> MaxXpPerLevel;

			[HideInInspector]
			public bool HasMissionCompleted;

			public int MatchBonusXp;

			public int PerformanceBonusXp;

			public int EventBonusXp;

			public int FoundersBonusXp;

			public int BoosterBonusXp;
		}
	}
}
