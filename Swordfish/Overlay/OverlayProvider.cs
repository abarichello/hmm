using System;
using ClientAPI.Service.API.Interfaces.Custom;

namespace HeavyMetalMachines.Swordfish.Overlay
{
	public class OverlayProvider : IOverlayProvider
	{
		public OverlayProvider(IOverlayContainer overlayContainer)
		{
			if (overlayContainer == null)
			{
				throw new ArgumentNullException("overlayContainer");
			}
			this._overlayContainer = overlayContainer;
		}

		public bool IsEnabled()
		{
			return this._overlayContainer.IsOverlayEnabled();
		}

		public void ShowWebPage(string url)
		{
			this._overlayContainer.ShowWebPage(url);
		}

		private readonly IOverlayContainer _overlayContainer;
	}
}
