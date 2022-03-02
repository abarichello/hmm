using System;
using System.Collections;
using System.Diagnostics;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Utils;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ShopCashGUILoadFeedback : BaseMonoBehaviour
	{
		public void SetReadyFeedbackAction(Action showReadyFeedback)
		{
			this.ShowReadyFeedback = showReadyFeedback;
		}

		public ShopCashGUILoadFeedbackState GetState()
		{
			return this.currState;
		}

		public void SetState(ShopCashGUILoadFeedbackState state)
		{
			this.currState = state;
		}

		public void ShowFeedback()
		{
			ShopCashGUILoadFeedback.Log.DebugFormat("Show cash shop feedback for state {0}", new object[]
			{
				this.currState
			});
			this.StopWaitMinimumDisplayingTimeRoutine();
			if (this.currState == ShopCashGUILoadFeedbackState.Ready)
			{
				this.waitMinimumDisplayingTimeRoutine = base.StartCoroutine(this.ShowFeedbackAfterMinimumDisplayingTime(this.ShowReadyFeedback));
			}
			else if (this.currState == ShopCashGUILoadFeedbackState.Loading)
			{
				this.waitMinimumDisplayingTimeRoutine = base.StartCoroutine(this.ShowFeedbackAfterMinimumDisplayingTime(new Action(this.ShowLoadingFeedback)));
			}
			else if (this.currState == ShopCashGUILoadFeedbackState.Unavailable)
			{
				this.waitMinimumDisplayingTimeRoutine = base.StartCoroutine(this.ShowFeedbackAfterMinimumDisplayingTime(new Action(this.ShowUnavailableFeedback)));
			}
			else
			{
				ShopCashGUILoadFeedback.Log.ErrorFormat("Cash shop can't show feeback for unknow state! {0}", new object[]
				{
					this.currState
				});
			}
		}

		public void HideFeedback()
		{
			this.StopWaitMinimumDisplayingTimeRoutine();
			this.timeControl.Stop();
			this.HideGuiElements();
		}

		public void HideFeedback(Action executeAfterHiding)
		{
			this.StopWaitMinimumDisplayingTimeRoutine();
			this.waitMinimumDisplayingTimeRoutine = base.StartCoroutine(this.ShowFeedbackAfterMinimumDisplayingTime(executeAfterHiding));
		}

		private void HideGuiElements()
		{
			this._errorIcon.SetActive(false);
			this._loadingIcon.SetActive(false);
			this._feedbackText.gameObject.SetActive(false);
		}

		private void StopWaitMinimumDisplayingTimeRoutine()
		{
			base.StopCoroutineSafe(this.waitMinimumDisplayingTimeRoutine);
			this.waitMinimumDisplayingTimeRoutine = null;
		}

		private IEnumerator ShowFeedbackAfterMinimumDisplayingTime(Action showFeedback)
		{
			int waitingTime = this.GetWaitingTimeToReplaceFeedback();
			if (waitingTime > 0)
			{
				yield return CoroutineUtil.WaitForRealSeconds((float)waitingTime);
			}
			this.HideGuiElements();
			showFeedback();
			this.timeControl.Reset();
			this.timeControl.Start();
			yield break;
		}

		private int GetWaitingTimeToReplaceFeedback()
		{
			double totalSeconds = this.timeControl.Elapsed.TotalSeconds;
			if (!this.timeControl.IsRunning || totalSeconds >= (double)this.minimumDisplayingTime)
			{
				return 0;
			}
			return Mathf.Max(1, this.minimumDisplayingTime - (int)totalSeconds);
		}

		private void ShowLoadingFeedback()
		{
			this._errorIcon.SetActive(false);
			this._loadingIcon.SetActive(true);
			this._feedbackText.text = Language.Get("SHOP_CASH_FEEDBACK_WAIT", ShopCashGUILoadFeedback.feedback_text_sheet);
			this._feedbackText.gameObject.SetActive(true);
			base.gameObject.GetComponent<Animation>().Play();
		}

		private void ShowUnavailableFeedback()
		{
			this._loadingIcon.SetActive(false);
			this._errorIcon.SetActive(true);
			this._feedbackText.text = Language.Get("SHOP_CASH_FEEDBACK_RETURN_LATER", ShopCashGUILoadFeedback.feedback_text_sheet);
			this._feedbackText.gameObject.SetActive(true);
			base.gameObject.GetComponent<Animation>().Play();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ShopCashGUILoadFeedback));

		private const string loading_data_feedback_key = "SHOP_CASH_FEEDBACK_WAIT";

		private const string return_later_feedback_key = "SHOP_CASH_FEEDBACK_RETURN_LATER";

		private static readonly ContextTag feedback_text_sheet = TranslationContext.Store;

		[SerializeField]
		[Tooltip("Minimum feedback displaying time in seconds before being allowed to be replaced.")]
		private int minimumDisplayingTime;

		[SerializeField]
		private GameObject _loadingIcon;

		[SerializeField]
		private GameObject _errorIcon;

		[SerializeField]
		private UILabel _feedbackText;

		private readonly Stopwatch timeControl = new Stopwatch();

		private ShopCashGUILoadFeedbackState currState;

		private Coroutine waitMinimumDisplayingTimeRoutine;

		private Action ShowReadyFeedback;
	}
}
