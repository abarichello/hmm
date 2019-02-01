using System;
using System.Collections;
using Commons.Swordfish.Battlepass;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.UnityUI;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	internal class UnityUiBattlepassMissionsWeekMissionsPresenter : MonoBehaviour
	{
		public void Setup(IBattlepassBuyUiActions buyUiActions, bool userHasPremium, MissionConfig missionConfig, BattlepassProgress progress, DateTime utcNow)
		{
			if (!this._tryToShowUnlockPremiumAnimation && !userHasPremium)
			{
				this._tryToShowUnlockPremiumAnimation = true;
			}
			this._buyUiActions = buyUiActions;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < missionConfig.Missions.Length; i++)
			{
				Mission mission = missionConfig.Missions[i];
				if (mission.GetStartDate() <= utcNow && !progress.IsDoingMission(i) && i < progress.MissionsCompleted.Length && !progress.MissionsCompleted[i])
				{
					if (mission.IsPremium > 0)
					{
						num2++;
					}
					else
					{
						num++;
					}
				}
			}
			this.SetupGroupInfo(this._freeNumberMissionLabel, this._freeMissionAnimation, num);
			this.SetupGroupInfo(this._premiumNumberMissionLabel, this._premiumMissionAnimation, num2);
			this.UpdatePremiumState(userHasPremium);
		}

		private void UpdatePremiumState(bool userHasPremium)
		{
			if (userHasPremium)
			{
				this._premiumMissionCanvasGroup.transform.localScale = this._premiumUnlockedCanvasGroupSize;
				this._premiumMissionCanvasGroup.alpha = this._premiumUnlockedCanvasGroupAlpha;
				this._buyPlusGroup.SetActive(false);
			}
			else
			{
				this._premiumMissionCanvasGroup.transform.localScale = this._premiumLockedCanvasGroupSize;
				this._premiumMissionCanvasGroup.alpha = this._premiumLockedCanvasGroupAlpha;
				this._buyPlusGroup.SetActive(true);
			}
		}

		private void SetupGroupInfo(HmmUiText numberMissionLabel, Animation missionAnimation, int missionCount)
		{
			numberMissionLabel.text = missionCount.ToString("0");
			if (missionCount > 0)
			{
				missionAnimation.Play();
			}
			else
			{
				missionAnimation.Stop();
			}
		}

		[UnityUiComponentCall]
		public void OnFreeGroupHover()
		{
			this.TryToStopAnimation(this._freeMissionAnimation);
		}

		[UnityUiComponentCall]
		public void OnPremiumGroupHover()
		{
			this.TryToStopAnimation(this._premiumMissionAnimation);
		}

		private void TryToStopAnimation(Animation toStopAnimation)
		{
			if (toStopAnimation.isPlaying)
			{
				toStopAnimation.Rewind();
				toStopAnimation.Play();
				toStopAnimation.Sample();
				toStopAnimation.Stop();
			}
		}

		[UnityUiComponentCall]
		public void OnUnlockPremiumClick()
		{
			this._rewardsToggleInfo.SetToggleValue(true);
			this._buyUiActions.OnUnlockPremiumButtonClick(true);
		}

		public void TryPremiumUnlock()
		{
			if (this._tryToShowUnlockPremiumAnimation)
			{
				this._tryToShowUnlockPremiumAnimation = false;
				this._premiumUnlockAnimation.Play();
				base.StartCoroutine(this.WaitPremiumUnlockAnimation(this._premiumUnlockAnimation.clip.length));
			}
		}

		private IEnumerator WaitPremiumUnlockAnimation(float delayInSec)
		{
			yield return new WaitForSeconds(delayInSec);
			this.UpdatePremiumState(true);
			yield break;
		}

		protected void OnDisable()
		{
			base.StopAllCoroutines();
		}

		[SerializeField]
		private HmmUiText _freeNumberMissionLabel;

		[SerializeField]
		private Animation _freeMissionAnimation;

		[SerializeField]
		private HmmUiText _premiumNumberMissionLabel;

		[SerializeField]
		private Animation _premiumMissionAnimation;

		[SerializeField]
		private CanvasGroup _premiumMissionCanvasGroup;

		[SerializeField]
		private GameObject _buyPlusGroup;

		[SerializeField]
		private UnityUiToggleInfo _rewardsToggleInfo;

		[SerializeField]
		private Animation _premiumUnlockAnimation;

		[Header("[Configs]")]
		[SerializeField]
		private Vector3 _premiumLockedCanvasGroupSize = new Vector3(0.9f, 0.9f, 0.9f);

		[SerializeField]
		private float _premiumLockedCanvasGroupAlpha = 0.35f;

		[SerializeField]
		private Vector3 _premiumUnlockedCanvasGroupSize = Vector3.one;

		[SerializeField]
		private float _premiumUnlockedCanvasGroupAlpha = 1f;

		private IBattlepassBuyUiActions _buyUiActions;

		private bool _tryToShowUnlockPremiumAnimation;
	}
}
