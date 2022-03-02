using System;
using HeavyMetalMachines.Customization.Business;
using HeavyMetalMachines.Items.DataTransferObjects;

namespace HeavyMetalMachines.Social.Avatar.Business
{
	public class GetLocalUserPortraitIconName : IGetLocalUserPortraitBorderIconName
	{
		public GetLocalUserPortraitIconName(IGetLocalUserCustomization getLocalUserCustomization, IGetPortraitBorderIconName getPortraitBorderIconName)
		{
			this._getLocalUserCustomization = getLocalUserCustomization;
			this._getPortraitBorderIconName = getPortraitBorderIconName;
		}

		public string GetMediumSquare()
		{
			CustomizationContent customizationContent = this._getLocalUserCustomization.Get();
			return this._getPortraitBorderIconName.GetMediumSquare(customizationContent.GetGuidBySlot(60));
		}

		private readonly IGetPortraitBorderIconName _getPortraitBorderIconName;

		private readonly IGetLocalUserCustomization _getLocalUserCustomization;
	}
}
