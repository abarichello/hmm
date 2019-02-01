using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class QuestionHintGuiItem : BaseHintGuiItem<QuestionHintGuiItem, QuestionHintContent>
	{
		public void onButtonClick_Approve()
		{
			this.SetAsFinished(true);
		}

		public void onButtonClick_Refuse()
		{
			this.SetAsFinished(false);
		}

		protected override void SetAsFinished()
		{
			this.SetAsFinished(false);
		}

		private void SetAsFinished(bool isApproved)
		{
			if (this._isFinished)
			{
				return;
			}
			base.DismissQuestionHint();
			if (isApproved)
			{
				base.ReferenceObject.ApproveAction();
				return;
			}
			if (base.ReferenceObject.RefuseAction == null)
			{
				return;
			}
			base.ReferenceObject.RefuseAction();
		}

		[SerializeField]
		private UIButton _accept_Button;

		[SerializeField]
		private UIButton _refuse_Button;
	}
}
