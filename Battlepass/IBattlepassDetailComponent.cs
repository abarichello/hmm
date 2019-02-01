using System;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassDetailComponent
	{
		UnityUiBattlepassDetailView.BattlepassDetailViewData RegisterDetailView(IBattlepassDetailView view);

		bool TryToShowDetailWindow(Action<bool> onWindowCloseAction);

		void HideDetailWindow(bool showMetalpassWindow);
	}
}
