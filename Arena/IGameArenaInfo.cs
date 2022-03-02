using System;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Arena
{
	public interface IGameArenaInfo : IGameplayGameArenaInfo, IMinimapGameArenaInfo, IUiGameArenaInfo
	{
		TeamKind ArenaFlipTeam { get; }

		int ArenaFlipScale { get; }

		int ArenaFlipRotation { get; }

		TeamKind TugOfWarInvertProgressTeam { get; }

		TeamKind TugOfWarFlipTeam { get; }

		int UnlockLevel { get; }

		bool IsTutorial { get; }

		bool IsCustomOnly { get; }

		bool IsOnCheatsEnabledOnly { get; }

		string SceneName { get; }

		string DraftName { get; }

		string LoadingBackgroundRedTeamImageName { get; }

		string LoadingBackgroundBlueTeamImageName { get; }

		PreCacheArenaObjects[] ObjectsToLoad { get; }

		int CameraInversionTeamAAngleY { get; }

		int CameraInversionTeamBAngleY { get; }

		bool FlipPinVerticalPosition { get; }

		bool FlipPinHorizontalPosition { get; }
	}
}
