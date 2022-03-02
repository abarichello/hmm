using System;
using HeavyMetalMachines.Store;

namespace HeavyMetalMachines.PurchaseFeedback
{
	public interface IPurchaseFeedbackComponent
	{
		void TryToShowBoughtHardCurrency(Action onTryToShowEnd, ILocalBalanceStorage localBalanceStorage);

		void RegisterView(IPurchaseFeedbackView purchaseFeedbackView);

		void HideView();

		void OnViewClosed();
	}
}
