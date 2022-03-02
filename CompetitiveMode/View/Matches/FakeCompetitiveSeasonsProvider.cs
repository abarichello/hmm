using System;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class FakeCompetitiveSeasonsProvider : ICompetitiveSeasonsProvider
	{
		public IObservable<CompetitiveSeason> GetCurrentSeason()
		{
			return Observable.Select<long, CompetitiveSeason>(Observable.Timer(TimeSpan.FromSeconds(1.0)), (long _) => this.GetMockedCurrentSeason());
		}

		public IObservable<CompetitiveSeason> GetNextSeason()
		{
			return this.GetMockedNextSeason();
		}

		public CompetitiveSeason GetMockedCurrentSeason()
		{
			if (this._currentSeason == null)
			{
				this._currentSeason = FakeCompetitiveSeasonsProvider.CreateSeason(1L);
			}
			return this._currentSeason;
		}

		private IObservable<CompetitiveSeason> GetMockedNextSeason()
		{
			if (this._nextSeason == null)
			{
				this._nextSeason = FakeCompetitiveSeasonsProvider.CreateSeason(2L);
			}
			return Observable.Select<long, CompetitiveSeason>(Observable.Timer(TimeSpan.FromSeconds(1.0)), (long _) => this._nextSeason);
		}

		public static CompetitiveSeason CreateSeason(long id)
		{
			Guid[] array = new Guid[Random.Range(1, 4)];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Guid(FakeCompetitiveSeasonsProvider.ItemIds[Random.Range(0, FakeCompetitiveSeasonsProvider.ItemIds.Length)]);
			}
			return new CompetitiveSeason
			{
				Id = id,
				StartDateTime = DateTime.Parse("2019-01-01T09:00:00.000Z"),
				EndDateTime = DateTime.Parse("2019-12-01T18:00:00.000Z"),
				TopPlayersPrizesItemTypesIds = array,
				TopPlayersCount = 10,
				RequiredMatchesCountToUnlock = 5,
				RequiredMatchesCountToCalibrate = 10,
				Divisions = FakeCompetitiveSeasonsProvider.CreateDivisions()
			};
		}

		private static Division[] CreateDivisions()
		{
			return new Division[]
			{
				FakeCompetitiveSeasonsProvider.CreateDivision(0, "RANKING_BRONZE_LEAGUE"),
				FakeCompetitiveSeasonsProvider.CreateDivision(1000, "RANKING_SILVER_LEAGUE"),
				FakeCompetitiveSeasonsProvider.CreateDivision(2000, "RANKING_GOLD_LEAGUE")
			};
		}

		private static Division CreateDivision(int startingScore, string nameDraft)
		{
			Guid[] array = new Guid[Random.Range(1, 4)];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Guid(FakeCompetitiveSeasonsProvider.ItemIds[Random.Range(0, FakeCompetitiveSeasonsProvider.ItemIds.Length)]);
			}
			return new Division
			{
				NameDraft = nameDraft,
				StartingScore = startingScore,
				EndingScore = startingScore + 999,
				PrizesItemTypeIds = array,
				Subdivisions = new Subdivision[]
				{
					new Subdivision
					{
						StartingScore = startingScore,
						EndingScore = startingScore + 199
					},
					new Subdivision
					{
						StartingScore = startingScore + 200,
						EndingScore = startingScore + 399
					},
					new Subdivision
					{
						StartingScore = startingScore + 400,
						EndingScore = startingScore + 599
					},
					new Subdivision
					{
						StartingScore = startingScore + 600,
						EndingScore = startingScore + 799
					},
					new Subdivision
					{
						StartingScore = startingScore + 800,
						EndingScore = startingScore + 999
					}
				}
			};
		}

		private CompetitiveSeason _currentSeason;

		private CompetitiveSeason _nextSeason;

		private static readonly string[] ItemIds = new string[]
		{
			"ed59094b-e7b6-4383-a135-3c0444b6c3b1",
			"a85721b3-d1dd-424d-b894-2f7c4a51e283",
			"818f14dd-198b-467c-b892-af19483b7a4d",
			"D27E4770-D0D6-45AB-A7F1-7D1CAD8498AB"
		};
	}
}
