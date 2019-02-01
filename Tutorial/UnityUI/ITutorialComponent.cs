using System;

namespace HeavyMetalMachines.Tutorial.UnityUI
{
	public interface ITutorialComponent
	{
		void Load();

		void Unload();

		void RegisterEvents();

		void UnregisterEvents();

		void RegisterView(ITutorialPanelView tutorialPanelView);
	}
}
