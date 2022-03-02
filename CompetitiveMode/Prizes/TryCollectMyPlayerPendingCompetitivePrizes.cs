using System;
using System.Linq;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Store.Business.PlayerInventory;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Prizes
{
	public class TryCollectMyPlayerPendingCompetitivePrizes : ITryCollectMyPlayerPendingCompetitivePrizes
	{
		public TryCollectMyPlayerPendingCompetitivePrizes(ICompetitivePrizesService competitivePrizesService, ILocalPlayerStorage playerStorage, IPlayerInventory playerInventory, ICompetitiveRewardsProvider competitiveRewardsProvider)
		{
			this._competitivePrizesService = competitivePrizesService;
			this._playerStorage = playerStorage;
			this._playerInventory = playerInventory;
			this._competitiveRewardsProvider = competitiveRewardsProvider;
		}

		public IObservable<CompetitivePrizesCollection> TryCollect()
		{
			long playerId = this._playerStorage.Player.PlayerId;
			return Observable.Select<CompetitiveSeasonPrizeCollection, CompetitivePrizesCollection>(Observable.Do<CompetitiveSeasonPrizeCollection>(this._competitivePrizesService.CollectPlayerPendingCompetitivePrizes(playerId), new Action<CompetitiveSeasonPrizeCollection>(this.AddItemsToInventory)), new Func<CompetitiveSeasonPrizeCollection, CompetitivePrizesCollection>(this.CreateCompetitivePrizesCollection));
		}

		private void AddItemsToInventory(CompetitiveSeasonPrizeCollection prizeCollection)
		{
			foreach (Item item in prizeCollection.CollectedItems)
			{
				this._playerInventory.AddItem(item);
			}
		}

		private CompetitivePrizesCollection CreateCompetitivePrizesCollection(CompetitiveSeasonPrizeCollection prizeCollection)
		{
			if (prizeCollection.CollectedItems.Any<Item>())
			{
				CollectedPrize[] array = new CollectedPrize[prizeCollection.CollectedItems.Length];
				CompetitiveReward[] rewards = this._competitiveRewardsProvider.GetRewards((from item in prizeCollection.CollectedItems
				select item.ItemTypeId).ToArray<Guid>());
				for (int i = 0; i < rewards.Length; i++)
				{
					array[i] = new CollectedPrize
					{
						Item = prizeCollection.CollectedItems[i],
						Prize = rewards[i]
					};
				}
				return new CompetitivePrizesCollection
				{
					HasCollected = true,
					CollectedPrizes = array,
					PrizeRank = prizeCollection.PrizeRank.Value,
					Season = prizeCollection.Season
				};
			}
			return new CompetitivePrizesCollection
			{
				HasCollected = false,
				CollectedPrizes = null,
				PrizeRank = this.CreateEmptyCompetitiveRank()
			};
		}

		private CompetitiveRank CreateEmptyCompetitiveRank()
		{
			CompetitiveRank result = default(CompetitiveRank);
			result.Division = 0;
			result.Score = 0;
			result.Subdivision = 0;
			result.TopPlacementPosition = null;
			return result;
		}

		private readonly ICompetitivePrizesService _competitivePrizesService;

		private readonly ICompetitiveRewardsProvider _competitiveRewardsProvider;

		private readonly ILocalPlayerStorage _playerStorage;

		private readonly IPlayerInventory _playerInventory;
	}
}
