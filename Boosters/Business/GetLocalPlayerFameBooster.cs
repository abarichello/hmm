using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Store.Infrastructure;
using Hoplon.Serialization;
using Hoplon.Time;

namespace HeavyMetalMachines.Boosters.Business
{
	public class GetLocalPlayerFameBooster : IGetLocalPlayerFameBooster
	{
		public GetLocalPlayerFameBooster(ISwordfishInventoryService inventoryService, ICurrentTime currentTime)
		{
			this._inventoryService = inventoryService;
			this._currentTime = currentTime;
		}

		public bool IsActive()
		{
			return this.GetRemaningTime().Seconds > 0;
		}

		public TimeSpan GetRemaningTime()
		{
			BoostersContent boosterContent = this.GetBoosterContent();
			if (boosterContent == null)
			{
				return TimeSpan.Zero;
			}
			return this.GetRemainingTimeSpan(boosterContent.StartDateSc, boosterContent.ScHours);
		}

		private BoostersContent GetBoosterContent()
		{
			Inventory inventoryByKind = this._inventoryService.GetInventoryByKind(9);
			if (inventoryByKind == null)
			{
				return null;
			}
			InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<!0>)inventoryByKind.Bag);
			if (inventoryBag == null)
			{
				return null;
			}
			return (BoostersContent)((JsonSerializeable<!0>)inventoryBag.Content);
		}

		private TimeSpan GetRemainingTimeSpan(long startDate, int hours)
		{
			DateTime dateTime = new DateTime(startDate);
			DateTime d = dateTime.AddHours((double)hours);
			return d - this._currentTime.NowServerUtc();
		}

		private readonly ISwordfishInventoryService _inventoryService;

		private readonly ICurrentTime _currentTime;
	}
}
