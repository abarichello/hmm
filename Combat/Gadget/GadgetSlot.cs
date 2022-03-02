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
		GridHighlightGadget,
		EffectBehaviourGadget,
		BombPassGadget,
		EmoteRadialGadget,
		EmoteGadget0,
		EmoteGadget1,
		EmoteGadget2,
		EmoteGadget3,
		[Obsolete("GD Lucas removed this last emote")]
		EmoteGadget4,
		QuickChatMenu,
		QuickChat00,
		QuickChat01,
		QuickChat02,
		QuickChat03,
		QuickChat04,
		QuickChat05,
		QuickChat06,
		QuickChat07,
		QuickChatSystem
	}
}
