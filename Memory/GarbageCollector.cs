using System;
using System.Diagnostics;
using Pocketverse;
using UnityEngine.Scripting;

namespace HeavyMetalMachines.Memory
{
	public static class GarbageCollector
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action OnCollect;

		public static void Enable()
		{
			if (GarbageCollector.GCMode == 1)
			{
				return;
			}
			GarbageCollector.GCMode = 1;
			GarbageCollector.Log.Info("GC has been enabled.");
		}

		public static void Disable()
		{
			GarbageCollector.GCMode = 0;
			GarbageCollector.Log.Info("GC has been disabled.");
		}

		public static void Collect(string reason)
		{
			bool flag = GarbageCollector.GCMode == 0;
			if (flag)
			{
				GarbageCollector.GCMode = 1;
			}
			GarbageCollector.Log.InfoFormat("Before GC Collect. Reason={0}", new object[]
			{
				reason
			});
			GC.Collect();
			GarbageCollector.Log.Info("After GC Collect.");
			if (GarbageCollector.OnCollect != null)
			{
				GarbageCollector.OnCollect();
			}
			if (flag)
			{
				GarbageCollector.GCMode = 0;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GarbageCollector));
	}
}
