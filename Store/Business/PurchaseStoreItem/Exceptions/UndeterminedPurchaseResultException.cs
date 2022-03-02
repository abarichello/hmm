using System;

namespace HeavyMetalMachines.Store.Business.PurchaseStoreItem.Exceptions
{
	public class UndeterminedPurchaseResultException : Exception
	{
		public UndeterminedPurchaseResultException() : base("The purchase could not be completed and the result is undetermined.")
		{
		}

		public UndeterminedPurchaseResultException(Exception innerException) : base("The purchase could not be completed and the result is undetermined. Check the inner exception for details.", innerException)
		{
		}
	}
}
