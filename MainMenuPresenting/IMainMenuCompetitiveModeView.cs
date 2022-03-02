using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public interface IMainMenuCompetitiveModeView
	{
		IButton OpenCompetitiveModeButton { get; }

		IActivatable OpenRankingButtonActivatable { get; }
	}
}
