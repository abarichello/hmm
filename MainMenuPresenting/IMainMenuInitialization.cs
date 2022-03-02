using System;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public interface IMainMenuInitialization
	{
		MainMenuNode NodeToGo { get; set; }

		void ClearNodeToGo();
	}
}
