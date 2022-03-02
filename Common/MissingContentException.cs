using System;

namespace HeavyMetalMachines.Common
{
	public class MissingContentException : Exception
	{
		public MissingContentException(string message) : base(message)
		{
		}
	}
}
