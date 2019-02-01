using System;

namespace HeavyMetalMachines.PurchaseFeedback
{
	public interface IPurchaseFeedbackComponent
	{
		void TryToShowBoughtHardCurrency(Action onTryToShowEnd);

		void RegisterView(IPurchaseFeedbackView purchaseFeedbackView);

		void HideView();

		void OnViewClosed();
	}
}
