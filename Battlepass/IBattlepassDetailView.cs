using System;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassDetailView
	{
		void SetVisibility(bool isVisible);

		bool IsVisible();
	}
}
