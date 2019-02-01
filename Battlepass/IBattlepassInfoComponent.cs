using System;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassInfoComponent
	{
		void RegisterInfoView(IBattlepassInfoView view);

		void ShowInfoWindow(Action onWindowCloseAction);

		void HideInfoWindow();

		void OnInfoWindowHideAnimationEnded();
	}
}
