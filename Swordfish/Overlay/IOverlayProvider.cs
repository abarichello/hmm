using System;

namespace HeavyMetalMachines.Swordfish.Overlay
{
	public interface IOverlayProvider
	{
		bool IsEnabled();

		void ShowWebPage(string url);
	}
}
