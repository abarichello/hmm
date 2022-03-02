using System;
using HeavyMetalMachines.Frontend;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public interface IMainMenuGuiProvider
	{
		bool TryToGetMainMenuGui(out MainMenuGui mainMenuGui);
	}
}
