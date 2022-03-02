using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.CompetitiveMode.DataTransferObjects.Players;
using HeavyMetalMachines.CompetitiveMode.DataTransferObjects.Prizes;
using HeavyMetalMachines.CompetitiveMode.DataTransferObjects.Seasons;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.CompetitiveMode.Prizes;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.Items.DataTransferObjects;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Infra
{
	public class SwordfishCompetitivePrizesService : ICompetitivePrizesService
	{
		public SwordfishCompetitivePrizesService(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<CompetitiveSeasonPrizeCollection> CollectPlayerPendingCompetitivePrizes(long playerId)
		{
			return Observable.Select<SerializableCompetitiveSeasonPrizeCollection, CompetitiveSeasonPrizeCollection>(this._customWs.ExecuteAsObservable("CollectPlayerPendingCompetitiveSeasonPrizes", playerId.ToString()), new Func<SerializableCompetitiveSeasonPrizeCollection, CompetitiveSeasonPrizeCollection>(this.ConvertToCompetitiveSeasonPrizeCollection));
		}

		private CompetitiveSeasonPrizeCollection ConvertToCompetitiveSeasonPrizeCollection(SerializableCompetitiveSeasonPrizeCollection collection)
		{
			CompetitiveSeasonPrizeCollection competitiveSeasonPrizeCollection = new CompetitiveSeasonPrizeCollection();
			competitiveSeasonPrizeCollection.Season = SwordfishCompetitivePrizesService.ConvertToCompetitiveSeason(collection.Season);
			CompetitiveSeasonPrizeCollection competitiveSeasonPrizeCollection2 = competitiveSeasonPrizeCollection;
			IEnumerable<SerializableItem> collectedItems = collection.CollectedItems;
			if (SwordfishCompetitivePrizesService.<>f__mg$cache0 == null)
			{
				SwordfishCompetitivePrizesService.<>f__mg$cache0 = new Func<SerializableItem, Item>(SwordfishCompetitivePrizesService.ConvertToItem);
			}
			competitiveSeasonPrizeCollection2.CollectedItems = collectedItems.Select(SwordfishCompetitivePrizesService.<>f__mg$cache0).ToArray<Item>();
			competitiveSeasonPrizeCollection.PrizeRank = SwordfishCompetitivePrizesService.ConvertToCompetitiveRank(collection.PrizeRank);
			return competitiveSeasonPrizeCollection;
		}

		private static CompetitiveSeason ConvertToCompetitiveSeason(SerializableCompetitiveSeason season)
		{
			if (season == null)
			{
				return null;
			}
			CompetitiveSeason competitiveSeason = new CompetitiveSeason();
			competitiveSeason.Id = season.Id;
			competitiveSeason.StartDateTime = season.StartDateTimeUtc;
			competitiveSeason.EndDateTime = season.EndDateTimeUtc;
			competitiveSeason.TopPlayersCount = season.TopPlayersCount;
			competitiveSeason.RequiredMatchesCountToCalibrate = season.RequiredMatchesCountToCalibrate;
			competitiveSeason.RequiredMatchesCountToUnlock = season.RequiredMatchesCountToUnlock;
			competitiveSeason.TopPlayersPrizesItemTypesIds = (from p in season.TopPlayersPrizesItemTypesIds
			select p.ItemTypeId).ToArray<Guid>();
			return competitiveSeason;
		}

		private static Item ConvertToItem(SerializableItem item)
		{
			return new Item
			{
				Id = item.Id,
				ItemTypeId = item.ItemTypeId,
				Bag = item.Bag,
				Quantity = item.Quantity,
				BagVersion = item.BagVersion,
				InventoryId = item.InventoryId
			};
		}

		private static CompetitiveRank? ConvertToCompetitiveRank(SerializableCompetitiveRank rank)
		{
			if (rank == null)
			{
				return null;
			}
			CompetitiveRank value = default(CompetitiveRank);
			value.Division = rank.Division;
			value.Subdivision = rank.Subdivision;
			value.Score = rank.Score;
			value.TopPlacementPosition = rank.TopPlacementPosition;
			return new CompetitiveRank?(value);
		}

		private readonly ICustomWS _customWs;

		[CompilerGenerated]
		private static Func<SerializableItem, Item> <>f__mg$cache0;
	}
}
