using System;
using HeavyMetalMachines.Customization.Infra;
using HeavyMetalMachines.Items.DataTransferObjects;

namespace HeavyMetalMachines.Players.Business
{
	public class GetLocalPlayerCustomizationContent : IGetLocalPlayerCustomizationContent
	{
		public GetLocalPlayerCustomizationContent(ICustomizationService customizationService)
		{
			this._customizationService = customizationService;
		}

		public CustomizationContent Get()
		{
			return this._customizationService.GetCurrentPlayerCustomizationContent();
		}

		private readonly ICustomizationService _customizationService;
	}
}
