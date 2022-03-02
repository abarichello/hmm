using System;
using System.Linq;
using System.Runtime.CompilerServices;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.CompetitiveMode.DataTransferObjects.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.DataTransferObjects.Players;
using HeavyMetalMachines.CompetitiveMode.Extensions;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.Swordfish;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class CompetitiveMatchResultProcessor : ICompetitiveMatchResultProcessor
	{
		public CompetitiveMatchResultProcessor(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<PlayerCompetitiveState[]> Process(CompetitiveMatchResult competitiveMatchResult)
		{
			SerializableCompetitiveMatchResult serializableCompetitiveMatchResult = CompetitiveMatchResultProcessor.ConvertMatchResultToSerializable(competitiveMatchResult);
			string text = serializableCompetitiveMatchResult.ToString();
			CompetitiveMatchResultProcessor.logger.DebugFormat("Process. competitiveMatchResult={0} resultString={1}", new object[]
			{
				competitiveMatchResult,
				text
			});
			IObservable<SerializablePlayerCompetitiveStateCollection> observable = this._customWs.ExecuteAsObservable("UpdateAndGetPlayersCompetitiveRankBasedOnMatchResult", text);
			if (CompetitiveMatchResultProcessor.<>f__mg$cache0 == null)
			{
				CompetitiveMatchResultProcessor.<>f__mg$cache0 = new Func<SerializablePlayerCompetitiveStateCollection, PlayerCompetitiveState[]>(CompetitiveMatchResultProcessor.ConvertResponseToStateArray);
			}
			return Observable.Select<SerializablePlayerCompetitiveStateCollection, PlayerCompetitiveState[]>(observable, CompetitiveMatchResultProcessor.<>f__mg$cache0);
		}

		private static PlayerCompetitiveState[] ConvertResponseToStateArray(SerializablePlayerCompetitiveStateCollection collection)
		{
			CompetitiveMatchResultProcessor.logger.DebugFormat("ConvertResponseToStateArray. collection={0}", new object[]
			{
				collection
			});
			PlayerCompetitiveState[] array = new PlayerCompetitiveState[collection.States.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = collection.States[i].ToPlayerCompetitiveState();
			}
			return array;
		}

		private static SerializableCompetitiveMatchResult ConvertMatchResultToSerializable(CompetitiveMatchResult competitiveMatchResult)
		{
			return new SerializableCompetitiveMatchResult
			{
				MatchId = competitiveMatchResult.MatchId,
				SeasonId = competitiveMatchResult.SeasonId,
				WinnerTeam = CompetitiveMatchResultProcessor.ConvertTeamToSerializable(competitiveMatchResult.WinnerTeam),
				LoserTeam = CompetitiveMatchResultProcessor.ConvertTeamToSerializable(competitiveMatchResult.LoserTeam),
				IsRankedMatch = competitiveMatchResult.IsRankedMatch
			};
		}

		private static SerializableCompetitiveMatchPlayerStateCollection ConvertTeamToSerializable(CompetitiveMatchPlayerState[] states)
		{
			SerializableCompetitiveMatchPlayerStateCollection serializableCompetitiveMatchPlayerStateCollection = new SerializableCompetitiveMatchPlayerStateCollection();
			serializableCompetitiveMatchPlayerStateCollection.PlayerStates = (from state in states
			select new SerializableCompetitiveMatchPlayerState
			{
				Afk = state.Afk,
				PlayerId = state.PlayerId
			}).ToArray<SerializableCompetitiveMatchPlayerState>();
			return serializableCompetitiveMatchPlayerStateCollection;
		}

		private static BitLogger logger = new BitLogger(typeof(CompetitiveMatchResultProcessor));

		private readonly ICustomWS _customWs;

		[CompilerGenerated]
		private static Func<SerializablePlayerCompetitiveStateCollection, PlayerCompetitiveState[]> <>f__mg$cache0;
	}
}
