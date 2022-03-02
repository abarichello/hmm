using System;
using UniRx;

namespace HeavyMetalMachines.Customization
{
	public interface IGetCustomizationHoverChange
	{
		IObservable<CustomizationInventoryCellItemData> ObserveHoverChange { get; }
	}
}
