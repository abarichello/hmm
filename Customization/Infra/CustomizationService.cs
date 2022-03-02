using System;
using Assets.ClientApiObjects;
using Assets.Customization;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Customization.Business;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Items.DataTransferObjects;
using HeavyMetalMachines.Swordfish;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Customization.Infra
{
	public class CustomizationService : ICustomizationService
	{
		public CustomizationContent GetCurrentPlayerCustomizationContent()
		{
			return this._localUserCustomization.Get();
		}

		public ItemTypeScriptableObject GetItemTypeScriptableObjectBySlot(PlayerCustomizationSlot slot, CustomizationContent customization)
		{
			return this._customizationAssets.GetItemTypeScriptableObjectBySlot(slot, customization);
		}

		public IObservable<NetResult> PercistCustomizationItemInCustomWs(PlayerCustomizationSlot slot, Guid itemId)
		{
			CustomizationBagAdapter customizationBagAdapter = new CustomizationBagAdapter
			{
				Slot = slot,
				TypeId = itemId
			};
			return this._customWs.ExecuteAsObservable("SaveCustomizationSelected", customizationBagAdapter.ToString());
		}

		[Inject]
		private IGetLocalUserCustomization _localUserCustomization;

		[Inject]
		private ICustomizationAssets _customizationAssets;

		[Inject]
		private ICustomWS _customWs;
	}
}
