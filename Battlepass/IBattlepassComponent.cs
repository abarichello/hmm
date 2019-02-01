using System;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassComponent
	{
		void RegisterView(IBattlepassView view);

		void ShowMetalpassWindow(Action onWindowCloseAction);

		void HideMetalpassWindow(bool imediate);

		void MetalpassBuyLevelRequest(int level, Action onRequestOk, Action onBuyWindowClosed);

		void MarkMissionsAsSeen();

		void SetLevelFake(int level);
	}
}
