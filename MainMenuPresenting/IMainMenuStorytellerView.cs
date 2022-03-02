using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public interface IMainMenuStorytellerView
	{
		IButton OpenStorytellerButton { get; }

		IActivatable ButtonActivatable { get; }
	}
}
