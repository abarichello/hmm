using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Store.Infrastructure;
using Hoplon.Serialization;
using Hoplon.Time;

namespace HeavyMetalMachines.Boosters.Business
{
	public class GetLocalPlayerXpBooster : IGetLocalPlayerXpBooster
	{
		public GetLocalPlayerXpBooster(ISwordfishInventoryService inventoryService, ICurrentTime currentTime)
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
			return this.GetRemainingTimeSpan(boosterContent.StartDateXp, boosterContent.XpHours);
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

		public string GetBoosterInfo()
		{
			BoostersContent boosterContent = this.GetBoosterContent();
			TimeSpan remainingTimeSpan = this.GetRemainingTimeSpan(boosterContent.StartDateXp, boosterContent.XpHours);
			return string.Format("{0}. {1}.", Language.Get("BOOSTERS_XP_GENERIC_TITLE", TranslationContext.MainMenuGui), this.GetTimeRemainingText(remainingTimeSpan));
		}

		private string GetTimeRemainingText(TimeSpan remainingBoosterTimeSpan)
		{
			int days = remainingBoosterTimeSpan.Days;
			int hours = remainingBoosterTimeSpan.Hours;
			int minutes = remainingBoosterTimeSpan.Minutes;
			string text = Language.Get("BOOSTERS_TIME_REMAINING", TranslationContext.MainMenuGui);
			if (days == 0 && hours == 0 && minutes == 0)
			{
				text += string.Format(" {0} 1 {1}", Language.Get("BOOSTERS_TIME_LESSTHAN", TranslationContext.MainMenuGui), Language.Get("ACTIVE_BOOSTER_DURATIONTYPE_MATCHTIME", TranslationContext.MainMenuGui));
			}
			else
			{
				bool flag = false;
				if (days > 0)
				{
					flag = true;
					text += string.Format(" {0} {1}", days, Language.Get("ACTIVE_BOOSTER_DAY", TranslationContext.MainMenuGui));
				}
				bool flag2 = false;
				if (hours > 0)
				{
					flag2 = true;
					text += string.Format("{0}{1} {2}", (!flag) ? " " : ", ", hours, Language.Get("ACTIVE_BOOSTER_DURATIONTYPE_HOURS", TranslationContext.MainMenuGui));
				}
				if (minutes > 0)
				{
					text += string.Format("{0}{1} {2}", (!flag && !flag2) ? " " : ", ", minutes, Language.Get("ACTIVE_BOOSTER_DURATIONTYPE_MATCHTIME", TranslationContext.MainMenuGui));
				}
			}
			return text;
		}

		private readonly ISwordfishInventoryService _inventoryService;

		private readonly ICurrentTime _currentTime;
	}
}
