using System;
using HeavyMetalMachines.Swordfish.Overlay;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class FakeOverlayProvider : IOverlayProvider
	{
		public bool IsEnabled()
		{
			return false;
		}

		public void ShowWebPage(string url)
		{
		}
	}
}
