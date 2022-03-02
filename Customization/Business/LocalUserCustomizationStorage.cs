using System;
using HeavyMetalMachines.Items.DataTransferObjects;
using UniRx;

namespace HeavyMetalMachines.Customization.Business
{
	public class LocalUserCustomizationStorage
	{
		public LocalUserCustomizationStorage()
		{
			this._customizationObservation = new Subject<CustomizationContent>();
		}

		public CustomizationContent LocalUserCustomization
		{
			get
			{
				return this._localUserCustomization;
			}
			set
			{
				this._localUserCustomization = value;
				this._customizationObservation.OnNext(this._localUserCustomization);
			}
		}

		public IObservable<CustomizationContent> CustomizationObservation
		{
			get
			{
				return this._customizationObservation;
			}
		}

		private readonly Subject<CustomizationContent> _customizationObservation;

		private CustomizationContent _localUserCustomization;
	}
}
