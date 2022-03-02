using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class WaitingWindowService : GameHubObject, IWaitingWindow
	{
		public void Show(bool showLabel, Type type)
		{
			GameHubObject.Hub.GuiScripts.SharedPreGameWindow.ShowWaitingWindow(showLabel, type);
		}

		public void Show(Type type)
		{
			GameHubObject.Hub.GuiScripts.SharedPreGameWindow.ShowWaitingWindow(type);
		}

		public void Hide(Type type)
		{
			GameHubObject.Hub.GuiScripts.SharedPreGameWindow.HideWaitingWindow(type);
		}
	}
}
