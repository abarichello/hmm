using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Items.DataTransferObjects.Match;
using HeavyMetalMachines.Store.Infrastructure;
using Hoplon.Logging;
using Hoplon.Serialization;
using Zenject;

namespace HeavyMetalMachines.Match.Infra
{
	public class MatchHistoryProvider : IMatchHistoryProvider
	{
		public MatchHistoryProvider(ILogger<MatchHistoryProvider> logger)
		{
			this._logger = logger;
		}

		public MatchHistoryInventoryBag GetInventoryBag()
		{
			Inventory matchHistoryInventory = this.GetMatchHistoryInventory();
			InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<!0>)matchHistoryInventory.Bag);
			if (inventoryBag == null || string.IsNullOrEmpty(inventoryBag.Content))
			{
				this._logger.WarnFormat("Inventory Bag is null or Empty", new object[0]);
				return new MatchHistoryInventoryBag();
			}
			return (MatchHistoryInventoryBag)((JsonSerializeable<!0>)inventoryBag.Content);
		}

		private Inventory GetMatchHistoryInventory()
		{
			return this._swordfishInventoryService.GetInventoryByKind(5);
		}

		[Inject]
		private ISwordfishInventoryService _swordfishInventoryService;

		private readonly ILogger<MatchHistoryProvider> _logger;
	}
}
