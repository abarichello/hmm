using System;
using UniRx;

namespace HeavyMetalMachines.Customization
{
	public interface IGetCustomizationChange
	{
		IObservable<ItemChangeRequestState> OnItemEquipChanged { get; }
	}
}
