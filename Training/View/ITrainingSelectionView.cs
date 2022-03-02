using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Training.View
{
	public interface ITrainingSelectionView
	{
		IButton MainButton { get; }

		int ArenaIndex { get; }

		ButtonNameInstance BiButtonName { get; }
	}
}
