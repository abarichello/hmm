using System;
using HeavyMetalMachines.Customization.Infra;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Result;
using Standard_Assets.Scripts.HMM.Customization;
using UniRx;

namespace HeavyMetalMachines.Customization.Business
{
	public class SetCustomization : ISetCustomization
	{
		public SetCustomization(ICustomizationService customizationService)
		{
			this._customizationService = customizationService;
		}

		public IObservable<PlayerCustomizationItem> OnSetCustomization
		{
			get
			{
				return this._setCustomizationSubject;
			}
		}

		public IObservable<Unit> Set(PlayerCustomizationSlot slot, Guid itemId)
		{
			return Observable.AsUnitObservable<NetResult>(Observable.Do<NetResult>(this._customizationService.PercistCustomizationItemInCustomWs(slot, itemId), delegate(NetResult _)
			{
				this.RaiseOnSetCustomization(slot, itemId);
			}));
		}

		private void RaiseOnSetCustomization(PlayerCustomizationSlot slot, Guid itemId)
		{
			PlayerCustomizationItem playerCustomizationItem = new PlayerCustomizationItem
			{
				ItemId = itemId,
				Slot = slot
			};
			this._setCustomizationSubject.OnNext(playerCustomizationItem);
		}

		private readonly Subject<PlayerCustomizationItem> _setCustomizationSubject = new Subject<PlayerCustomizationItem>();

		private readonly ICustomizationService _customizationService;
	}
}
