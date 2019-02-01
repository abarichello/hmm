using System;

namespace HeavyMetalMachines.PurchaseFeedback
{
	public interface IPurchaseFeedbackView
	{
		void Show(PurchaseFeedbackView.PurchasedFeedbackViewData viewData);

		void Hide();
	}
}
