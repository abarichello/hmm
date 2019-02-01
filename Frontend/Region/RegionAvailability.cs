using System;
using System.Collections.Generic;
using ClientAPI.Objects;

namespace HeavyMetalMachines.Frontend.Region
{
	public class RegionAvailability
	{
		public RegionAvailability()
		{
		}

		public RegionAvailability(IEnumerable<RegionServerPeriod> allRegionServerPeriods, DateTime localTime, DateTime serverTime)
		{
			this.Setup(allRegionServerPeriods, localTime, serverTime);
		}

		public void Setup(IEnumerable<RegionServerPeriod> allRegionServerPeriods, DateTime localTime, DateTime serverTime)
		{
			this._regionServerTimeDictionary.Clear();
			foreach (RegionServerPeriod regionServerPeriod in allRegionServerPeriods)
			{
				List<RegionServerPeriod> list;
				if (!this._regionServerTimeDictionary.TryGetValue(regionServerPeriod.RegionNameI18n, out list))
				{
					this._regionServerTimeDictionary[regionServerPeriod.RegionNameI18n] = new List<RegionServerPeriod>();
				}
				this._regionServerTimeDictionary[regionServerPeriod.RegionNameI18n].Add(regionServerPeriod);
			}
			this._timeDiff = serverTime - localTime;
		}

		public RegionAvailability.AvailabilityPeriod GetNextAvailableTime(DateTime currentLocalTime, string regionName)
		{
			RegionAvailability.AvailabilityPeriod result = new RegionAvailability.AvailabilityPeriod
			{
				OpenTime = TimeSpan.Zero,
				CloseTime = TimeSpan.Zero
			};
			List<RegionServerPeriod> regionServerPeriods = this.GetRegionServerPeriods(regionName);
			if (regionServerPeriods == null)
			{
				return result;
			}
			DateTime dateTime = currentLocalTime.ToUniversalTime() + this._timeDiff;
			TimeSpan timeSpan = TimeSpan.MaxValue;
			for (int i = 0; i < regionServerPeriods.Count; i++)
			{
				RegionServerPeriod regionServerPeriod = regionServerPeriods[i];
				if (this.IsInRange(dateTime, regionServerPeriod))
				{
					result.OpenTime = TimeSpan.Zero;
					DateTime weekDateTime = this.GetWeekDateTime(dateTime, regionServerPeriod.EndWeekDay, regionServerPeriod.EndHour.TimeOfDay);
					this.FixDateRange(ref dateTime, ref weekDateTime);
					result.CloseTime = weekDateTime - DateTime.UtcNow;
					return result;
				}
				DateTime weekDateTime2 = this.GetWeekDateTime(dateTime, regionServerPeriod.BeginWeekDay, regionServerPeriod.BeginHour.TimeOfDay);
				this.FixDateRange(ref dateTime, ref weekDateTime2);
				TimeSpan timeSpan2 = weekDateTime2 - dateTime;
				if (timeSpan2 < TimeSpan.Zero)
				{
					throw new Exception("wooooowwww, how come this happend?!!!!");
				}
				if (timeSpan2 <= timeSpan)
				{
					timeSpan = timeSpan2;
					result.OpenTime = timeSpan;
					DateTime weekDateTime3 = this.GetWeekDateTime(dateTime, regionServerPeriod.EndWeekDay, regionServerPeriod.EndHour.TimeOfDay);
					this.FixDateRange(ref weekDateTime2, ref weekDateTime3);
					result.CloseTime = timeSpan + (weekDateTime3 - weekDateTime2);
				}
			}
			return result;
		}

		public Dictionary<DayOfWeek, List<RegionAvailability.AvailabilityPeriod>> GetAvailabilityPeriodsByDate(DateTime currentLocalTime, string regionName)
		{
			List<RegionServerPeriod> regionServerPeriods = this.GetRegionServerPeriods(regionName);
			if (regionServerPeriods == null)
			{
				return null;
			}
			Dictionary<DayOfWeek, List<RegionAvailability.AvailabilityPeriod>> dictionary = new Dictionary<DayOfWeek, List<RegionAvailability.AvailabilityPeriod>>();
			TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(currentLocalTime);
			DateTime srvDateTime = currentLocalTime.ToUniversalTime() + this._timeDiff;
			for (int i = 0; i < regionServerPeriods.Count; i++)
			{
				RegionServerPeriod regionServerPeriod = regionServerPeriods[i];
				DateTime dateTime = this.GetWeekDateTime(srvDateTime, regionServerPeriod.BeginWeekDay, regionServerPeriod.BeginHour.TimeOfDay) + utcOffset;
				DateTime dateTime2 = this.GetWeekDateTime(srvDateTime, regionServerPeriod.EndWeekDay, regionServerPeriod.EndHour.TimeOfDay) + utcOffset;
				this.FixDateRange(ref dateTime, ref dateTime2);
				if (dateTime.DayOfWeek == dateTime2.DayOfWeek)
				{
					if (!dictionary.ContainsKey(dateTime.DayOfWeek))
					{
						dictionary[dateTime.DayOfWeek] = new List<RegionAvailability.AvailabilityPeriod>();
					}
					dictionary[dateTime.DayOfWeek].Add(new RegionAvailability.AvailabilityPeriod
					{
						OpenTime = dateTime.TimeOfDay,
						CloseTime = dateTime2.TimeOfDay
					});
				}
				else
				{
					RegionAvailability.AvailabilityPeriod[] array = new RegionAvailability.AvailabilityPeriod[1 + (dateTime2.Date - dateTime.Date).Days];
					for (int j = 0; j < array.Length; j++)
					{
						array[j].OpenTime = ((j != 0) ? TimeSpan.Zero : dateTime.TimeOfDay);
						array[j].CloseTime = ((j != array.Length - 1) ? new TimeSpan(24, 0, 0) : dateTime2.TimeOfDay);
						DayOfWeek key = (dateTime.DayOfWeek + j) % (DayOfWeek)7;
						if (!dictionary.ContainsKey(key))
						{
							dictionary[key] = new List<RegionAvailability.AvailabilityPeriod>();
						}
						dictionary[key].Add(array[j]);
					}
				}
			}
			return dictionary;
		}

		public bool IsInRange(DateTime currentLocalTime, string regionName)
		{
			DateTime currentTime = currentLocalTime.ToUniversalTime() + this._timeDiff;
			List<RegionServerPeriod> list = this._regionServerTimeDictionary[regionName];
			for (int i = 0; i < list.Count; i++)
			{
				RegionServerPeriod rsp = list[i];
				if (this.IsInRange(currentTime, rsp))
				{
					return true;
				}
			}
			return false;
		}

		private List<RegionServerPeriod> GetRegionServerPeriods(string regionName)
		{
			List<RegionServerPeriod> result = null;
			if (this._regionServerTimeDictionary != null)
			{
				this._regionServerTimeDictionary.TryGetValue(regionName, out result);
			}
			return result;
		}

		private DateTime GetWeekDateTime(DateTime srvDateTime, DayOfWeek dayOfWeek, TimeSpan hour)
		{
			return srvDateTime.Date + new TimeSpan(dayOfWeek - srvDateTime.DayOfWeek, hour.Hours, hour.Minutes, 0);
		}

		private bool IsInRange(DateTime currentTime, RegionServerPeriod rsp)
		{
			DateTime weekDateTime = this.GetWeekDateTime(currentTime, rsp.BeginWeekDay, rsp.BeginHour.TimeOfDay);
			DateTime weekDateTime2 = this.GetWeekDateTime(currentTime, rsp.EndWeekDay, rsp.EndHour.TimeOfDay);
			this.FixDateRange(ref weekDateTime, ref weekDateTime2);
			return currentTime >= weekDateTime && currentTime <= weekDateTime2;
		}

		private void FixDateRange(ref DateTime beginDate, ref DateTime endDate)
		{
			if (endDate < beginDate)
			{
				endDate = endDate.AddDays(7.0);
			}
		}

		private readonly Dictionary<string, List<RegionServerPeriod>> _regionServerTimeDictionary = new Dictionary<string, List<RegionServerPeriod>>();

		private TimeSpan _timeDiff;

		public struct AvailabilityPeriod
		{
			public override string ToString()
			{
				return string.Format("OpenTime: {0}, CloseTime: {1}", this.OpenTime, this.CloseTime);
			}

			public TimeSpan OpenTime;

			public TimeSpan CloseTime;
		}
	}
}
