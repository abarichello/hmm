using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Items.DataTransferObjects;
using UniRx;

namespace HeavyMetalMachines.Customization.Infra
{
	public interface ICustomizationService
	{
		CustomizationContent GetCurrentPlayerCustomizationContent();

		ItemTypeScriptableObject GetItemTypeScriptableObjectBySlot(PlayerCustomizationSlot slot, CustomizationContent customization);

		IObservable<NetResult> PercistCustomizationItemInCustomWs(PlayerCustomizationSlot slot, Guid itemId);
	}
}
