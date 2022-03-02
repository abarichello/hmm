using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using Standard_Assets.Scripts.HMM.Customization;
using UniRx;

namespace HeavyMetalMachines.Customization.Business
{
	public interface ISetCustomization
	{
		IObservable<Unit> Set(PlayerCustomizationSlot slot, Guid itemId);

		IObservable<PlayerCustomizationItem> OnSetCustomization { get; }
	}
}
