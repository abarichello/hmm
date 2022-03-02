using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class WaitAndGetMyPlayerCompetitiveStateProgress : IWaitAndGetMyPlayerCompetitiveStateProgress
	{
		public WaitAndGetMyPlayerCompetitiveStateProgress(IMyPlayerStateProgressStorage myPlayerStateProgressStorage, IClearPlayersStateStorage clearPlayersStateStorage, ILogger<WaitAndGetMyPlayerCompetitiveStateProgress> logger)
		{
			this._myPlayerStateProgressStorage = myPlayerStateProgressStorage;
			this._clearPlayersStateStorage = clearPlayersStateStorage;
			this._logger = logger;
		}

		public IObservable<PlayerCompetitiveProgress> WaitAndGet()
		{
			if (this._myPlayerStateProgressStorage.InitialState != null && this._myPlayerStateProgressStorage.FinalState != null)
			{
				PlayerCompetitiveProgress playerCompetitiveProgress = new PlayerCompetitiveProgress(this._myPlayerStateProgressStorage.InitialState.Value, this._myPlayerStateProgressStorage.FinalState.Value);
				return Observable.Do<PlayerCompetitiveProgress>(Observable.Return<PlayerCompetitiveProgress>(playerCompetitiveProgress), delegate(PlayerCompetitiveProgress _)
				{
					this._clearPlayersStateStorage.Clear();
				});
			}
			return Observable.Catch<PlayerCompetitiveProgress, TimeoutException>(Observable.Timeout<PlayerCompetitiveProgress>(Observable.Do<PlayerCompetitiveProgress>(Observable.First<PlayerCompetitiveProgress>(this._myPlayerStateProgressStorage.OnProgressSet), delegate(PlayerCompetitiveProgress _)
			{
				this._clearPlayersStateStorage.Clear();
			}), TimeSpan.FromSeconds(30.0)), (TimeoutException exception) => this.GetFallbackProgress());
		}

		private IObservable<PlayerCompetitiveProgress> GetFallbackProgress()
		{
			PlayerCompetitiveState fallbackProgressOrDefault = this.GetFallbackProgressOrDefault();
			PlayerCompetitiveProgress playerCompetitiveProgress = new PlayerCompetitiveProgress(fallbackProgressOrDefault, fallbackProgressOrDefault);
			return Observable.Do<PlayerCompetitiveProgress>(Observable.Return<PlayerCompetitiveProgress>(playerCompetitiveProgress), delegate(PlayerCompetitiveProgress _)
			{
				this.LogProgressTookTooLongToBeReceived();
			});
		}

		private PlayerCompetitiveState GetFallbackProgressOrDefault()
		{
			if (this._myPlayerStateProgressStorage.InitialState != null)
			{
				return this._myPlayerStateProgressStorage.InitialState.Value;
			}
			PlayerCompetitiveState result = default(PlayerCompetitiveState);
			PlayerCompetitiveRank rank = default(PlayerCompetitiveRank);
			CompetitiveRank currentRank = default(CompetitiveRank);
			currentRank.Division = 0;
			currentRank.Subdivision = 0;
			currentRank.Score = 0;
			currentRank.TopPlacementPosition = null;
			rank.CurrentRank = currentRank;
			result.Rank = rank;
			result.Status = 2;
			return result;
		}

		private void LogProgressTookTooLongToBeReceived()
		{
			this._logger.Error("Took too long to receive player competitive progress. Fallbacking to unchanged state.");
		}

		private readonly IMyPlayerStateProgressStorage _myPlayerStateProgressStorage;

		private readonly IClearPlayersStateStorage _clearPlayersStateStorage;

		private readonly ILogger<WaitAndGetMyPlayerCompetitiveStateProgress> _logger;
	}
}
