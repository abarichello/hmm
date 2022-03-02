using System;

namespace HeavyMetalMachines.PostProcessing
{
	public interface IGamePostProcessing
	{
		PostProcessingState Request(string identifier, Func<bool> condition, bool cleanState);
	}
}
