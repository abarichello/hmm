using System;
using System.Linq;
using HeavyMetalMachines.Tournaments.Infra;
using HeavyMetalMachines.Tournaments.Ranking;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Tournaments.Injection
{
	public class RandomTournamentRankingProvider : ITournamentRankingProvider
	{
		public IObservable<TournamentRanking> GetGeneralRanking(long tournamentId, int numberOfPositions)
		{
			return Observable.Delay<TournamentRanking>(Observable.Return<TournamentRanking>(this.GenerateRandomRanking(tournamentId, numberOfPositions)), TimeSpan.FromSeconds(1.0));
		}

		public IObservable<TournamentRanking> GetStepRanking(long tournamentId, int numberOfPositions)
		{
			return Observable.Delay<TournamentRanking>(Observable.Return<TournamentRanking>(this.GenerateRandomRanking(tournamentId, numberOfPositions)), TimeSpan.FromSeconds(1.0));
		}

		public IObservable<TournamentRankingPosition> GetTeamGeneralRankingPosition(long tournamentId, Guid teamId)
		{
			return Observable.Delay<TournamentRankingPosition>(Observable.Return<TournamentRankingPosition>(RandomTournamentRankingProvider.GenerateRandomRankingPosition((long)Random.Range(1, 50), tournamentId)), TimeSpan.FromSeconds(1.0));
		}

		public IObservable<TournamentRankingPosition> GetTeamStepRankingPosition(long stepId, Guid teamId)
		{
			return Observable.Delay<TournamentRankingPosition>(Observable.Return<TournamentRankingPosition>(RandomTournamentRankingProvider.GenerateRandomRankingPosition((long)Random.Range(1, 50), stepId)), TimeSpan.FromSeconds(1.0));
		}

		private TournamentRanking GenerateRandomRanking(long stepIndex, int numberOfPositions)
		{
			TournamentRanking tournamentRanking = new TournamentRanking();
			tournamentRanking.Positions = new TournamentRankingPosition[numberOfPositions];
			for (int i = 0; i < numberOfPositions; i++)
			{
				tournamentRanking.Positions[i] = RandomTournamentRankingProvider.GenerateRandomRankingPosition((long)i, stepIndex);
			}
			return tournamentRanking;
		}

		private static TournamentRankingPosition GenerateRandomRankingPosition(long position, long stepId)
		{
			int count = Random.Range(5, 20);
			string text = new string((from s in Enumerable.Repeat<string>("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnoprqstuvwxyz0123456789                        ", count)
			select s[Random.Range(0, s.Length)]).ToArray<char>());
			string[][] array = new string[][]
			{
				new string[]
				{
					"Wololo",
					"Miojo",
					"Kakaroto"
				},
				new string[]
				{
					"Qwert",
					"Asdf",
					"Zxcv"
				},
				new string[]
				{
					"Uiop",
					"Hjkl",
					"Vbnm"
				}
			};
			return new TournamentRankingPosition
			{
				Position = position + 1L,
				Points = (long)Random.Range(0, 9999),
				TeamId = Guid.NewGuid(),
				TeamName = ((position != 0L) ? text : string.Format("[ {0} ] - {1}", stepId, text)),
				TeamTag = text.Substring(0, 3),
				ClassificatoryPoints = (long)((int)Mathf.Max(0f, (float)((2L ^ stepId + 2L) * 25L - (10L * (position + 1L) ^ 2L)))),
				TeamMembersName = array[Random.Range(0, array.Length)],
				TeamIconAssetName = "team_image_meme_" + Random.Range(0, 6).ToString("00")
			};
		}

		private const string RandomCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnoprqstuvwxyz0123456789                        ";
	}
}
