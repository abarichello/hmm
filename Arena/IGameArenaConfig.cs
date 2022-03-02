using System;
using Assets.ClientApiObjects.Components.API;

namespace HeavyMetalMachines.Arena
{
	public interface IGameArenaConfig
	{
		[Obsolete]
		string GetSceneName(int arenaIndex);

		[Obsolete]
		string GetLoadingImageName(int arenaIndex, int team);

		[Obsolete]
		string GetPickImageName(int arenaIndex);

		[Obsolete]
		string GetUnlockImageName(int arenaIndex);

		[Obsolete]
		string GetArenaDraftName(int arenaIndex);

		[Obsolete]
		IGameArenaInfo GetCurrentArena();

		[Obsolete]
		PreCacheArenaObjects[] SceneObjectsToLoad(int arenaIndex);

		[Obsolete]
		IGameArenaInfo GetArenaByIndex(int arenaIndex);

		int GetNumberOfArenas();

		IGameArenaInfo GetArenaInfoByGuid(Guid itemTypeGuid);

		IGameModeItemTypeComponent GetCurrentArenaMode();

		IBotDifficultyGameArenaInfo GetCurrentArenaBotDifficulty();
	}
}
