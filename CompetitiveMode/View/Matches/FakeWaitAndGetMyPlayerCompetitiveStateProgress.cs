using System;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class FakeWaitAndGetMyPlayerCompetitiveStateProgress : IWaitAndGetMyPlayerCompetitiveStateProgress
	{
		public FakeWaitAndGetMyPlayerCompetitiveStateProgress()
		{
			this._progresses = new PlayerCompetitiveProgress[]
			{
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateCalibratingCompetitiveState(5),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateCalibratingCompetitiveState(6)
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateCalibratingCompetitiveState(9),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(1, 2, 1500, null)
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(0, 2, 430, null),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(0, 2, 580, null)
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 3, 2799, null),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 2815, null)
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(1, 4, 1990, null),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 0, 2020, null)
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 2990, null),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 3020, new int?(5))
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 3150, new int?(15)),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 3205, new int?(4))
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(0, 4, 950, null),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(0, 4, 875, null)
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 1, 2210, null),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 0, 2185, null)
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(1, 0, 1020, null),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(0, 4, 990, null)
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 3020, new int?(10)),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 2980, null)
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 3255, new int?(24)),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 3205, new int?(25))
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 3150, new int?(3)),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 3205, new int?(3))
				},
				new PlayerCompetitiveProgress
				{
					InitialState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 3150, new int?(3)),
					FinalState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateRankedState(2, 4, 3075, new int?(3))
				}
			};
		}

		public IObservable<PlayerCompetitiveProgress> WaitAndGet()
		{
			return Observable.Select<long, PlayerCompetitiveProgress>(Observable.Timer(TimeSpan.FromSeconds(1.0)), (long _) => this._progresses[FakeWaitAndGetMyPlayerCompetitiveStateProgress._currentProgressIndex]);
		}

		public static void SetProgressIndex(int index)
		{
			FakeWaitAndGetMyPlayerCompetitiveStateProgress._currentProgressIndex = index;
		}

		private static PlayerCompetitiveState CreateCalibratingCompetitiveState(int playedMatches)
		{
			PlayerCompetitiveState result = default(PlayerCompetitiveState);
			result.Status = 1;
			result.CalibrationState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateCalibrationState(playedMatches);
			return result;
		}

		private static PlayerCompetitiveCalibrationState CreateCalibrationState(int matchesPlayed)
		{
			bool[] array = new bool[matchesPlayed];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (i % 2 == 0);
			}
			PlayerCompetitiveCalibrationState result = default(PlayerCompetitiveCalibrationState);
			result.TotalMatchesPlayed = matchesPlayed;
			result.TotalRequiredMatches = 10;
			result.MatchesResults = array;
			return result;
		}

		private static PlayerCompetitiveState CreateRankedState(int division, int subdivision, int score, int? topPlacementPosition = null)
		{
			PlayerCompetitiveState result = default(PlayerCompetitiveState);
			result.Status = 2;
			result.CalibrationState = FakeWaitAndGetMyPlayerCompetitiveStateProgress.CreateCalibrationState(10);
			PlayerCompetitiveRank rank = default(PlayerCompetitiveRank);
			CompetitiveRank currentRank = default(CompetitiveRank);
			currentRank.Division = division;
			currentRank.Subdivision = subdivision;
			currentRank.Score = score;
			currentRank.TopPlacementPosition = topPlacementPosition;
			rank.CurrentRank = currentRank;
			result.Rank = rank;
			return result;
		}

		private readonly PlayerCompetitiveProgress[] _progresses;

		private static int _currentProgressIndex;
	}
}
