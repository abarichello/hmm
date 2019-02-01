using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkDataHolder<T>
	{
		private PerkDataHolder()
		{
		}

		public static void PushData(int eventId, DataHolderSlot slot, T perkData)
		{
			if (PerkDataHolder<T>.Singleton._allCached.ContainsKey(PerkDataHolder<T>.GetKey(eventId, slot)))
			{
				PerkDataHolder<T>.Log.ErrorFormat("EventId:{0} Slot:{1} trying to be pushed a second time! Please, fix that! currentValue{2} newValue{3}", new object[]
				{
					eventId,
					slot,
					PerkDataHolder<T>.Singleton._allCached[PerkDataHolder<T>.GetKey(eventId, slot)],
					perkData
				});
				return;
			}
			PerkDataHolder<T>.Singleton._allCached.Add(PerkDataHolder<T>.GetKey(eventId, slot), perkData);
		}

		public static T PopData(int eventId, DataHolderSlot slot)
		{
			T result = PerkDataHolder<T>.Singleton._allCached[PerkDataHolder<T>.GetKey(eventId, slot)];
			PerkDataHolder<T>.Singleton._allCached.Remove(PerkDataHolder<T>.GetKey(eventId, slot));
			return result;
		}

		private static long GetKey(int eventId, DataHolderSlot slot)
		{
			return (long)eventId + ((long)slot << 32);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkDataHolder<T>));

		private static readonly PerkDataHolder<T> Singleton = new PerkDataHolder<T>();

		private readonly Dictionary<long, T> _allCached = new Dictionary<long, T>();
	}
}
