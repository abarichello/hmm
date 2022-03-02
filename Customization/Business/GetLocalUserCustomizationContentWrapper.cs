using System;
using HeavyMetalMachines.Items.DataTransferObjects;
using Zenject;

namespace HeavyMetalMachines.Customization.Business
{
	public class GetLocalUserCustomizationContentWrapper : IGetLocalUserCustomization
	{
		public CustomizationContent Get()
		{
			return this._userInfo.Inventory.Customizations;
		}

		[Inject]
		private readonly UserInfo _userInfo;
	}
}
