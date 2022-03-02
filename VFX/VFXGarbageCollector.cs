using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class VFXGarbageCollector
	{
		private VFXGarbageCollector()
		{
		}

		public static int Collect(Transform drawer)
		{
			if (drawer == null)
			{
				return 0;
			}
			drawer.gameObject.scene.GetRootGameObjects(VFXGarbageCollector.sceneRootsCache);
			int num = 0;
			for (int i = 0; i < VFXGarbageCollector.sceneRootsCache.Count; i++)
			{
				VFXGarbageCollector.sceneRootsCache[i].GetComponentsInChildren<MasterVFX>(VFXGarbageCollector.cache);
				for (int j = 0; j < VFXGarbageCollector.cache.Count; j++)
				{
					VFXGarbageCollector.Log.WarnFormat("Destroying leaked VFX: {0}", new object[]
					{
						VFXGarbageCollector.cache[j].name
					});
					VFXGarbageCollector.cache[j].gameObject.SetActive(false);
					Object.Destroy(VFXGarbageCollector.cache[j].gameObject);
					num++;
				}
				VFXGarbageCollector.cache.Clear();
			}
			VFXGarbageCollector.sceneRootsCache.Clear();
			return num;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(VFXGarbageCollector));

		private static readonly List<MasterVFX> cache = new List<MasterVFX>(50);

		private static readonly List<GameObject> sceneRootsCache = new List<GameObject>(50);
	}
}
