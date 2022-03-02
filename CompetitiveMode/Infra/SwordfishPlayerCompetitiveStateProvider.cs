using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Competitive.DataTransferObjects;
using HeavyMetalMachines.CompetitiveMode.DataTransferObjects.Players;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Infra
{
	public class SwordfishPlayerCompetitiveStateProvider : IPlayerCompetitiveStateProvider
	{
		public SwordfishPlayerCompetitiveStateProvider(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<PlayerCompetitiveState> GetPlayerCompetitiveState(long playerId, long seasonId)
		{
			GetPlayerCompetitiveStateParameters getPlayerCompetitiveStateParameters = new GetPlayerCompetitiveStateParameters
			{
				PlayerId = playerId,
				SeasonId = seasonId
			};
			IObservable<SerializablePlayerCompetitiveState> observable = this._customWs.ExecuteAsObservable("GetPlayerCompetitiveState", (string)getPlayerCompetitiveStateParameters);
			if (SwordfishPlayerCompetitiveStateProvider.<>f__mg$cache0 == null)
			{
				SwordfishPlayerCompetitiveStateProvider.<>f__mg$cache0 = new Func<SerializablePlayerCompetitiveState, PlayerCompetitiveState>(SwordfishPlayerCompetitiveStateProvider.ToPlayerCompetitiveState);
			}
			return Observable.Select<SerializablePlayerCompetitiveState, PlayerCompetitiveState>(observable, SwordfishPlayerCompetitiveStateProvider.<>f__mg$cache0);
		}

		public IObservable<PlayerCompetitiveState[]> GetPlayersCompetitiveState(long[] playersIds, long seasonId)
		{
			GetPlayersCompetitiveStatesParameters getPlayersCompetitiveStatesParameters = new GetPlayersCompetitiveStatesParameters
			{
				PlayersIds = playersIds,
				SeasonId = seasonId
			};
			IObservable<SerializablePlayerCompetitiveStateCollection> observable = this._customWs.ExecuteAsObservable("GetPlayersCompetitiveState", (string)getPlayersCompetitiveStatesParameters);
			if (SwordfishPlayerCompetitiveStateProvider.<>f__mg$cache1 == null)
			{
				SwordfishPlayerCompetitiveStateProvider.<>f__mg$cache1 = new Func<SerializablePlayerCompetitiveStateCollection, PlayerCompetitiveState[]>(SwordfishPlayerCompetitiveStateProvider.ToPlayerCompetitiveStates);
			}
			return Observable.Select<SerializablePlayerCompetitiveStateCollection, PlayerCompetitiveState[]>(observable, SwordfishPlayerCompetitiveStateProvider.<>f__mg$cache1);
		}

		private static PlayerCompetitiveState ToPlayerCompetitiveState(SerializablePlayerCompetitiveState state)
		{
			PlayerCompetitiveState result = default(PlayerCompetitiveState);
			result.PlayerId = state.PlayerId;
			result.Status = state.Status;
			PlayerCompetitiveRank rank = default(PlayerCompetitiveRank);
			rank.CurrentRank = CompetitiveSerializableExtensions.ToModel(state.CurrentRank);
			rank.HighestRank = CompetitiveSerializableExtensions.ToModel(state.HighestRank);
			result.Rank = rank;
			PlayerCompetitiveCalibrationState calibrationState = default(PlayerCompetitiveCalibrationState);
			calibrationState.TotalMatchesPlayed = state.CalibrationTotalMatchesPlayed;
			calibrationState.TotalRequiredMatches = state.CalibrationTotalRequiredMatches;
			calibrationState.MatchesResults = state.CalibrationMatchesResults;
			result.CalibrationState = calibrationState;
			PlayerCompetitiveRequirements requirements = default(PlayerCompetitiveRequirements);
			requirements.TotalMatchesPlayed = state.LockedTotalMatchesPlayed;
			requirements.TotalRequiredMatches = state.LockedTotalRequiredMatches;
			result.Requirements = requirements;
			return result;
		}

		private static PlayerCompetitiveState[] ToPlayerCompetitiveStates(SerializablePlayerCompetitiveStateCollection array)
		{
			IEnumerable<SerializablePlayerCompetitiveState> states = array.States;
			if (SwordfishPlayerCompetitiveStateProvider.<>f__mg$cache2 == null)
			{
				SwordfishPlayerCompetitiveStateProvider.<>f__mg$cache2 = new Func<SerializablePlayerCompetitiveState, PlayerCompetitiveState>(SwordfishPlayerCompetitiveStateProvider.ToPlayerCompetitiveState);
			}
			return states.Select(SwordfishPlayerCompetitiveStateProvider.<>f__mg$cache2).ToArray<PlayerCompetitiveState>();
		}

		private readonly ICustomWS _customWs;

		[CompilerGenerated]
		private static Func<SerializablePlayerCompetitiveState, PlayerCompetitiveState> <>f__mg$cache0;

		[CompilerGenerated]
		private static Func<SerializablePlayerCompetitiveStateCollection, PlayerCompetitiveState[]> <>f__mg$cache1;

		[CompilerGenerated]
		private static Func<SerializablePlayerCompetitiveState, PlayerCompetitiveState> <>f__mg$cache2;
	}
}
