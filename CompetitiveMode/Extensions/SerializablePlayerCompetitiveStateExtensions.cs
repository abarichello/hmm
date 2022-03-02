using System;
using HeavyMetalMachines.CompetitiveMode.DataTransferObjects.Players;
using HeavyMetalMachines.CompetitiveMode.Players;

namespace HeavyMetalMachines.CompetitiveMode.Extensions
{
	public static class SerializablePlayerCompetitiveStateExtensions
	{
		public static PlayerCompetitiveState ToPlayerCompetitiveState(this SerializablePlayerCompetitiveState state)
		{
			PlayerCompetitiveState result = default(PlayerCompetitiveState);
			result.PlayerId = state.PlayerId;
			result.Status = state.Status;
			result.CalibrationState = SerializablePlayerCompetitiveStateExtensions.ConvertCalibrationState(state);
			result.Requirements = SerializablePlayerCompetitiveStateExtensions.ConvertRequirements(state);
			result.Rank = SerializablePlayerCompetitiveStateExtensions.ConvertRank(state);
			return result;
		}

		private static PlayerCompetitiveRank ConvertRank(SerializablePlayerCompetitiveState serializableState)
		{
			PlayerCompetitiveRank result = default(PlayerCompetitiveRank);
			result.CurrentRank = SerializablePlayerCompetitiveStateExtensions.ConvertCompetitiveRank(serializableState.CurrentRank);
			result.HighestRank = SerializablePlayerCompetitiveStateExtensions.ConvertCompetitiveRank(serializableState.HighestRank);
			return result;
		}

		private static CompetitiveRank ConvertCompetitiveRank(SerializableCompetitiveRank serializableRank)
		{
			CompetitiveRank result = default(CompetitiveRank);
			result.Score = serializableRank.Score;
			result.Division = serializableRank.Division;
			result.Subdivision = serializableRank.Subdivision;
			result.TopPlacementPosition = serializableRank.TopPlacementPosition;
			return result;
		}

		private static PlayerCompetitiveRequirements ConvertRequirements(SerializablePlayerCompetitiveState serializableState)
		{
			PlayerCompetitiveRequirements result = default(PlayerCompetitiveRequirements);
			result.TotalRequiredMatches = serializableState.LockedTotalRequiredMatches;
			result.TotalMatchesPlayed = serializableState.LockedTotalMatchesPlayed;
			return result;
		}

		private static PlayerCompetitiveCalibrationState ConvertCalibrationState(SerializablePlayerCompetitiveState serializableState)
		{
			PlayerCompetitiveCalibrationState result = default(PlayerCompetitiveCalibrationState);
			result.TotalRequiredMatches = serializableState.CalibrationTotalRequiredMatches;
			result.TotalMatchesPlayed = serializableState.CalibrationTotalMatchesPlayed;
			result.MatchesResults = serializableState.CalibrationMatchesResults;
			return result;
		}
	}
}
