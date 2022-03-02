using System;
using HeavyMetalMachines.Matches.DataTransferObjects;

namespace HeavyMetalMachines.Arena
{
	public interface IUiGameArenaInfo
	{
		string PickBackgroundImageName { get; }

		string UnlockIconName { get; }

		string CustomMatchSelectionImageName { get; }

		string ArenaSelectorImageName { get; }

		string GameModeDraft { get; }

		string MatchesSlotIconName { get; }

		bool LifebarShowIndestructibleFeedback { get; }

		bool DisableUINearBombFeedback { get; }

		MatchKind[] ShowInMatchKindsSelector { get; }
	}
}
