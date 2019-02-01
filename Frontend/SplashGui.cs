using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class SplashGui : StateGuiController
	{
		public void PlaySplashes(Action splashesEndedCallback)
		{
			GameHubBehaviour.Hub.Swordfish.Log.BILogFunnel(FunnelBITags.SplashVideoStart, null);
			splashesEndedCallback();
		}

		public bool HasFinished()
		{
			return true;
		}

		public void ShowLoginStatusMessage(string message)
		{
		}
	}
}
