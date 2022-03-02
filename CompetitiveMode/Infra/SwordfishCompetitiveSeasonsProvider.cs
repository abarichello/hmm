using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.CompetitiveMode.DataTransferObjects.Seasons;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.Swordfish;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Infra
{
	public class SwordfishCompetitiveSeasonsProvider : ICompetitiveSeasonsProvider
	{
		public SwordfishCompetitiveSeasonsProvider(IRanked rankedService)
		{
			this._rankedService = rankedService;
		}

		public IObservable<CompetitiveSeason> GetCurrentSeason()
		{
			IObservable<RankedSeason> observable = SwordfishObservable.FromSwordfishCall<RankedSeason>(delegate(SwordfishClientApi.ParameterizedCallback<RankedSeason> success, SwordfishClientApi.ErrorCallback error)
			{
				this._rankedService.GetCurrentSeason(null, success, error);
			});
			if (SwordfishCompetitiveSeasonsProvider.<>f__mg$cache0 == null)
			{
				SwordfishCompetitiveSeasonsProvider.<>f__mg$cache0 = new Func<RankedSeason, CompetitiveSeason>(SwordfishCompetitiveSeasonsProvider.TryConvertToCompetitiveSeason);
			}
			return Observable.Select<RankedSeason, CompetitiveSeason>(observable, SwordfishCompetitiveSeasonsProvider.<>f__mg$cache0);
		}

		public IObservable<CompetitiveSeason> GetNextSeason()
		{
			IObservable<RankedSeason> observable = SwordfishObservable.FromSwordfishCall<RankedSeason>(delegate(SwordfishClientApi.ParameterizedCallback<RankedSeason> success, SwordfishClientApi.ErrorCallback error)
			{
				this._rankedService.GetNextSeason(null, success, error);
			});
			if (SwordfishCompetitiveSeasonsProvider.<>f__mg$cache1 == null)
			{
				SwordfishCompetitiveSeasonsProvider.<>f__mg$cache1 = new Func<RankedSeason, CompetitiveSeason>(SwordfishCompetitiveSeasonsProvider.TryConvertToCompetitiveSeason);
			}
			return Observable.Select<RankedSeason, CompetitiveSeason>(observable, SwordfishCompetitiveSeasonsProvider.<>f__mg$cache1);
		}

		private static CompetitiveSeason TryConvertToCompetitiveSeason(RankedSeason rankedSeason)
		{
			if (rankedSeason == null)
			{
				return null;
			}
			RankedSeasonBag rankedSeasonBag = (RankedSeasonBag)((JsonSerializeable<!0>)rankedSeason.Bag);
			CompetitiveSeason competitiveSeason = new CompetitiveSeason();
			competitiveSeason.Id = rankedSeason.Id;
			competitiveSeason.StartDateTime = rankedSeason.Start;
			competitiveSeason.EndDateTime = rankedSeason.End;
			competitiveSeason.TopPlayersCount = rankedSeasonBag.TopPlayersCount;
			competitiveSeason.RequiredMatchesCountToCalibrate = rankedSeasonBag.RequiredMatchesCountToCalibrate;
			competitiveSeason.RequiredMatchesCountToUnlock = rankedSeasonBag.RequiredMatchesCountToUnlock;
			competitiveSeason.TopPlayersPrizesItemTypesIds = (from prize in rankedSeasonBag.TopPlacementPrizesItemTypeIds
			select prize.ItemTypeId).ToArray<Guid>();
			CompetitiveSeason competitiveSeason2 = competitiveSeason;
			IEnumerable<RankedDivision> divisions = rankedSeason.Divisions;
			if (SwordfishCompetitiveSeasonsProvider.<>f__mg$cache2 == null)
			{
				SwordfishCompetitiveSeasonsProvider.<>f__mg$cache2 = new Func<RankedDivision, Division>(SwordfishCompetitiveSeasonsProvider.ConvertToDivision);
			}
			competitiveSeason2.Divisions = divisions.Select(SwordfishCompetitiveSeasonsProvider.<>f__mg$cache2).ToArray<Division>();
			return competitiveSeason;
		}

		private static Division ConvertToDivision(RankedDivision division)
		{
			RankedDivisionBag rankedDivisionBag = (RankedDivisionBag)((JsonSerializeable<!0>)division.Bag);
			Division division2 = new Division();
			division2.NameDraft = division.Name;
			division2.StartingScore = division.LowerBound;
			division2.EndingScore = division.UpperBound;
			division2.PrizesItemTypeIds = (from prize in rankedDivisionBag.Prizes
			select prize.ItemTypeId).ToArray<Guid>();
			Division division3 = division2;
			IEnumerable<RankedSubdivisionBag> subdivisions = rankedDivisionBag.Subdivisions;
			if (SwordfishCompetitiveSeasonsProvider.<>f__mg$cache3 == null)
			{
				SwordfishCompetitiveSeasonsProvider.<>f__mg$cache3 = new Func<RankedSubdivisionBag, Subdivision>(SwordfishCompetitiveSeasonsProvider.ConvertToSubdivision);
			}
			division3.Subdivisions = subdivisions.Select(SwordfishCompetitiveSeasonsProvider.<>f__mg$cache3).ToArray<Subdivision>();
			return division2;
		}

		private static Subdivision ConvertToSubdivision(RankedSubdivisionBag subdivisionBag)
		{
			return new Subdivision
			{
				StartingScore = subdivisionBag.StartingScore,
				EndingScore = subdivisionBag.EndingScore
			};
		}

		private readonly IRanked _rankedService;

		[CompilerGenerated]
		private static Func<RankedSeason, CompetitiveSeason> <>f__mg$cache0;

		[CompilerGenerated]
		private static Func<RankedSeason, CompetitiveSeason> <>f__mg$cache1;

		[CompilerGenerated]
		private static Func<RankedDivision, Division> <>f__mg$cache2;

		[CompilerGenerated]
		private static Func<RankedSubdivisionBag, Subdivision> <>f__mg$cache3;
	}
}
