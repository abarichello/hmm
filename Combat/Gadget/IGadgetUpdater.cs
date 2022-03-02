using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public interface IGadgetUpdater
	{
		void RunGadgetUpdate();

		void DestroyEffect(DestroyEffectMessage evt);

		void ObjectUnspawned(UnspawnEvent evt);

		void ObjectSpawned(SpawnEvent evt);
	}
}
