using System;
using System.Collections;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using Hoplon.Input.UiNavigation;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	internal class UnityUiBattlepassMissionsTabPresenter : MonoBehaviour
	{
		private IUiNavigationSubGroupHolder UiNavigationSubGroupHolder
		{
			get
			{
				return this._uiNavigationSubGroupHolder;
			}
		}

		private void OnEnable()
		{
			this._canvasGroup.alpha = 1f;
		}

		private void OnDisable()
		{
			this._canvasGroup.alpha = 0f;
		}

		public void Setup(IBattlepassBuyUiActions buyUiActions, BattlepassViewData battlepassViewData)
		{
			bool userHasPremium = battlepassViewData.DataSeason.UserHasPremium;
			MissionConfig mission = battlepassViewData.BattlepassConfig.Mission;
			BattlepassProgress battlepassProgress = battlepassViewData.BattlepassProgress;
			DateTime utcNow = battlepassViewData.DataTime.UtcNow;
			this._weekGroupPresenter.Setup(battlepassViewData);
			this._weekMissionsPresenter.Setup(buyUiActions, userHasPremium, mission, battlepassProgress, utcNow);
			this._listMissionsGroupPresenter.Setup(buyUiActions, userHasPremium, mission, battlepassProgress);
			this._togglePresenter.Setup(true, this._listMissionsGroupPresenter);
		}

		public void SetVisibility(bool isVisible, bool hasPremium)
		{
			base.StopAllCoroutines();
			base.gameObject.SetActive(true);
			this.SetUiNavigationFocus(isVisible);
			if (isVisible)
			{
				this._mainAnimation.Play("battlepass_transition_in");
				if (hasPremium)
				{
					this._weekMissionsPresenter.TryPremiumUnlock();
					this._togglePresenter.SelectActiveMissionsToggle();
					this._listMissionsGroupPresenter.TryPremiumUnlock();
				}
			}
			else
			{
				this._mainAnimation.Play("battlepass_transition_out");
				base.StartCoroutine(this.WaitForCloseAnimation(this._mainAnimation.GetClip("battlepass_transition_out").length));
			}
		}

		private IEnumerator WaitForCloseAnimation(float timeInSec)
		{
			yield return new WaitForSeconds(timeInSec + Time.deltaTime);
			base.gameObject.SetActive(false);
			yield break;
		}

		private void SetUiNavigationFocus(bool focused)
		{
			if (focused)
			{
				this.UiNavigationSubGroupHolder.SubGroupFocusGet();
			}
			else
			{
				this.UiNavigationSubGroupHolder.SubGroupFocusRelease();
			}
		}

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private Animation _mainAnimation;

		[SerializeField]
		private UnityUiBattlepassMissionsWeekGroupPresenter _weekGroupPresenter;

		[SerializeField]
		private UnityUiBattlepassMissionsWeekMissionsPresenter _weekMissionsPresenter;

		[SerializeField]
		private UnityUiBattlepassMissionsListMissionsGroupPresenter _listMissionsGroupPresenter;

		[SerializeField]
		private UnityUiBattlepassMissionsTogglePresenter _togglePresenter;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;
	}
}
