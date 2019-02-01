using System;
using System.Collections;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.EndMatch.Battlepass
{
	public class EndMatchBattlepassMissionSlot : MonoBehaviour
	{
		public bool IsCompleted()
		{
			return this._missionSlotData.IsCompleted;
		}

		public int GetXpAmount()
		{
			return this._missionSlotData.XpAmount;
		}

		public void Setup(EndMatchBattlepassMissionSlot.MissionSlotData slotData)
		{
			base.transform.Rotate(0f, 90f, 0f);
			base.GetComponent<NGUIWidgetAlpha>().alpha = 0f;
			this._checkMarkWidgetAlpha.gameObject.SetActive(slotData.IsCompleted);
			this._xpAmountLabel.text = slotData.XpAmount.ToString("0");
			this._progressLabel.text = string.Format("{0}/{1}", slotData.CurrentProgressAmount, slotData.ProgressMaxAmount);
			this._progressSliderTexture.fillAmount = 0f;
			this._nameLabel.text = slotData.NameText;
			this._descriptionLabel.text = slotData.DescriptionText;
			this._borderTexture.mainTexture = ((!slotData.IsPremium) ? ((!slotData.IsCompleted) ? this._freeBorderTexture : this._freeCompletedBorderTexture) : ((!slotData.IsCompleted) ? this._premiumBorderTexture : this._premiumCompletedBorderTexture));
			UIWidget boxXpTexture = this._boxXpTexture;
			Texture mainTexture = (!slotData.IsPremium) ? this._freeBoxXpTexture : this._premiumBoxXpTexture;
			this._boxXpAuraTexture.mainTexture = mainTexture;
			boxXpTexture.mainTexture = mainTexture;
			this._progressSliderTexture.mainTexture = ((!slotData.IsPremium) ? this._freeProgressBarTexture : this._premiumProgressBarTexture);
			this._rewardWidgetAlpha.alpha = ((!slotData.IsCompleted) ? this._incompleteRewardAlpha : this._completeRewardAlpha);
			this._ticketTexture.mainTexture = ((!slotData.IsPremium) ? this._freeTicketTexture : this._premiumTicketTexture);
			this._ticketTooltipTrigger.TooltipText = Language.Get((!slotData.IsPremium) ? "BATTLEPASS_REWARD_CATEGORY_FREE" : "BATTLEPASS_REWARD_CATEGORY_PREMIUM", TranslationSheets.Battlepass);
			this._missionSlotData = slotData;
		}

		public void PlayCardInAnimation()
		{
			this._missionCompletedAnimation.Play("mission_complete_card_in");
		}

		public IEnumerator ShowProgressBarAnimationCoroutine(float progressDurationTimeInSec)
		{
			if (this._missionSlotData.CurrentProgressAmount == 0)
			{
				yield break;
			}
			for (float timer = 0f; timer < progressDurationTimeInSec; timer += Time.deltaTime)
			{
				float normalizedTime = timer / progressDurationTimeInSec;
				float progressValue = Mathf.Lerp(0f, (float)this._missionSlotData.CurrentProgressAmount, normalizedTime);
				this._progressSliderTexture.fillAmount = progressValue / (float)this._missionSlotData.ProgressMaxAmount;
				yield return null;
			}
			this._progressSliderTexture.fillAmount = (float)this._missionSlotData.CurrentProgressAmount / (float)this._missionSlotData.ProgressMaxAmount;
			yield break;
		}

		public void PlayMissionCompletedAnimation()
		{
			this._missionCompletedAnimation.Play("mission_complete");
		}

		public void PlayRewardAnimation()
		{
			this._rewardAnimation.Play("mission_complete_reward_feedback");
		}

		public void Dispose()
		{
			base.StopAllCoroutines();
		}

		[Header("[GUI Components]")]
		[SerializeField]
		private NGUIWidgetAlpha _rewardWidgetAlpha;

		[SerializeField]
		private Animation _missionCompletedAnimation;

		[SerializeField]
		private Animation _rewardAnimation;

		[SerializeField]
		private UILabel _xpAmountLabel;

		[SerializeField]
		private UILabel _nameLabel;

		[SerializeField]
		private UILabel _descriptionLabel;

		[SerializeField]
		private UITexture _progressSliderTexture;

		[SerializeField]
		private UILabel _progressLabel;

		[SerializeField]
		private NGUIWidgetAlpha _checkMarkWidgetAlpha;

		[SerializeField]
		private UITexture _borderTexture;

		[SerializeField]
		private UITexture _boxXpTexture;

		[SerializeField]
		private UITexture _boxXpAuraTexture;

		[SerializeField]
		private UITexture _ticketTexture;

		[SerializeField]
		private HMMTooltipTrigger _ticketTooltipTrigger;

		[Header("[Assets]")]
		[SerializeField]
		private Texture _freeTicketTexture;

		[SerializeField]
		private Texture _premiumTicketTexture;

		[SerializeField]
		private Texture _freeProgressBarTexture;

		[SerializeField]
		private Texture _premiumProgressBarTexture;

		[SerializeField]
		private Texture _freeBoxXpTexture;

		[SerializeField]
		private Texture _premiumBoxXpTexture;

		[SerializeField]
		private Texture _freeBorderTexture;

		[SerializeField]
		private Texture _freeCompletedBorderTexture;

		[SerializeField]
		private Texture _premiumBorderTexture;

		[SerializeField]
		private Texture _premiumCompletedBorderTexture;

		[Range(0f, 1f)]
		[SerializeField]
		private float _completeRewardAlpha = 1f;

		[Range(0f, 1f)]
		[SerializeField]
		private float _incompleteRewardAlpha = 0.2f;

		private EndMatchBattlepassMissionSlot.MissionSlotData _missionSlotData;

		[Serializable]
		public struct MissionSlotData
		{
			public int MissionIndex;

			public bool IsPremium;

			public bool IsCompleted;

			public int XpAmount;

			public int CurrentProgressAmount;

			public int ProgressMaxAmount;

			public string NameText;

			public string DescriptionText;
		}
	}
}
