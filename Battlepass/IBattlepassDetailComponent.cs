using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassDetailComponent
	{
		void RegisterDetailView(IBattlepassDetailView view);

		bool TryToShowDetailWindow(Action<bool> onWindowCloseAction);

		void HideDetailWindow(bool showMetalpassWindow);

		BattlepassConfig BattlepassConfig { get; }

		bool CanOpenDetailWindow();
	}
}
