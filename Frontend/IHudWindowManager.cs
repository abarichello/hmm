using System;

namespace HeavyMetalMachines.Frontend
{
	public interface IHudWindowManager
	{
		void Push(IHudWindow window);

		void Remove(IHudWindow window);

		bool IsWindowVisible<T>() where T : IHudWindow;

		GuiGameState State { get; }

		event GuiGameStateChange OnGuiStateChange;

		event Action OnHelp;
	}
}
