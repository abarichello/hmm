using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Bomb
{
	public class UnparentOnObjectUnspawn : GameHubBehaviour, IObjectSpawnListener
	{
		public void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.transform.parent = null;
		}

		public void OnObjectSpawned(SpawnEvent evt)
		{
		}
	}
}
