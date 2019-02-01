using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Infra
{
	public class ICachedAsset : GameHubBehaviour
	{
		public void SetBaseAsset(Component baseAsset)
		{
			this.baseAsset = baseAsset;
		}

		protected void ReturnToCache()
		{
			GameHubBehaviour.Hub.Resources.ReturnToPrefabCache(this.baseAsset, this);
		}

		private Component baseAsset;
	}
}
