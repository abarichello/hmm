using System;

namespace HeavyMetalMachines.Store.Business
{
	public class UnavailableProviderException : Exception
	{
		public UnavailableProviderException(Exception innerException) : base("A problem occurred when calling the provider. Check the inner exception for details.", innerException)
		{
		}
	}
}
