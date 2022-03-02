using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Store.View
{
	public interface IStoreFilterView
	{
		IActivatable MainActivatable { get; }

		IButton LeftButton { get; }

		IButton RightButton { get; }

		ILabel CurrentFilterLabel { get; }

		IActivatable ArrowLeftActivatable { get; }

		IActivatable ArrowRightActivatable { get; }

		IActivatable BorderActivatable { get; }
	}
}
