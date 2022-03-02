using System;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.Frontend.Region
{
	public interface IRegionView
	{
		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
