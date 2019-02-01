using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class PickStateFeedback
	{
		public void OnPickCharacterStateStarted()
		{
			this.OnStateStart(this.FirstStateSignal);
		}

		public void OnPickSkinStateStarted()
		{
			PickStateFeedback.OnStateFinish(this.FirstStateSignal, this.FirstStateNumber);
			this.OnStateStart(this.SecondStateSignal);
		}

		public void OnPickGridStateStarted()
		{
			PickStateFeedback.OnStateFinish(this.SecondStateSignal, this.SecondStateNumber);
			this.OnStateStart(this.ThirdStateSignal);
		}

		public void OnWaitOtherPlayersStateStarted()
		{
			PickStateFeedback.OnStateFinish(this.ThirdStateSignal, this.ThirdStateNumber);
			this.OnStateStart(this.FourthStateSignal);
			this.FourthStateAnim.gameObject.SetActive(true);
			this.FourthStateAnim.Play();
			string text = Language.Get("WAIT_FOR_OTHER_PLAYERS_STATE_FEEDBACK", TranslationSheets.PickMode);
			this.ConfirmButtonLabel.text = text;
		}

		public void OnWaitOtherPlayersStateFinished()
		{
			PickStateFeedback.OnStateFinish(this.FourthStateSignal, this.FourthStateNumber);
			this.FourthStateAnim.Stop();
			this.FourthStateAnim.gameObject.SetActive(false);
		}

		private void OnStateStart(UI2DSprite signal)
		{
			signal.color = this.OnStateStartColor;
		}

		private static void OnStateFinish(UI2DSprite signal, UILabel number)
		{
			signal.gameObject.SetActive(false);
			number.gameObject.SetActive(false);
		}

		public Color OnStateStartColor;

		public UI2DSprite FirstStateSignal;

		public UILabel FirstStateNumber;

		public UI2DSprite SecondStateSignal;

		public UILabel SecondStateNumber;

		public UI2DSprite ThirdStateSignal;

		public UILabel ThirdStateNumber;

		public UI2DSprite FourthStateSignal;

		public UILabel FourthStateNumber;

		public Animation FourthStateAnim;

		public UILabel ConfirmButtonLabel;
	}
}
