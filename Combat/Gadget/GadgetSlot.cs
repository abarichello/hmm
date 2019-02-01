using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public enum GadgetSlot
	{
		Any = -1,
		None,
		CustomGadget0,
		CustomGadget1,
		CustomGadget2,
		BoostGadget,
		PassiveGadget,
		GenericGadget,
		HPUpgrade,
		EPUpgrade,
		RespawnGadget,
		BombGadget,
		LiftSceneryGadget,
		[Obsolete]
		OBSOLETE_PingMinimapGadget,
		DmgUpgrade,
		OutOfCombatGadget = 18,
		TrailGadget,
		TakeoffGadget,
		KillGadget,
		BombExplosionGadget,
		SprayGadget,
		GridHighlightGadget
	}
}
