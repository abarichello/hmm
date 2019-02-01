using System;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassSeasonScroller
	{
		UnityUiBattlepassSeasonCellView.SeasonCellViewData GetSeasonCellViewData(int slotLevel);

		bool IsSlotSelectionInCorner(int slotIndex);

		bool IsSlotSelectionOutCorner(int slotIndex);
	}
}
