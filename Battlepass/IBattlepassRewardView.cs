using System;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassRewardView
	{
		void SetVisibility(bool isVisible);

		bool IsVisible();
	}
}
