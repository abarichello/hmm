using System;
using Standard_Assets.Scripts.HMM.Customization;
using UniRx;

namespace HeavyMetalMachines.Customization.Business
{
	public class ObservePlayerAvatarChanged : IObservePlayerAvatarChanged
	{
		public ObservePlayerAvatarChanged(ISetCustomization setCustomization)
		{
			this._setCustomization = setCustomization;
		}

		public IObservable<Unit> Observe()
		{
			return Observable.AsUnitObservable<PlayerCustomizationItem>(Observable.Where<PlayerCustomizationItem>(this._setCustomization.OnSetCustomization, (PlayerCustomizationItem item) => item.Slot == 61));
		}

		private readonly ISetCustomization _setCustomization;
	}
}
