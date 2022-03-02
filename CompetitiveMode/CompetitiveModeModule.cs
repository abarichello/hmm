using System;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.CompetitiveMode.Prizes;
using HeavyMetalMachines.CompetitiveMode.Ranking;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.Matches;
using Hoplon.DependencyInjection;

namespace HeavyMetalMachines.CompetitiveMode
{
	public class CompetitiveModeModule
	{
		public CompetitiveModeModule(IInjectionBinder injectionBinder)
		{
			this._injectionBinder = injectionBinder;
		}

		public void Install()
		{
			this._injectionBinder.BindTransient<IUpdateSeasons, UpdateSeasons>();
			this._injectionBinder.BindTransient<IGetCurrentOrNextCompetitiveSeason, GetCurrentOrNextCompetitiveSeason>();
			this._injectionBinder.BindTransient<IGetCompetitiveDivisions, GetCompetitiveDivisions>();
			this._injectionBinder.BindTransient<IClearPlayersStateStorage, ClearPlayersStateStorage>();
			this._injectionBinder.BindSingle<IPlayersStateStorage, PlayersStateStorage>();
			this._injectionBinder.BindTransient<IGetOrFetchPlayerState, GetOrFetchPlayerState>();
			this._injectionBinder.BindTransient<IUpdatePlayerState, UpdatePlayerState>();
			this._injectionBinder.BindTransient<IGetPlayerCompetitiveState, GetPlayerCompetitiveState>();
			this._injectionBinder.BindTransient<IFetchPlayerCompetitiveState, FetchPlayerCompetitiveState>();
			this._injectionBinder.BindTransient<IInitializeAndWatchMyPlayerCompetitiveStateProgress, InitializeAndWatchMyPlayerCompetitiveStateProgress>();
			this._injectionBinder.BindTransient<IShouldTrackPlayerCompetitiveStateProgress, ShouldTrackPlayerCompetitiveStateProgress>();
			this._injectionBinder.BindTransient<IWaitAndGetMyPlayerCompetitiveStateProgress, WaitAndGetMyPlayerCompetitiveStateProgress>();
			this._injectionBinder.BindSingle<IMyPlayerStateProgressStorage, MyPlayerStateProgressStorage>();
			this._injectionBinder.BindTransient<IUpdateCurrentMatchPlayersCompetitiveState, UpdateCurrentMatchPlayersCompetitiveState>();
			this._injectionBinder.BindTransient<IInitializeCompetitiveMode, InitializeCompetitiveMode>();
			this._injectionBinder.BindTransient<IGetCompetitiveGlobalRanking, GetCompetitiveGlobalRanking>();
			this._injectionBinder.BindTransient<IGetCompetitiveFriendsRanking, GetCompetitiveFriendsRanking>();
			this._injectionBinder.BindTransient<IGetCompetitiveDivisionsPrizes, GetCompetitiveDivisionsPrizes>();
			this._injectionBinder.BindTransient<ITryCollectMyPlayerPendingCompetitivePrizes, TryCollectMyPlayerPendingCompetitivePrizes>();
			this._injectionBinder.BindTransient<IConsumeCurrentCompetitiveSeasonNews, ConsumeCurrentCompetitiveSeasonNews>();
			this._injectionBinder.BindSingle<ICurrentMatchStorage, CurrentMatchStorage>();
			this._injectionBinder.BindTransient<IRestoreCurrentMatch, RestoreCurrentMatch>();
			this._injectionBinder.BindSingletonInstance<CompetitiveModeLocalConfiguration>(new CompetitiveModeLocalConfiguration
			{
				AllowedNumberOfGroupMembers = 0
			});
		}

		private readonly IInjectionBinder _injectionBinder;
	}
}
