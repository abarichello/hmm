using System;
using HeavyMetalMachines.DataTransferObjects;
using HeavyMetalMachines.ToggleableFeatures;

namespace HeavyMetalMachines.EnvironmentConfigurations
{
	public static class RemotelyToggleableFeatures
	{
		public static readonly RemotelyToggleableFeature[] DefaultConfiguration = new RemotelyToggleableFeature[]
		{
			new RemotelyToggleableFeature
			{
				Name = Features.OnboardingScreenPopup.Name,
				IsEnabled = false,
				IsAbTestEnabled = false
			},
			new RemotelyToggleableFeature
			{
				Name = Features.ForcedTutorial.Name,
				IsEnabled = true,
				IsAbTestEnabled = false
			},
			new RemotelyToggleableFeature
			{
				Name = Features.ShopPopup.Name,
				IsEnabled = true,
				IsAbTestEnabled = false
			},
			new RemotelyToggleableFeature
			{
				Name = Features.ShopPopupFirstLoginReward.Name,
				IsEnabled = false,
				IsAbTestEnabled = false
			}
		};
	}
}
