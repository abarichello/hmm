using System;
using HeavyMetalMachines.Arena;

namespace HeavyMetalMachines.Arenas
{
	public class GetArenaData : IGetArenaData
	{
		public GetArenaData(GameArenaConfig gameArenaConfig)
		{
			this._gameArenaConfig = gameArenaConfig;
		}

		public ArenaData Get(int arenaIndex)
		{
			IGameArenaInfo arenaByIndex = this._gameArenaConfig.GetArenaByIndex(arenaIndex);
			return GetArenaData.ConvertGameArenaInfoToArenaInfo(arenaByIndex);
		}

		private static ArenaData ConvertGameArenaInfoToArenaInfo(IGameArenaInfo gameArenaInfo)
		{
			return new ArenaData
			{
				NameDraft = gameArenaInfo.DraftName
			};
		}

		private readonly GameArenaConfig _gameArenaConfig;
	}
}
