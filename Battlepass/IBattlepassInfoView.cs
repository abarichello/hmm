using System;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassInfoView
	{
		void SetVisibility(bool isVisible);

		bool IsVisible();
	}
}
