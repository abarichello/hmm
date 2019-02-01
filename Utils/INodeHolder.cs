using System;

namespace HeavyMetalMachines.Utils
{
	public interface INodeHolder
	{
		int[] NodeIds { get; }

		int[] NodeRemovals { get; }
	}
}
