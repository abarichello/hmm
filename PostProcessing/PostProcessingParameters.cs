using System;

namespace HeavyMetalMachines.PostProcessing
{
	public struct PostProcessingParameters<T>
	{
		public bool Enabled;

		public T Parameters;
	}
}
